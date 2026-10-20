using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using Unify2D.Core;
using UnifyCore;

namespace Unify2D.Scripting
{
    public class Scripting
    {
        AssemblyLoadContext _context;
        List<Type> _types = new();
        Dictionary<string, List<Type>> _scriptTypes = new(StringComparer.OrdinalIgnoreCase);
        GameEditor _editor;
        FileSystemWatcher _watcher;
        string _watchedPath;
        long _reloadAt;
        Dictionary<string, string> _sources;
        byte[] _assemblyImage;
        string _fingerprint;
        public bool HasCompilationErrors { get; private set; }
        internal string LastError { get; private set; }
        const int ReloadDelayMilliseconds = 350;
        public event Action SourcesChanged;

        public void RequestReload()
        {
            Interlocked.Exchange(ref _reloadAt, Environment.TickCount64 + ReloadDelayMilliseconds);
        }

        // File-system callbacks only schedule work. Compilation and scene changes run here.
        public void Update()
        {
            EnsureWatcher();
            long due = Interlocked.Read(ref _reloadAt);
            if (due != 0 && Environment.TickCount64 >= due &&
                Interlocked.CompareExchange(ref _reloadAt, 0, due) == due)
                Reload();
        }

        private void EnsureWatcher()
        {
            string path = _editor.AssetsPath;
            path = string.IsNullOrEmpty(path) ? null : Path.GetFullPath(path);
            if (string.Equals(path, _watchedPath, StringComparison.OrdinalIgnoreCase) &&
                (_watcher != null || path == null || !Directory.Exists(path))) return;

            _watcher?.Dispose();
            _watcher = null;
            _watchedPath = path;
            _sources = null;
            if (path == null || !Directory.Exists(path)) return;

            _watcher = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName |
                    NotifyFilters.LastWrite | NotifyFilters.Size
            };
            _watcher.Changed += (_, e) =>
            {
                if (string.Equals(Path.GetExtension(e.FullPath), ".cs", StringComparison.OrdinalIgnoreCase) ||
                    e.FullPath.EndsWith(".cs.meta", StringComparison.OrdinalIgnoreCase))
                    RequestReload();
            };
            // Include directory moves and atomic saves through temporary files.
            _watcher.Created += (_, _) => RequestReload();
            _watcher.Deleted += (_, _) => RequestReload();
            _watcher.Renamed += (_, _) => RequestReload();
            _watcher.Error += (_, _) => RequestReload();
            _watcher.EnableRaisingEvents = true;
            RequestReload();
        }

        public void Reload()
        {
            var previousCore = GameCore.Current;
            var editingCore = _editor?.PlayToolbox?.EditingCore;
            if (editingCore != null) GameCore.SetCurrent(editingCore);
            try
            {
                if (_types.Count == 0)
                    _types.AddRange(typeof(Component).Assembly.GetTypes().Where(IsComponentType));
                try
                {
                    LoadScripts();
                }
                catch (IOException)
                {
                    // External editors may still hold a file or be replacing it atomically.
                    RequestReload();
                }
            }
            finally { GameCore.SetCurrent(previousCore); }
        }

        private static bool IsComponentType(Type type) =>
            type.IsSubclassOf(typeof(Component)) && !type.IsAbstract &&
            !type.ContainsGenericParameters && type.GetConstructor(Type.EmptyTypes) != null;

        internal bool LastLoadUsedCache { get; private set; }

        private void LoadScripts()
        {
            var sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(_editor.AssetsPath)) return;
            foreach (var path in Directory.GetFiles(_editor.AssetsPath, "*.cs", SearchOption.AllDirectories))
                sources[Path.GetFullPath(path)] = File.ReadAllText(path);

            LastLoadUsedCache = false;
            string fingerprint;
            try { fingerprint = ScriptAssemblyCache.Fingerprint(sources); }
            catch (InvalidDataException error)
            {
                HasCompilationErrors = true;
                LastError = error.Message;
                _fingerprint = null;
                Debug.LogError(error.Message);
                return;
            }
            if (_sources != null && fingerprint == _fingerprint) return;
            _fingerprint = fingerprint;
            string cachePath = ScriptAssemblyCache.GetPath(_editor.AssetsPath);
            var cached = ScriptAssemblyCache.Read(cachePath, fingerprint, sources);
            if (cached != null)
            {
                try
                {
                    ActivateAssembly(cached.Image, cached.TypesBySource);
                    HasCompilationErrors = false;
                    LastError = null;
                    _sources = sources;
                    LastLoadUsedCache = true;
                    SourcesChanged?.Invoke();
                    return;
                }
                catch (BadImageFormatException) { }
                catch (System.IO.FileLoadException) { }
                catch (TypeLoadException) { }
                catch (ReflectionTypeLoadException) { }
            }

            var syntaxes = sources.Select(pair => ScriptCompilationDefaults.Parse(pair.Value, pair.Key)).ToList();
            var compilation = CSharpCompilation.Create(
                "GameAssembly",
                syntaxTrees: ScriptCompilationDefaults.WithImports(syntaxes),
                references: ScriptAssemblyCache.References,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var ruleErrors = ScriptComponentRules.Validate(compilation);
            if (ruleErrors.Count > 0)
            {
                HasCompilationErrors = true;
                LastError = string.Join("\n", ruleErrors.Select(diagnostic => diagnostic.ToString()));
                _sources = sources;
                SourcesChanged?.Invoke();
                foreach (var diagnostic in ruleErrors) Debug.LogError(diagnostic.ToString());
                return;
            }
            compilation = ScriptMetadata.AddIdentities(compilation, ScriptMetadata.ReadGuids(sources.Keys));
            EmitResult result = compilation.Emit(ms);
            HasCompilationErrors = !result.Success;
            LastError = result.Success ? null : string.Join("\n", result.Diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error));
            _sources = sources;
            SourcesChanged?.Invoke();
            if (!result.Success)
            {
                foreach (var diagnostic in result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error))
                    Debug.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                return;
            }

            var namesBySource = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (var syntax in syntaxes)
            {
                var model = compilation.GetSemanticModel(syntax);
                namesBySource[syntax.FilePath] = syntax.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Select(declaration => model.GetDeclaredSymbol(declaration)?.ToDisplayString())
                    .Where(name => name != null).ToArray();
            }
            byte[] image = ms.ToArray();
            ActivateAssembly(image, namesBySource);
            ScriptAssemblyCache.Write(cachePath, new ScriptAssemblyCache.Entry {
                Fingerprint = fingerprint, Image = image, TypesBySource = namesBySource
            });
        }

        private void ActivateAssembly(byte[] image, Dictionary<string, string[]> namesBySource)
        {
            var context = new AssemblyLoadContext(null, true);
            Assembly assembly;
            List<Type> scriptTypes;
            var typesBySource = new Dictionary<string, List<Type>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var ms = new MemoryStream(image);
                assembly = context.LoadFromStream(ms);
                scriptTypes = assembly.GetTypes().Where(IsComponentType).ToList();
                foreach (var pair in namesBySource)
                {
                    var names = pair.Value.ToHashSet();
                    typesBySource[pair.Key] = scriptTypes
                        .Where(type => names.Contains(type.FullName.Replace('+', '.'))).ToList();
                }
            }
            catch
            {
                context.Unload();
                throw;
            }

            var previousContext = _context;
            var previousScriptTypes = _scriptTypes;
            _context = context;
            _assemblyImage = image;
            _scriptTypes = typesBySource;
            _types.Clear();
            _types.AddRange(typeof(Component).Assembly.GetTypes().Where(IsComponentType));
            _types.AddRange(scriptTypes);
            GuidTypeBinder.Instance.SetScriptAssembly(assembly);
            if (previousContext != null)
            {
                try { ReplaceComponents(GetRenamedTypes(previousScriptTypes)); }
                finally { previousContext.Unload(); }
            }
        }

        private Dictionary<Type, Type> GetRenamedTypes(Dictionary<string, List<Type>> previousScriptTypes)
        {
            var renamedTypes = new Dictionary<Type, Type>();
            foreach (var oldType in previousScriptTypes.Values.SelectMany(types => types).Distinct())
            {
                var identity = oldType.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                    .FirstOrDefault(attribute => attribute.Key.StartsWith(GuidTypeBinder.ScriptGuidPrefix, StringComparison.Ordinal) &&
                        attribute.Value == oldType.FullName);
                if (identity != null && GuidTypeBinder.Instance.TryGetScriptType(
                    identity.Key.Substring(GuidTypeBinder.ScriptGuidPrefix.Length), out var newType))
                    renamedTypes[oldType] = newType;
            }
            return renamedTypes;
        }

        private void ReplaceComponents(Dictionary<Type, Type> renamedTypes)
        {
            if (SceneManager.Instance.CurrentScene == null) return;
            foreach (var go in SceneManager.Instance.CurrentScene.GameObjectsWithChildren.ToList())
            {
                foreach (var oldComponent in go.Components.ToList())
                {
                    var oldType = oldComponent.GetType();
                    var newType = GetNewType(oldType);
                    if (newType == null)
                        renamedTypes.TryGetValue(oldType, out newType);
                    if (newType == null || newType == oldType)
                    {
                        continue;
                    }

                    Component newComp;
                    try
                    {
                        newComp = (Component)Activator.CreateInstance(newType);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"Cannot reload {newType.FullName}: {exception.GetBaseException().Message}");
                        continue;
                    }

                    var oldFields = oldComponent.GetType().GetFields(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    foreach (var oldField in oldFields)
                    {
                        var newFields = newComp.GetType().GetFields(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        foreach (var newField in newFields)
                        {
                            if (newField.Name == oldField.Name && newField.FieldType == oldField.FieldType && !newField.IsInitOnly)
                                newField.SetValue(newComp, oldField.GetValue(oldComponent));
                        }

                    }

                    go.RemoveComponent(oldComponent);
                    go.AddComponent(newComp);
                }
            }
        }

        public void Load(GameEditor editor)
        {
            _editor = editor;
            EnsureWatcher();
            Reload();

        }

        public void Build()
        {

        }

        public void Unload()
        {
            GuidTypeBinder.Instance.SetScriptAssembly(null);
            _watcher?.Dispose();
            _watcher = null;
            _watchedPath = null;
            Interlocked.Exchange(ref _reloadAt, 0);
            _sources = null;
            _assemblyImage = null;
            _fingerprint = null;
            _scriptTypes.Clear();
            _types.Clear();
            _context?.Unload();
            _context = null;
        }

        public List<Type> GetTypes()
        {
            if (GameCore.Current?.SerializationBinder is Runtime.PlayScriptAssembly playScripts)
                return typeof(Component).Assembly.GetTypes().Concat(playScripts.Types).Where(IsComponentType).ToList();
            return _types;
        }

        internal KeyValuePair<string, string>[] GetSourceSnapshot() =>
            _sources?.ToArray() ?? Array.Empty<KeyValuePair<string, string>>();

        internal Runtime.PlayScriptAssembly CreatePlayAssembly()
        {
            Reload();
            if (HasCompilationErrors || _assemblyImage == null)
                throw new InvalidOperationException("Fix script compilation errors before starting Play.");
            return new Runtime.PlayScriptAssembly(_assemblyImage);
        }

        public bool TryAddComponent(GameObject target, Type type)
        {
            if (target == null || type == null || !IsComponentType(type)) return false;
            try
            {
                if (GameCore.Current?.IsPlaySession == true)
                    type = GameCore.Current.SerializationBinder.BindToType(type.Assembly.GetName().Name, type.FullName);
                target.AddComponent((Component)Activator.CreateInstance(type));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Cannot add {type.FullName}: {exception.GetBaseException().Message}");
                return false;
            }
        }

        public bool TryAddScript(GameObject target, string path)
        {
            if (target == null) return false;
            path = Path.GetFullPath(path);
            if (!_scriptTypes.TryGetValue(path, out var types))
            {
                Reload();
                _scriptTypes.TryGetValue(path, out types);
            }
            if (types == null || types.Count != 1)
            {
                Debug.LogError($"Cannot attach {path}: expected one concrete Component with a parameterless constructor. Check compilation errors or use Add Component to choose a class.");
                return false;
            }
            return TryAddComponent(target, types[0]);
        }

        Type GetNewType(Type oldType)
        {
            foreach (var type in _types)
            {
                if (type.ToString() == oldType.ToString())
                {
                    return type;
                }
            }

            return null;
        }

    }
}
