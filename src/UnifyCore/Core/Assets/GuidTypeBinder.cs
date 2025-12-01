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

        var assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
        try
        {
            var assembly = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "GameAssembly.dll"));
            assemblies.Add(assembly);

        }
        catch (Exception)
        {
        }


        foreach (var asm in assemblies)
        {
            foreach (var t in asm.GetTypes())
            {
                if (t.FullName == guid || t.Name == guid)
                {
                    return t;
                }
            }
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
