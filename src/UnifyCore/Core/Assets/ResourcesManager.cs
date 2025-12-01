using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Unify2D.Core;

namespace UnifyCore
{
    public class ResourcesManager
    {
        private static Texture2D _whiteTexture;

        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();


        public ResourcesManager()
        {
            _whiteTexture = new Texture2D(GameCore.Current.Game.GraphicsDevice, 128, 128);
            Color[] colors = new Color[128 * 128];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.White;
            }
            _whiteTexture.SetData(colors);
        }


        public Texture2D GetTexture(string path)
        {
            if ( string.IsNullOrEmpty( path ) )
                return _whiteTexture;

            if (_textures.TryGetValue(path, out Texture2D texture))
                return texture;

            texture = GameCore.Current.Game.Content.Load<Texture2D>($"./Assets/{path}");
            _textures.Add(path, texture);

            return texture;
        }




    }
}
