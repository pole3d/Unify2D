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
                if (string.Equals(Path.GetExtension(e.FullPath), ".cs", StringComparison.OrdinalIgnoreCase))
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

        private static bool IsComponentType(Type type) =>
            type.IsSubclassOf(typeof(Component)) && !type.IsAbstract &&
            !type.ContainsGenericParameters && type.GetConstructor(Type.EmptyTypes) != null;

        private void LoadScripts()
        {
            List<SyntaxTree> syntaxes = new List<SyntaxTree>();
            var sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(_editor.AssetsPath) == false)
                return;

            foreach (var item in Directory.GetFiles(_editor.AssetsPath, "*.cs", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(item);
                sources[Path.GetFullPath(item)] = content;
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(content, path: Path.GetFullPath(item));
                syntaxes.Add(syntaxTree);
            }

            if (_sources != null && sources.Count == _sources.Count &&
                sources.All(pair => _sources.TryGetValue(pair.Key, out var text) && text == pair.Value))
                return;

            string assemblyName = "GameAssembly";
            List<MetadataReference> references = new();

            foreach (var r in ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator))
            {
                references.Add(MetadataReference.CreateFromFile(r));
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: syntaxes,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

          //  bool success = true;

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                _sources = sources;
                SourcesChanged?.Invoke();

                if (!result.Success)
                {
                    //success = false;
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Debug.LogError($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    var previousContext = _context;
                    _context = new AssemblyLoadContext(null, true);
                    Assembly assembly = _context.LoadFromStream(ms);

                    _types.Clear();
                    _types.AddRange(typeof(Component).Assembly.GetTypes().Where(IsComponentType));
                    var scriptTypes = assembly.GetTypes().Where(IsComponentType).ToList();
                    _types.AddRange(scriptTypes);
                    var previousScriptTypes = _scriptTypes;
                    _scriptTypes = new Dictionary<string, List<Type>>(StringComparer.OrdinalIgnoreCase);
                    foreach (var syntax in syntaxes)
                    {
                        var model = compilation.GetSemanticModel(syntax);
                        var names = syntax.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                            .Select(declaration => model.GetDeclaredSymbol(declaration)?.ToDisplayString())
                            .ToHashSet();
                        _scriptTypes[syntax.FilePath] = scriptTypes
                            .Where(type => names.Contains(type.FullName.Replace('+', '.'))).ToList();
                    }
                    if (previousContext != null)
                    {
                        ReplaceComponents(GetRenamedTypes(previousScriptTypes));
                        previousContext.Unload();
                    }
                }
            }
        }

        private Dictionary<Type, Type> GetRenamedTypes(Dictionary<string, List<Type>> previousScriptTypes)
        {
            var renamedTypes = new Dictionary<Type, Type>();
            var previousNames = previousScriptTypes.Values.SelectMany(types => types)
                .Select(type => type.FullName).ToHashSet();
            var currentNames = _types.Select(type => type.FullName).ToHashSet();

            foreach (var source in previousScriptTypes)
            {
                if (!_scriptTypes.TryGetValue(source.Key, out var currentTypes)) continue;

                var removed = source.Value.Where(type => !currentNames.Contains(type.FullName)).ToList();
                var added = currentTypes.Where(type => !previousNames.Contains(type.FullName)).ToList();
                // A unique replacement in the same source file identifies a renamed class.
                // Never guess between several removed or added component classes.
                if (removed.Count == 1 && added.Count == 1)
                    renamedTypes[removed[0]] = added[0];
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
            _watcher?.Dispose();
            _watcher = null;
            _watchedPath = null;
            Interlocked.Exchange(ref _reloadAt, 0);
            _sources = null;
            _scriptTypes.Clear();
            _types.Clear();
            _context?.Unload();
            _context = null;
        }

        public List<Type> GetTypes()
        {
            return _types;
        }

        public bool TryAddComponent(GameObject target, Type type)
        {
            if (target == null || type == null || !IsComponentType(type)) return false;
            try
            {
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
