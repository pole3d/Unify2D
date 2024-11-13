using Microsoft.Xna.Framework;
using System;

namespace Unify2D.Core.Tools
{
    internal class Conversions
    {
        public static uint ColorToUInt(Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                          (color.G << 8) | (color.B << 0));
        }

        public static Color UIntToColor(uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return new Color(a, r, g, b);
        }

        public static float Deg2Rad = (float) Math.PI / 180f;


        //Remove it from here later
        public static Vector2 RotationOffset(Vector2 vector, float radians)
        {
            float sin = (float) Math.Sin(radians);
            float cos = (float) Math.Cos(radians);

            Vector2 rotated = new Vector2((vector.X * cos) + (vector.Y * sin), ((vector.X * sin) - (vector.Y * cos)));

            return rotated;
        }
    }
}
