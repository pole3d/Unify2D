using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Unify2D.Core;

namespace UnifyCore
{
    public class ResourcesManager
    {
        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        public Texture2D GetTexture(string path)
        {
            if (_textures.TryGetValue(path, out Texture2D texture))
                return texture;

            texture = GameCore.Current.Game.Content.Load<Texture2D>($"./Assets/{path}");
            _textures.Add(path, texture);

            return texture;
        }




    }
}
