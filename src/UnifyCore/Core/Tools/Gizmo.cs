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
            GameCore.Current.SpriteBatch.Draw(_texture, rectangle, rectangle, Color.White, rotation, origin, SpriteEffects.None, 1);
        }

        public static void DrawWireSquare(Vector2 topLeft, Vector2 bottomRight, int width, Color color)
        {
            Rectangle left = new Rectangle((int)topLeft.X, (int)topLeft.Y, width, (int)(bottomRight.Y - topLeft.Y));
            Rectangle right = new Rectangle((int)bottomRight.X, (int)topLeft.Y, width, (int)(bottomRight.Y - topLeft.Y));

            Rectangle top = new Rectangle((int)topLeft.X, (int)bottomRight.Y, (int)(bottomRight.X - topLeft.X), width);
            Rectangle bottom = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), width);


            GameCore.Current.SpriteBatch.Draw(_texture, left, color);
            GameCore.Current.SpriteBatch.Draw(_texture, right, color);
            GameCore.Current.SpriteBatch.Draw(_texture, top, color);
            GameCore.Current.SpriteBatch.Draw(_texture, bottom, color);
        }
    }
}
