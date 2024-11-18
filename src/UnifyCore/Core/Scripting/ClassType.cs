using System;

namespace UnifyCore.Scripting;

public class ClassType
{
    public string ClassName { get; private set; }
    public string Namespace { get; private set; }
    public Type Type { get; private set; }
    
    public bool IsNativeClass => Type == null;
    
    public ClassType(string className, string nameSpace = "", Type type = null)
    {
        ClassName = className;
        Namespace = nameSpace;
        Type = type;
    }

    public ClassInstance Instantiate()
    {
        return new ClassInstance(this);
    }
}