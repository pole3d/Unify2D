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
    internal class BoolPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance)
        {
            bool value = (bool)property.GetValue(instance);

            if (ImGui.Checkbox(property.Name, ref value))
            {
                property.SetValue(instance, value);
            }
        }
    }
}
