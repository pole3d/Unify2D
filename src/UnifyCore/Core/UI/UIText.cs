using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.IO;

namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="UIText"/> class extends the functionality of
    /// the base <see cref="UIComponent"/> class and introduces specific properties to display text 
    /// </summary>
    public class UIText : UIComponent
    {
        /// Properties
        // Used by the GameAssetPropertyViewer to set the Texture - To Change
        public GameAsset AssetTexture { get; set; }
        public string Text { get; set; } = "Lorem Ipsum";
        public Color VertexColor { get; set; } = Color.White;

        [JsonIgnore] public SpriteFont Font { get; private set; }
        //public string FontPath { get; set; }

        public float FontSize { get; set; } = 12;

        [JsonProperty] private string _textGuid;

        public void SetAsset(GameAsset asset)
        {
            _textGuid = asset.GUID;

            Font = GameCore.Current.ResourcesManager.GetFont(asset.Path);
        }

        public void SetFont(SpriteFont spriteFont)
        {
            Font = spriteFont;
        }


        public override void Load(Game game)
        {
            base.Load(game);

            var asset = GameCore.Current.AssetsManager.GetAsset(_textGuid);
            if (asset == null)
            {

                Debug.LogError($"Can't load font {_textGuid} {_gameObject.Name}");
                return;
            }

            SetAsset(asset);
        }

        public override void Draw()
        {
            if (_gameObject == null) return;

            if (string.IsNullOrEmpty(Text) || Font == null)
            {
                //SetFont(FontPath);
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
            //     _asset?.Release();
        }
    }
}