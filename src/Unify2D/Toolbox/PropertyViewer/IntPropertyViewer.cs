using ImGuiNET;
using System.Reflection;

namespace Unify2D.Toolbox
{
    internal class IntPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance)
        {
            int value = (int)property.GetValue(instance);
            if (ImGui.InputInt(property.Name, ref value))
            {
                property.SetValue(instance, value);
            }
        }
    }
}
