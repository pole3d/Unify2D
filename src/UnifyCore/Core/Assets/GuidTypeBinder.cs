using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unify2D;
using Unify2D.Core.Graphics;

public class GuidTypeBinder : ISerializationBinder
{
    public static GuidTypeBinder Instance = new GuidTypeBinder();

    // TODO : move this from here
    public GuidTypeBinder()
    {
        Register(typeof(SpriteRenderer), "12345678-1234-1234-1234-123456789abc");
    }

    private readonly Dictionary<string, Type> _guidToType = new();
    private readonly Dictionary<Type, string> _typeToGuid = new();
    private readonly Dictionary<(string Assembly, string Name), Type> _resolvedTypes = new();
    private readonly Dictionary<Assembly, Dictionary<string, Type>> _typeNames = new();
    private Assembly _scriptAssembly;
    private bool _triedScriptAssembly;

    public void SetScriptAssembly(Assembly assembly)
    {
        // Drop references to the previous collectible script assembly.
        _resolvedTypes.Clear();
        _typeNames.Clear();
        _scriptAssembly = assembly;
        _triedScriptAssembly = assembly != null;
    }

    private Type FindType(Assembly assembly, string name)
    {
        // Scene files normally contain full names: avoid enumerating every type.
        var type = assembly.GetType(name, false);
        if (type != null) return type;
        if (!_typeNames.TryGetValue(assembly, out var names))
        {
            names = new Dictionary<string, Type>(StringComparer.Ordinal);
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException error) { types = error.Types; }
            foreach (var candidate in types)
            {
                if (candidate == null) continue;
                if (candidate.FullName != null) names.TryAdd(candidate.FullName, candidate);
                names.TryAdd(candidate.Name, candidate);
            }
            _typeNames[assembly] = names;
        }
        return names.TryGetValue(name, out type) ? type : null;
    }

    public void Register(Type type, string guid)
    {
        _guidToType[guid] = type;
        _typeToGuid[type] = guid;
    }

    public Type BindToType(string assemblyName, string guid)
    {
        if (_guidToType.TryGetValue(guid, out var type))
        {
            return type;
        }

        var key = (assemblyName ?? string.Empty, guid);
        if (_resolvedTypes.TryGetValue(key, out type)) return type;
        string simpleAssemblyName = string.IsNullOrEmpty(assemblyName)
            ? null : new AssemblyName(assemblyName).Name;

        // The editor supplies its current in-memory assembly. Standalone games
        // can still resolve the exported GameAssembly.dll.
        if (!_triedScriptAssembly)
        {
            _triedScriptAssembly = true;
            string path = Path.Combine(AppContext.BaseDirectory, "GameAssembly.dll");
            try
            {
                if (File.Exists(path)) _scriptAssembly = Assembly.LoadFrom(path);
            }
            catch (IOException) { }
            catch (BadImageFormatException) { }
        }
        if (_scriptAssembly != null &&
            (simpleAssemblyName == null || simpleAssemblyName == _scriptAssembly.GetName().Name))
        {
            type = FindType(_scriptAssembly, guid);
            if (type != null) return _resolvedTypes[key] = type;
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        // Respect the assembly named in the scene before legacy name-only lookup.
        foreach (var asm in assemblies)
        {
            if (asm.IsCollectible || asm.GetName().Name != simpleAssemblyName ||
                (_scriptAssembly != null && asm.GetName().Name == _scriptAssembly.GetName().Name)) continue;
            type = FindType(asm, guid);
            if (type != null) return _resolvedTypes[key] = type;
        }
        foreach (var asm in assemblies)
        {
            // Old editor/Play assemblies may remain alive until the next GC.
            if (asm.IsCollectible ||
                (_scriptAssembly != null && asm.GetName().Name == _scriptAssembly.GetName().Name)) continue;
            type = FindType(asm, guid);
            if (type != null) return _resolvedTypes[key] = type;
        }

        Debug.LogError($"Unrecognized type GUID: {guid}");
        return null;
    }

    public void BindToName(Type serializedType, out string assemblyName, out string guid)
    {
        assemblyName = null;
        if (_typeToGuid.TryGetValue(serializedType, out guid))
        {

        }
        else
        {
            throw new JsonSerializationException($"Type not registered: {serializedType.FullName}");
        }
    }
}
