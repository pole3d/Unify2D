using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unify2D.Core;

namespace Unify2D.Scripting
{
    public class Scripting
    {
        List<Type> _types = new();
        
        private readonly string _programFilesPath = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
        private readonly string _cmakePath;
        
        private bool _isAssemblyLoaded = false;

        private ScriptingBridge _scriptingBridge;

        public Scripting()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(
                $@"{_programFilesPath}\Microsoft Visual Studio\Installer\vswhere.exe",
                "-find **/cmake.exe"
            );

            startInfo.RedirectStandardOutput = true;

            Process process = Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Could not start VSWhere");
            }
            
            process.WaitForExit();

            using StreamReader reader = process.StandardOutput;
            _cmakePath = reader.ReadLine();

            _scriptingBridge = new ScriptingBridge();
            _scriptingBridge.Open(Path.Combine(Directory.GetCurrentDirectory(), "UnifyCore.dll"));
        }

        public void Reload()
        {
            _scriptingBridge.Reload();
            _scriptingBridge.InstantiateClass("Hello");
            
            FetchTypes();
        }

        public void LoadDll()
        {
            FetchTypes();
            
            CreateBuildFiles();

            string assemblyPath = $@"{GameEditor.Instance.ProjectPath}\build\Debug\GameAssembly.dll";
            if (!File.Exists(assemblyPath))
            {
                return;
            }
            
            _scriptingBridge.Open(assemblyPath);
            
            // _types.AddRange(assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Component)) && type.IsAbstract == false));
            
            // ReplaceComponents();
        }
        
        // private void ReplaceComponents()
        // {
        //     foreach (GameObject go in SceneManager.Instance.CurrentScene.GameObjects)
        //     {
        //         List<Component> newComponents = new List<Component>();
        //         foreach (Component oldComponent in go.Components)
        //         {
        //             Component newComp = Activator.CreateInstance(GetNewType(oldComponent.GetType())) as Component;
        //             newComponents.Add(newComp);
        //
        //             FieldInfo[] oldFields = oldComponent.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //     
        //             foreach (FieldInfo oldField in oldFields)
        //             {
        //                 FieldInfo[] newFields = newComp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //     
        //                 foreach (FieldInfo newField in newFields)
        //                 {
        //                     if (newField.Name == oldField.Name)
        //                     {
        //                         newField.SetValue(newComp, oldField.GetValue(oldComponent));
        //                     }
        //                 }
        //             }
        //         }
        //     
        //         go.ClearComponents();
        //     
        //         foreach (Component item in newComponents)
        //         {
        //             go.AddComponent(item);
        //         }
        //     }
        // }

        // public void Load(GameEditor editor)
        // {
            // _editor = editor;
            // Reload();
        // }

        private void CreateBuildFiles()
        {
            string buildDirectory = $@"{GameEditor.Instance.ProjectPath}\build";
            if (File.Exists(buildDirectory))
            {
                return;
            }

            Directory.CreateDirectory(buildDirectory);
            ProcessStartInfo startInfo = new ProcessStartInfo(
                _cmakePath,
                ".."
            );

            startInfo.WorkingDirectory = buildDirectory;

            Process process = Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Could not start CMake");
            }

            process.WaitForExit();

            Build();
        }
        
        public void Build()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(
                _cmakePath,
                "--build ."
            );

            startInfo.WorkingDirectory = $@"{GameEditor.Instance.ProjectPath}\build";
            
            Process process = Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Could not start CMake");
            }
            
            process.WaitForExit();
        }

        private void FetchTypes()
        {
            _types.AddRange(typeof(Component).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Component)) && type.IsAbstract == false));
        }

        public void Unload()
        {
            // _context.Unload();
        }

        public List<Type> GetTypes()
        {
            return _types;
        }

        // private Type GetNewType(Type oldType)
        // {
        //     return _types.FirstOrDefault(type => type.ToString() == oldType.ToString());
        // }
    }
}
