﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Unify2D.Core.Tools
{
    /// <summary>
    /// The <see cref="Gizmo"/> class is a static class that facilitates
    /// in-editor visual debugging by providing methods to draw graphical
    /// elements in the Unify editor scene view. It offers a set of static
    /// methods for drawing gizmos, labels, and other visual cues to aid
    /// developers in understanding and debugging their scenes during development.
    /// </summary>
    public static class Gizmo
    {
        private static Texture2D _texture = new Texture2D(GameCore.Current.Game.GraphicsDevice, 1, 1);
        public static void SetColor(Color color)
        {
            _texture.SetData(new Color[] { color });
        }

        public static void DrawSquare(Vector2 origin, Vector2 size, float rotation)
        {
            Rectangle rectangle = new Rectangle((int)origin.X, (int)origin.Y, (int)size.X, (int)size.Y);

            GameCore.Current.SpriteBatch.Draw(_texture, rectangle, rectangle, Color.LightGreen, rotation, size / 2f, SpriteEffects.None, 1);
        }

        #region DrawSquare Overrides
        public static void DrawSquare(Vector2 topLeft, Vector2 bottomRight)
        {
            Rectangle rectangle = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));
            GameCore.Current.SpriteBatch.Draw(_texture, rectangle, Color.White);
        }
        public static void DrawSquare(Vector2 origin, Vector2 topLeft, Vector2 bottomRight, float rotation)
        {
            Rectangle rectangle = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));

            GameCore.Current.SpriteBatch.Draw(_texture, rectangle, rectangle, Color.White, rotation, origin, SpriteEffects.None, 1);
        }
        #endregion

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

        public static void DrawWireSquare(Vector2 position, Vector2 size, int thickness, float rotation, Color color)
        {
            Vector2 reverseSize = new Vector2(size.Y, size.X); //Rotating the draw also rotates THE FUCKING AXIS

            Vector2 topLeft = new Vector2(position.X - size.X / 2, position.Y - size.Y / 2);
            Vector2 bottomRight = new Vector2(position.X + size.X / 2, position.Y + size.Y / 2);

            Rectangle top = new Rectangle((int)position.X, (int)position.Y, (int)(bottomRight.X - topLeft.X), thickness);
            Rectangle right = new Rectangle((int)position.X, (int)position.Y, (int)(bottomRight.Y - topLeft.Y), thickness);
            Rectangle bottom = new Rectangle((int)position.X, (int)position.Y, (int)(bottomRight.X - topLeft.X), thickness);
            Rectangle left = new Rectangle((int)position.X, (int)position.Y, (int)(bottomRight.Y - topLeft.Y), thickness);

            

            GameCore.Current.SpriteBatch.Draw(_texture, top, top, color, 0 + rotation, size / 2f, SpriteEffects.None, 1);
            GameCore.Current.SpriteBatch.Draw(_texture, right, right, color, MathF.PI/2 + rotation, reverseSize / 2f, SpriteEffects.None, 1);
            GameCore.Current.SpriteBatch.Draw(_texture, bottom, bottom, color, MathF.PI + rotation, size / 2f, SpriteEffects.None, 1);
            GameCore.Current.SpriteBatch.Draw(_texture, left, left, color, 3 * MathF.PI/2 + rotation, reverseSize / 2f, SpriteEffects.None, 1);
        }
    }
}
