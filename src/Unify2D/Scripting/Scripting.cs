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

        public void Reload()
        {
            if (_types.Count == 0)
                _types.AddRange(typeof(Component).Assembly.GetTypes().Where(IsComponentType));
            LoadScripts();
        }

        private static bool IsComponentType(Type type) =>
            type.IsSubclassOf(typeof(Component)) && !type.IsAbstract &&
            !type.ContainsGenericParameters && type.GetConstructor(Type.EmptyTypes) != null;

        private void LoadScripts()
        {
            List<SyntaxTree> syntaxes = new List<SyntaxTree>();

            if (Directory.Exists(_editor.AssetsPath) == false)
                return;

            foreach (var item in Directory.GetFiles(_editor.AssetsPath, "*.cs", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(item);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(content, path: Path.GetFullPath(item));
                syntaxes.Add(syntaxTree);
            }

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
                    _scriptTypes.Clear();
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
                        ReplaceComponents();
                        previousContext.Unload();
                    }
                }
            }
        }

        private void ReplaceComponents()
        {
            if (SceneManager.Instance.CurrentScene == null) return;
            foreach (var go in SceneManager.Instance.CurrentScene.GameObjectsWithChildren.ToList())
            {
                foreach (var oldComponent in go.Components.ToList())
                {
                    var oldType = oldComponent.GetType();
                    var newType = GetNewType(oldType);
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
            Reload();

        }

        public void Build()
        {

        }

        public void Unload()
        {
            _context?.Unload();
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
