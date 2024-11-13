using ImGuiNET;
using System.Reflection;

namespace Unify2D.Toolbox
{
    internal class FloatPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance)
        {
            float value = (float)property.GetValue(instance);
            if (ImGui.InputFloat(property.Name, ref value))
            {
                property.SetValue(instance, value);
            }
        }
    }
}
