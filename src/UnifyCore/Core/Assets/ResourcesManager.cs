using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Core;
using FontStashSharp;
using SpriteFontPlus;

namespace UnifyCore
{
    public class ResourcesManager : IDisposable
    {
        private Texture2D _whiteTexture;

        Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        private SpriteFont _defaultFont;
        private string _arialDefault = @"C:\Windows\Fonts\arial.ttf"; // _Common Font test


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

            _defaultFont = LoadFontFromFile(_arialDefault);
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
            //font = _content.Load<SpriteFont>($"./Assets/{path}");

            string fullPath = Path.Combine(_content.RootDirectory, "Assets", path.TrimStart('\\', '/'));            

            font = LoadFontFromFile(fullPath);

            _fonts.Add(path, font);

            return font;
        }

        private SpriteFont LoadFontFromFile(string fullPath) 
        {
            var baked = TtfFontBaker.Bake(File.ReadAllBytes(fullPath), 50, 1024, 1024, new[]
            {
                CharacterRange.BasicLatin,
                CharacterRange.Latin1Supplement,
                CharacterRange.LatinExtendedA,
                CharacterRange.Cyrillic
            });

            return baked.CreateSpriteFont(GameCore.Current.GraphicsDevice);
        }

        public void SetRootDirectory(string path)
        {
            _content.Unload();
            _textures.Clear();
            _fonts.Clear();
            _content.RootDirectory = path;
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
