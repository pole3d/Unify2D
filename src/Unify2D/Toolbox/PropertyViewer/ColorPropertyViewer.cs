using ImGuiNET;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace Unify2D.Toolbox
{
    internal class ColorPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property , object instance)
        {
            Color color = (Color)property.GetValue(instance);
            System.Numerics.Vector4 vector = new System.Numerics.Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

            if (ImGui.ColorEdit4(property.Name, ref vector))
            {
                color = new Color(vector.X, vector.Y, vector.Z, vector.W);
                property.SetValue(instance, color);
            }
        }
    }
}
