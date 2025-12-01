using System;
using System.Reflection;
using ImGuiNET;

namespace Unify2D.Toolbox;

public class EnumPropertyViewer : PropertyViewer
{
    public override void Draw(PropertyInfo property, object instance)
    {
        if (property.PropertyType.IsEnum == false)
        {
            return;
        }

        DrawEnumFoldout(property.PropertyType, property, instance);
    }

    private void DrawEnumFoldout(Type type, PropertyInfo property, object instance)
    {
        string[] enumNames = Enum.GetNames(type);
        Array enumValues = Enum.GetValues(type);

        string typeName = type.ToString();
        int index = -1;
        for (int i = typeName.Length - 1; i >= 0; i--)
        {
            if (char.IsLetter(typeName[i]) == false)
            {
                index = i;
                break;
            }
        }
        string label = typeName.Substring(index + 1);

        int currentFoldoutIndex = (int)Convert.ChangeType(property.GetValue(instance), type);

        ImGui.Combo(label, ref currentFoldoutIndex, enumNames, enumNames.Length);

        object value = enumValues.GetValue(currentFoldoutIndex);
        property.SetValue(instance, value);
    }
}