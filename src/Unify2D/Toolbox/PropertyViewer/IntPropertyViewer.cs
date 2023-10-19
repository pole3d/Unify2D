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
