using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Unify2D.Core;

namespace UnifyCore
{
    public class ResourcesManager
    {
        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        /// <summary>
        /// TODO remove startasset
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startAsset"></param>
        /// <returns></returns>
        public Texture2D GetTexture(string path, bool startAsset = true)
        {
            if (_textures.TryGetValue(path, out Texture2D texture))
                return texture;

            if (startAsset)
                texture = GameCore.Current.Game.Content.Load<Texture2D>($"./Assets{path}");
            else
                texture = GameCore.Current.Game.Content.Load<Texture2D>($"{path}");

            _textures.Add(path, texture);

            return texture;
        }
    }
}
