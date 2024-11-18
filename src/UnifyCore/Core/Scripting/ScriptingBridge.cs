using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace UnifyCore.Scripting;

public class ScriptingBridge
{
    [DllImport("ScriptingBridge.dll", CallingConvention=CallingConvention.Cdecl)]
    private static extern IntPtr CreateEngine();

    [DllImport("ScriptingBridge.dll", CallingConvention=CallingConvention.Cdecl)]
    private static extern void DeleteEngine(IntPtr enginePointer);

    [DllImport("ScriptingBridge.dll", CallingConvention=CallingConvention.Cdecl)]
    private static extern void Reload(IntPtr enginePointer);

    [DllImport("ScriptingBridge.dll", CallingConvention=CallingConvention.Cdecl)]
    private static extern void Open(IntPtr enginePointer, StringBuilder assemblyPath, int length);

    [DllImport("ScriptingBridge.dll", CallingConvention=CallingConvention.Cdecl)]
    private static extern IntPtr InstantiateClass(IntPtr enginePointer, StringBuilder className, int classNameLength, StringBuilder nameSpace, int nameSpaceLength);

    [DllImport("ScriptingBridge.dll", CallingConvention=CallingConvention.Cdecl)]
    private static extern void CallMethod(IntPtr enginePointer, IntPtr objectPointer, StringBuilder methodName, int methodNameLength, IntPtr parameter = default);

    [DllImport("ScriptingBridge.dll", CallingConvention=CallingConvention.Cdecl)]
    private static extern IntPtr GetComponents(IntPtr enginePointer, out int count);

    private readonly IntPtr _pointer;

    public ScriptingBridge()
    {
        _pointer = CreateEngine();
    }

    ~ScriptingBridge()
    {
        DeleteEngine(_pointer);
    }

    public void Reload()
    {
        Reload(_pointer);
    }

    public void Open(string name)
    {
        StringBuilder builder = new StringBuilder(name);
        Open(_pointer, builder, builder.Length);
    }

    public void CallMethod(IntPtr objectPointer, string methodName)
    {
        StringBuilder methodNameBuilder = new StringBuilder(methodName);
        CallMethod(_pointer, objectPointer, methodNameBuilder, methodNameBuilder.Length);
    }

    public void CallMethod(IntPtr objectPointer, string methodName, object parameter)
    {
        StringBuilder methodNameBuilder = new StringBuilder(methodName);
        CallMethod(_pointer, objectPointer, methodNameBuilder, methodNameBuilder.Length, GetPointer(parameter));
    }

    public IntPtr InstantiateClass(string className, string nameSpace = "")
    {
        StringBuilder classNameBuilder = new StringBuilder(className);
        StringBuilder nameSpaceBuilder = new StringBuilder(nameSpace);

        return InstantiateClass(
            _pointer,
            classNameBuilder, classNameBuilder.Length,
            nameSpaceBuilder, nameSpaceBuilder.Length
        );
    }

    public List<ClassType> GetComponents()
    {
        List<ClassType> types = new();

        int typeCount;
        IntPtr typesPointer = GetComponents(_pointer, out typeCount);

        for (int i = 0; i < typeCount; i++)
        {
            IntPtr classTypePointer = Marshal.ReadIntPtr(typesPointer, IntPtr.Size * i);
            types.Add(ReadClassType(classTypePointer));
        }
        
        return types;
    }

    private ClassType ReadClassType(IntPtr classTypePointer)
    {
        return new ClassType(
            Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(classTypePointer)),
            Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(classTypePointer, IntPtr.Size))
        );
    }
    
    private static IntPtr GetPointer(object managedObject)
    {
        GCHandle handle = GCHandle.Alloc(managedObject, GCHandleType.Normal);
        return GCHandle.ToIntPtr(handle);
    }
}