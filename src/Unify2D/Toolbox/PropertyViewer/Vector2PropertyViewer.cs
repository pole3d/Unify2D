using ImGuiNET;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class Vector2PropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance)
        {
            var XnaVector = (Microsoft.Xna.Framework.Vector2)property.GetValue(instance);  //Conversion between the two Vector2 types

            Vector2 value = new Vector2(XnaVector.X, XnaVector.Y);

            if (ImGui.InputFloat2(property.Name, ref value))
            {
                property.SetValue(instance, new Microsoft.Xna.Framework.Vector2(value.X, value.Y));   //Converting back
            }
        }
    }
}
