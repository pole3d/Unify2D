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
        Register(typeof(SpriteRenderer), SpriteRendererGuid);
    }

    private readonly Dictionary<string, Type> _guidToType = new();
    private readonly Dictionary<Type, string> _typeToGuid = new();
    private readonly Dictionary<(string Assembly, string Name), Type> _resolvedTypes = new();
    private readonly Dictionary<Assembly, Dictionary<string, Type>> _typeNames = new();
    private Assembly _scriptAssembly;
    private bool _triedScriptAssembly;
    public const string ScriptGuidPrefix = "Unify2D.ScriptGuid.";
    public const string SpriteRendererGuid = "12345678-1234-1234-1234-123456789abc";
    private readonly Dictionary<string, Type> _scriptGuids = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _scriptNames = new(StringComparer.Ordinal);
    public bool TryGetScriptGuid(string fullName, out string guid) => _scriptNames.TryGetValue(fullName, out guid);
    public bool TryGetScriptType(string guid, out Type type) => _scriptGuids.TryGetValue(guid, out type);

    public void SetScriptAssembly(Assembly assembly)
    {
        // Drop references to the previous collectible script assembly.
        _resolvedTypes.Clear();
        _typeNames.Clear();
        _scriptAssembly = assembly;
        _triedScriptAssembly = assembly != null;
        _scriptGuids.Clear();
        _scriptNames.Clear();
        if (assembly == null) return;
        foreach (var metadata in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (!metadata.Key.StartsWith(ScriptGuidPrefix, StringComparison.Ordinal)) continue;
            string guid = metadata.Key.Substring(ScriptGuidPrefix.Length);
            var type = assembly.GetType(metadata.Value, throwOnError: true);
            if (!Guid.TryParse(guid, out var parsed) || parsed == Guid.Empty ||
                !type.IsSubclassOf(typeof(Unify2D.Core.Component)))
                throw new JsonSerializationException($"Invalid script identity: {metadata.Key}");
            _scriptGuids.Add(parsed.ToString("D"), type);
            _scriptNames.Add(type.FullName, parsed.ToString("D"));
        }
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

        if (_scriptGuids.TryGetValue(guid, out type)) return type;

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
                if (File.Exists(path)) SetScriptAssembly(Assembly.LoadFrom(path));
            }
            catch (IOException) { }
            catch (BadImageFormatException) { }
        }
        if (_scriptGuids.TryGetValue(guid, out type)) return type;
        if (Guid.TryParse(guid, out _))
            throw new JsonSerializationException($"Unknown component GUID: {guid}. Restore its script and .meta.");
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
        if (serializedType.Assembly == _scriptAssembly &&
            _scriptNames.TryGetValue(serializedType.FullName, out guid)) return;
        if (_typeToGuid.TryGetValue(serializedType, out guid)) return;
        if (serializedType.Assembly.GetName().Name == "GameAssembly" &&
            serializedType.IsSubclassOf(typeof(Unify2D.Core.Component)))
            throw new JsonSerializationException($"Script has no .meta identity: {serializedType.FullName}");
        assemblyName = serializedType.Assembly.GetName().Name;
        guid = serializedType.FullName;
    }
}
