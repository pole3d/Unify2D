using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Builder
{
    internal class GameBuilder
    {
        const string AssetsPath = "./Assets";
        const string TemplatePath = "./GameTemplate";
        string AssetsPathFull  => ToolsEditor.CombinePath(_editor.ProjectPath, AssetsPath);
        string BuildPathFull => ToolsEditor.CombinePath(_editor.ProjectPath,  "./Build");
        const string ExeName = "UnifyGame.exe";

        GameCore _core;
        GameEditor _editor;

        public void Build( GameCore core , GameEditor editor)
        {
            _core = core;
            _editor = editor;

            if (Directory.Exists(BuildPathFull) == false)
                Directory.CreateDirectory(BuildPathFull);

            if (Directory.Exists(ToolsEditor.CombinePath(BuildPathFull,AssetsPath)) == false)
                Directory.CreateDirectory(ToolsEditor.CombinePath(BuildPathFull, AssetsPath));

            if ( Directory.Exists(TemplatePath) == false )
            {
                return;
            }

            foreach (var file in Directory.GetFiles(TemplatePath))
            {
                string fileName = Path.GetFileName(file);
                string newPath = ToolsEditor.CombinePath  ( BuildPathFull , fileName );
                File.Copy(file, newPath, true);
            }

            Directory.CreateDirectory(BuildPathFull + AssetsPath);

            foreach (var file in Directory.GetFiles(AssetsPathFull))
            {
                string fileName = Path.GetFileName(file);
                string newPath = ToolsEditor.CombinePath(ToolsEditor.CombinePath(BuildPathFull, AssetsPath)   , fileName);

                if (File.Exists(newPath))
                    File.Delete(newPath);

                var sourceFile = new FileInfo(file);
                sourceFile.CopyTo(newPath, true);

            }       

            Save();

            CreateDll();
        }

        void CreateDll()
        {
            List<SyntaxTree> syntaxes = new List<SyntaxTree>();

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

            string dllPath = ToolsEditor.CombinePath(BuildPathFull, "GameAssembly.dll");

            EmitResult result = compilation.Emit(dllPath);

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

            }
        }
    

        void Save()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            string text = JsonConvert.SerializeObject(_core.GameObjects, settings);

            File.WriteAllText(ToolsEditor.CombinePath(BuildPathFull,"test.scene"), text);
        }

        public void StartBuild()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = ToolsEditor.CombinePath(BuildPathFull, ExeName);
            startInfo.WorkingDirectory = BuildPathFull;

            Process.Start(startInfo);
        }
    }
}
