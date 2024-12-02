using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpriteFontPlus;

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
        public Color VertexColor { get; set; } = Color.White;
        
        [JsonIgnore] public SpriteFont Font { get; private set; }
        public string FontPath { get; set; }
        
        public float FontSize { get; set; } = 12;

        [JsonProperty]
        private GameAsset _asset;

        public void SetFont(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            
            FontPath = path;
            TtfFontBakerResult fontBakeResult = TtfFontBaker.Bake(File.ReadAllBytes(path),
                25, 1024, 1024,
                new[] { CharacterRange.BasicLatin, CharacterRange.Latin1Supplement, CharacterRange.LatinExtendedA, CharacterRange.Cyrillic }
            );
            Font = fontBakeResult.CreateSpriteFont(GameCore.Current.GraphicsDevice);
        }
        
        public override void Load(Game game, GameObject go)
        {
            base.Load(game, go);
            
            try
            {
                _asset = new GameAsset(Font, _asset.Name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            SetFont(FontPath);
        }
        
        public override void Draw()
        {
            if (_gameObject == null) return;
            
            if (string.IsNullOrEmpty(Text) || Font == null)
            {
                SetFont(FontPath);
                return;
            }

            Vector2 stringSize = Font.MeasureString(Text);
            Vector2 origin = Origin + stringSize * GetAnchorVector(Anchor);
            
            GameCore.Current.SpriteBatch.DrawString(
                Font, 
                Text,  
                GameObject.Position, 
                VertexColor, 
                GameObject.Rotation, 
                origin, 
                Vector2.One * FontSize * GameObject.Scale, 
                SpriteEffects.None, 
                0);
        }
        
        internal override void Destroy()
        {
            _asset?.Release();
        }
    }
}