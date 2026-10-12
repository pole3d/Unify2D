using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Core;

namespace UnifyCore
{
    public class ResourcesManager : IDisposable
    {
        private Texture2D _whiteTexture;

        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        private SpriteFont _defaultFont;

        private Microsoft.Xna.Framework.Content.ContentManager _content;

        Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();

        public ResourcesManager()
        {
            _content = new Microsoft.Xna.Framework.Content.ContentManager(
                GameCore.Current.Game.Services, GameCore.Current.Game.Content.RootDirectory);
            _whiteTexture = new Texture2D(GameCore.Current.Game.GraphicsDevice, 128, 128);
            Color[] colors = new Color[128 * 128];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.White;
            }
            _whiteTexture.SetData(colors);

            _defaultFont = null;
        }


        public Texture2D GetTexture(string path)
        {
            if (string.IsNullOrEmpty(path))
                return _whiteTexture;

            if (_textures.TryGetValue(path, out Texture2D texture))
                return texture;

            texture = _content.Load<Texture2D>($"./Assets/{path}");

            _textures.Add(path, texture);

            return texture;
        }

        public SpriteFont GetFont(string path)
        {
            if (string.IsNullOrEmpty(path))
                return _defaultFont;

            if (_fonts.TryGetValue(path, out SpriteFont font))
                return font;

            font = _content.Load<SpriteFont>($"./Assets/{path}");

            _fonts.Add(path, font);

            return font;
        }

        public void Dispose()
        {
            _content.Dispose();
            _whiteTexture.Dispose();
            _textures.Clear();
            _fonts.Clear();
        }
    }
}
