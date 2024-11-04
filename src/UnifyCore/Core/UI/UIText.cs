using System;
using System.IO;
using FNT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="UIText"/> class extends the functionality of
    /// the base <see cref="UIComponent"/> class and introduces specific properties to display text 
    /// </summary>
    public class UIText : UIComponent
    {
        /// Properties
        public string Text { get; set; } = "Lorem Ipsum";
        public int Size { get; set; } = 1;
        public Color VertexColor { get; set; } = Color.White;
        public FontLibrary.IFont Font { get; set; }

        [JsonProperty]
        private GameAsset _asset;

        public override void Draw()
        {
            if (string.IsNullOrEmpty(Text) || Font == null) return;

            Debug.Log("draw");
            
            FontLibrary.IText text = Font.MakeText("Hello, world!");
            GameCore.Current.SpriteBatch.DrawString(text, GameObject.Position, VertexColor);
        }
        
        public void Initialize(Game game, GameObject go, string path)
        {
            _gameObject = go;
            try
            {
                path = path.Remove(0,1);
                
                // FontLibrary font = game.Content.Load<FontLibrary>($"./Assets/{path}");
                // path = @"C:\Users\matteo.benaissa\Unify2D\bin\Debug\Unify2D\NEW_PROJECT\Assets\font.ttf";
                
                path = $"{game.Content.RootDirectory}/Assets/{path}";
                FontLibrary font = new FontLibrary(File.OpenRead(path ), GameCore.Current.GraphicsDevice);
                
                FontLibrary.IFont fontFace = font.CreateFont(64); //--- CODE STOP HERE ---
                
                Font = fontFace;
                Debug.Log(Font != null);
                
                _asset = new GameAsset(Font, path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        
        internal override void Destroy()
        {
            _asset.Release();
        }

        public override void Load(Game game, GameObject go)
        {
            Initialize(game, go, _asset.Name);
        }
    }
}