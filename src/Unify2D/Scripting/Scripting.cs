using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Unify2D.Core;

namespace Unify2D.Scripting
{
    public class Scripting
    {
        AssemblyLoadContext _context;
        List<Type> _types = new();
        GameEditor _editor;

        public void Reload()
        {
            _types.Clear();

            if (_context != null)
            {
                _context.Unloading += ContextUnloaded;
                _context.Unload();
                _context = null;
            }
            else
            {
                LoadScripts();
            }
        }

        private void LoadScripts()
        {
            List<SyntaxTree> syntaxes = new List<SyntaxTree>();

            if (Directory.Exists(_editor.AssetsPath) == false)
                return;

            foreach (var item in Directory.GetFiles(_editor.AssetsPath, "*.cs"))
            {
                string content = File.ReadAllText(item);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(content);
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

            bool success = true;

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    success = false;
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

                    _context = new AssemblyLoadContext(null, true);
                    Assembly assembly = _context.LoadFromStream(ms);

                    _types.AddRange(typeof(Core.Component).Assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(Core.Component)) && type.IsAbstract == false));
                    _types.AddRange(assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(Core.Component)) && type.IsAbstract == false));
                }
            }
        }

        private void ContextUnloaded(AssemblyLoadContext obj)
        {
            Console.WriteLine("context unload");
            for (int i = 0; (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            LoadScripts();

            ReplaceComponents();
        }

        private void ReplaceComponents()
        {
            foreach (var go in SceneManager.Instance.CurrentScene.GameObjects)
            {
                List<Component> newComponents = new List<Component>();
                foreach (var oldComponent in go.Components)
                {
                    var newComp = Activator.CreateInstance(GetNewType(oldComponent.GetType())) as Component;
                    newComponents.Add(newComp);

                    var oldFields = oldComponent.GetType().GetFields(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    foreach (var oldField in oldFields)
                    {
                        var newFields = newComp.GetType().GetFields(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        foreach (var newField in newFields)
                        {
                            if (newField.Name == oldField.Name)
                                newField.SetValue(newComp, oldField.GetValue(oldComponent));
                        }

                    }

                }

                go.ClearComponents();

                foreach (var item in newComponents)
                {
                    go.AddComponent(item);
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
            _context.Unload();
        }

        public List<Type> GetTypes()
        {
            return _types;
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
