using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Unify2D.Core;

namespace Unify2D.Scripting
{
    public class Scripting
    {
        List<Type> _types = new();
        GameEditor _editor;
        
        private readonly string _programFilesPath = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
        private readonly string _msBuildPath;
        
        private AssemblyLoadContext _context;
        private bool _isAssemblyLoaded = false;

        private string _currentScenePath;
        
        public Scripting()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(
                $@"{_programFilesPath}\Microsoft Visual Studio\Installer\vswhere.exe",
                @"-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe"
            );

            startInfo.RedirectStandardOutput = true;

            Process process = Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Could not start VSWhere");
            }
            
            process.WaitForExit();

            using StreamReader reader = process.StandardOutput;
            _msBuildPath = reader.ReadLine();
        }

        public void Reload()
        {
            if (!_isAssemblyLoaded)
            {
                OnUnload(null);
                return;
            }
            
            _context.Unloading += OnUnload;
            _context.Unload();
        }

        private void OnUnload(AssemblyLoadContext assemblyLoadContext)
        {
            _types.Clear();
            _context = new AssemblyLoadContext(null, true);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            ProcessStartInfo startInfo = new ProcessStartInfo(
                _msBuildPath,
                $@"{GameEditor.Instance.AssetsPath}\GameAssembly.sln"
            );

            Process process = Process.Start(startInfo);
            if (process == null)
            {
                throw new Exception("Could not start MSBuild");
            }
            
            process.WaitForExit();

            byte[] data = File.ReadAllBytes($@"{GameEditor.Instance.AssetsPath}\bin\Debug\net6.0\GameAssembly.dll");
            
            using MemoryStream stream = new MemoryStream(data);
            Assembly assembly = _context.LoadFromStream(stream);
            _isAssemblyLoaded = true;

            _types.AddRange(typeof(Component).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Component)) && type.IsAbstract == false));
            _types.AddRange(assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Component)) && type.IsAbstract == false));
            
            ReplaceComponents();
        }

        private void ReplaceComponents()
        {
            foreach (GameObject go in SceneManager.Instance.CurrentScene.GameObjects)
            {
                List<Component> newComponents = new List<Component>();
                foreach (Component oldComponent in go.Components)
                {
                    Component newComp = Activator.CreateInstance(GetNewType(oldComponent.GetType())) as Component;
                    newComponents.Add(newComp);

                    FieldInfo[] oldFields = oldComponent.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
                    foreach (FieldInfo oldField in oldFields)
                    {
                        FieldInfo[] newFields = newComp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
                        foreach (FieldInfo newField in newFields)
                        {
                            if (newField.Name == oldField.Name)
                            {
                                newField.SetValue(newComp, oldField.GetValue(oldComponent));
                            }
                        }
                    }
                }
            
                go.ClearComponents();
            
                foreach (Component item in newComponents)
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

        private Type GetNewType(Type oldType)
        {
            return _types.FirstOrDefault(type => type.ToString() == oldType.ToString());
        }
    }
}
