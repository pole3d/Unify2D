using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
