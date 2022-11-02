using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Toolbox;

namespace Unify2D.Scripting
{
    public class Scripting
    {
        AssemblyLoadContext _context;
        List<Type> _types = new();
        GameEditor _editor;

        public void Load(GameEditor editor )
        {
            _editor = editor;

            List<SyntaxTree> syntaxes = new List<SyntaxTree>();

            foreach (var item in Directory.GetFiles( editor.AssetsPath,"*.cs" ))
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

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    _context = new AssemblyLoadContext(null, true);
                    Assembly assembly = _context.LoadFromStream(ms);

                    _types.AddRange(typeof(Core.Component).Assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(Core.Component))));
                    _types.AddRange(assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(typeof(Core.Component))));

                }
            }
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

    }
}
