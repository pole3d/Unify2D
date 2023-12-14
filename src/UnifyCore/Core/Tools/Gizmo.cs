using ImGuiNET;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D;

namespace Unify2D.Core.Tools
{
    public static class Gizmo
    {
        private static Texture2D _texture = new Texture2D(GameCore.Current.Game.GraphicsDevice, 1, 1);
        public static void SetColor(Color color)
        {
            _texture.SetData(new Color[] { color });
        }

        public static void DrawSquare(Vector2 topLeft, Vector2 bottomRight)
        {
            Rectangle rectangle = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));
            GameCore.Current.SpriteBatch.Draw(_texture, rectangle, Color.White);
        }

        public static void DrawWireSquare(Vector2 topLeft, Vector2 bottomRight, int width)
        {
            Rectangle left   = new Rectangle((int)topLeft.X, (int)topLeft.Y, width, (int)(bottomRight.Y - topLeft.Y));
            Rectangle right  = new Rectangle((int)bottomRight.X, (int)topLeft.Y, width, (int)(bottomRight.Y - topLeft.Y));

            

            Rectangle top    = new Rectangle((int)topLeft.X, (int)bottomRight.Y, (int)(bottomRight.X - topLeft.X), width);
            Rectangle bottom = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), width);

            GameCore.Current.SpriteBatch.Draw(_texture, left, Color.White);
            GameCore.Current.SpriteBatch.Draw(_texture, right, Color.White);
            GameCore.Current.SpriteBatch.Draw(_texture, top, Color.White);
            GameCore.Current.SpriteBatch.Draw(_texture, bottom, Color.White);
        }


        //Added overrides   -Thomas
        public static void DrawSquare(Vector2 origin, Vector2 topLeft, Vector2 bottomRight, float rotation)
        {
            Rectangle rectangle = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));

            /*
            float sin = MathF.Sin(rotation * Conversions.Deg2Rad);
            float cos = MathF.Cos(rotation * Conversions.Deg2Rad);

            Vector2 position = new Vector2((origin.X * cos) + (origin.Y * sin), -((origin.X * sin) - (origin.Y * cos)));
            */

            GameCore.Current.SpriteBatch.Draw(_texture, rectangle, rectangle, Color.White, rotation, origin, SpriteEffects.None, 1);
        }
        public static void DrawSquare(Vector2 origin, Vector2 size, float rotation)
        {
            Rectangle rectangle = new Rectangle((int)origin.X, (int)origin.Y, (int)size.X, (int)size.Y);

            /*
            float sin = MathF.Sin(rotation * Conversions.Deg2Rad);
            float cos = MathF.Cos(rotation * Conversions.Deg2Rad);

            Vector2 position = new Vector2((origin.X * cos) + (origin.Y * sin), -((origin.X * sin) - (origin.Y * cos)));
            */

            GameCore.Current.SpriteBatch.Draw(_texture, rectangle, rectangle, Color.White, rotation, size / 2f, SpriteEffects.None, 1);
        }

        public static void DrawWireBox(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, int thickness, Color color)
        {
            var numericsTopLeft = new System.Numerics.Vector2((int)topLeft.X, (int)topLeft.Y);
            var numericsTopRight = new System.Numerics.Vector2((int)topRight.X, (int)topRight.Y);
            var numericsBottomLeft = new System.Numerics.Vector2((int)bottomLeft.X, (int)bottomLeft.Y);
            var numericsBottomRight = new System.Numerics.Vector2((int)bottomRight.X, (int)bottomRight.Y);

            //DrawUp
            ImGui.GetWindowDrawList().AddLine(numericsTopLeft, numericsTopRight, Conversions.ColorToUInt(color), thickness);
            //DrawDown
            ImGui.GetWindowDrawList().AddLine(numericsBottomLeft, numericsBottomRight, Conversions.ColorToUInt(color), thickness);
            //DrawLeft
            ImGui.GetWindowDrawList().AddLine(numericsTopLeft, numericsBottomLeft, Conversions.ColorToUInt(color), thickness);
            //DrawRight
            ImGui.GetWindowDrawList().AddLine(numericsTopRight, numericsBottomRight, Conversions.ColorToUInt(color), thickness);
        }
    }
}
