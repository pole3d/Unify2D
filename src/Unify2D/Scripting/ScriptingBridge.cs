using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Unify2D.Scripting;

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
    private static extern void InstantiateClass(IntPtr EnginePointer, StringBuilder ClassName, int Length);

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
        Open(_pointer, builder, builder.Capacity);
    }

    public void InstantiateClass(string className)
    {
        StringBuilder builder = new StringBuilder(className);
        InstantiateClass(_pointer, builder, builder.Capacity);
    }
}