using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
//using Newtonsoft.Json;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unify2D.Core;
using UnifyCore;

namespace Unify2D.Builder
{
    internal class GameBuilder
    {
        const string AssetsPath = "./Assets";
        const string TemplatePath = "./GameTemplate";
        const string RuntimesFolderPath = "./runtimes";
        const string JsonFolderSceneName = "SceneJson.json";
        string AssetsPathFull => CoreTools.CombinePath(_editor.ProjectPath, AssetsPath);
        string BuildPathFull => CoreTools.CombinePath(_editor.ProjectPath, "./Build");
        string RuntimesFolderPathFull => CoreTools.CombinePath(TemplatePath, RuntimesFolderPath);

        const string AssemblyName = "GameAssembly";
        const string ExeName = "UnifyGame.exe";

        GameCore _core;
        GameEditor _editor;

        public bool Build(GameCore core, GameEditor editor)
        {
            _core = core;
            _editor = editor;

            if (Directory.Exists(BuildPathFull) == false)
                Directory.CreateDirectory(BuildPathFull);

            if (Directory.Exists(CoreTools.CombinePath(BuildPathFull, AssetsPath)) == false)
                Directory.CreateDirectory(CoreTools.CombinePath(BuildPathFull, AssetsPath));

            if (Directory.Exists(TemplatePath) == false)
            {
                Console.WriteLine($"There's no template at the given path {TemplatePath}");

                return false;
            }

            foreach (string file in Directory.GetFiles(TemplatePath))
            {
                string fileName = Path.GetFileName(file);
                string newPath = CoreTools.CombinePath(BuildPathFull, fileName);
                File.Copy(file, newPath, true);
            }

            Directory.CreateDirectory(BuildPathFull + RuntimesFolderPath);
            if (Directory.Exists(RuntimesFolderPathFull))
            {
                string newPath = CoreTools.CombinePath(BuildPathFull, RuntimesFolderPath);
                CopyFilesRecursively(RuntimesFolderPathFull, newPath);
            }


            Directory.CreateDirectory(BuildPathFull + AssetsPath);
            if (Directory.Exists(AssetsPathFull))
            {
                string newPath = CoreTools.CombinePath(BuildPathFull, AssetsPath);
                CopyFilesRecursively(AssetsPathFull, newPath);
            }

            SaveAllScene();

            CreateDll();

            return true;
        }

        private void SaveAllScene()
        {
            SceneManager.Instance.SaveCurrentScene();

            if (Directory.Exists(_editor.AssetsPath))
            {
                List<SceneInfo> listSceneToJson = new List<SceneInfo>();
                try
                {
                    // Récupérer tous les fichiers .scene dans le répertoire et ses sous-répertoires
                    foreach (string item in Directory.GetFiles(_editor.AssetsPath, "*.scene", SearchOption.AllDirectories))
                    {
                        string name = item.Substring(item.LastIndexOf('\\') + 1);
                        string path = item.Replace(_editor.ProjectPath, "");

                        SceneInfo scene = new SceneInfo(name, path);
                        listSceneToJson.Add(scene);
                    }

                    string json = JsonSerializer.Serialize(listSceneToJson);
                    string pathJson = CoreTools.CombinePath(BuildPathFull, JsonFolderSceneName);
                    File.WriteAllText(pathJson, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Une erreur s'est produite : " + ex.Message);
                }
            }
        }
        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(newPath) == ".cs" || Path.GetExtension(newPath) == ".cs.meta") continue;

                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        void CreateDll()
        {
            List<SyntaxTree> syntaxes = new List<SyntaxTree>();

            if (Directory.Exists(_editor.AssetsPath))
            {
                foreach (string item in Directory.GetFiles(_editor.AssetsPath, "*.cs"))
                {
                    string content = File.ReadAllText(item);
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(content);
                    syntaxes.Add(syntaxTree);

                }
            }

          
            List<MetadataReference> references = new();

            var libs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);

            foreach (var r in libs)
            {
                references.Add(MetadataReference.CreateFromFile(r));
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
            AssemblyName,
            syntaxTrees: syntaxes,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            string dllPath = CoreTools.CombinePath(BuildPathFull, $"{AssemblyName}.dll");

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

        public void StartBuild()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = CoreTools.CombinePath(BuildPathFull, ExeName);
            startInfo.WorkingDirectory = BuildPathFull;

            Process.Start(startInfo);
        }
    }
}
