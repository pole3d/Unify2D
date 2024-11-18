using ImGuiNET;
using System;
using System.Reflection;

namespace Unify2D.Toolbox
{
    internal class StringPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance)
        {
            string value = (string)property.GetValue(instance);
            if (value == null)
            {
                value = String.Empty;
            }

            if (ImGui.InputText(property.Name, ref value, 40))
            {
                property.SetValue(instance, value);
            }
        }
    }
}
