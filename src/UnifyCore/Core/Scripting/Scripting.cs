using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unify2D.Core;

namespace UnifyCore.Scripting;

public class Scripting
{
    public static Scripting Instance { get; } = new();

    List<ClassType> _types = new();
    
    private readonly string _programFilesPath = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
    private readonly string _cmakePath;
    
    private bool _isAssemblyLoaded = false;

    public ScriptingBridge Bridge { get; private set; }

    private Scripting()
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

        Bridge = new ScriptingBridge();
        Bridge.Open(Path.Combine(Directory.GetCurrentDirectory(), "UnifyCore.dll"));
    }

    public void Reload()
    {
        Bridge.Reload();
        
        FetchTypes();
    }

    public void LoadDll(string assemblyPath)
    {
        CreateBuildFiles(assemblyPath);

        if (!File.Exists(assemblyPath))
        {
            return;
        }
        
        Bridge.Open(assemblyPath);
        
        FetchTypes();
        
        
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

    private void CreateBuildFiles(string buildDirectory)
    {
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

        Build(buildDirectory);
    }
    
    public void Build(string buildDirectory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo(
            _cmakePath,
            "--build ."
        );

        startInfo.WorkingDirectory = buildDirectory;
        
        Process process = Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception("Could not start CMake");
        }
        
        process.WaitForExit();
    }

    private void FetchTypes()
    {
        _types.Clear();
        
        foreach (Type type in typeof(Component).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Component)) && type.IsAbstract == false))
        {
            _types.Add(new ClassType(type.Name, type.Namespace ?? "", type));
        }
        
        _types.AddRange(Bridge.GetComponents());
        
        // _types.AddRange(typeof(Component).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Component)) && type.IsAbstract == false));
    }

    public void Unload()
    {
        // _context.Unload();
    }

    public List<ClassType> GetTypes()
    {
        return _types;
    }

    // private Type GetNewType(Type oldType)
    // {
    //     return _types.FirstOrDefault(type => type.ToString() == oldType.ToString());
    // }
}
