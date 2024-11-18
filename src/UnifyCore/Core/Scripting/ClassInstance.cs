using System;
using System.Runtime.InteropServices;
using Unify2D.Core;
using Unify2D.Core.Graphics;

namespace UnifyCore.Scripting;

public class ClassInstance
{
    private readonly ScriptingBridge _bridge = Scripting.Instance.Bridge;
    
    private IntPtr _nativeClassPointer;
    
    public Component CSharpObject { get; private set; }
    public bool IsNative { get; private set; }
    public string ClassName { get; private set; }
    
    public ClassInstance(ClassType classType)
    {
        IsNative = classType.IsNativeClass;
        ClassName = classType.ClassName;

        if (IsNative)
        {
            _nativeClassPointer = _bridge.InstantiateClass(classType.ClassName, classType.Namespace);
        }
        else
        {
            CSharpObject = (Component) Activator.CreateInstance(classType.Type);
        }
    }

    public void Update()
    {
        if (IsNative)
        {
            _bridge.CallMethod(_nativeClassPointer, "Update");
        }
        else
        {
            CSharpObject.Update(GameCore.Current);
        }
    }

    public bool IsRenderer()
    {
        if (IsNative)
        {
            return false;
        }
        else
        {
            return CSharpObject is Renderer;
        }
    }

    public void Initialize(GameObject gameObject)
    {
        if (IsNative)
        {
            _bridge.CallMethod(_nativeClassPointer, "Initialize", gameObject);
        }
        else
        {
            CSharpObject.Initialize(gameObject);
        }
    }
}