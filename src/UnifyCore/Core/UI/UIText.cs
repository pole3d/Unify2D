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
        public SpriteFont Font { get; private set; }
        public float FontSize { get; set; } = 12;

        [JsonProperty]
        private GameAsset _asset;

        public override void Draw()
        {
            if (string.IsNullOrEmpty(Text) || Font == null) return;

            Vector2 stringSize = Font.MeasureString(Text);
            Vector2 origin = Origin + stringSize * GetAnchorVector(Anchor);
            
            GameCore.Current.SpriteBatch.DrawString(
                Font, 
                Text,  
                GameObject.Position, 
                VertexColor, 
                GameObject.Rotation, 
                origin, 
                Vector2.One * FontSize, 
                SpriteEffects.None, 
                0);
        }
        
        public void Initialize(GameObject go, string path)
        {
            _gameObject = go;
            try
            {
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
            Initialize(go, _asset.Name);
        }
        
    }
}