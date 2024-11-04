using System;
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
        public SpriteFont Font { get; set; }

        [JsonProperty]
        private GameAsset _asset;

        public override void Draw()
        {
            if (string.IsNullOrEmpty(Text) || Font == null) return;

            Debug.Log("draw");
            GameCore.Current.SpriteBatch.DrawString(Font, Text, GameObject.Position, VertexColor, GameObject.Rotation, Vector2.Zero, Size, SpriteEffects.None, 0);
        }
        
        public void Initialize(Game game, GameObject go, string path)
        {
            _gameObject = go;
            try
            {
                path = path.Remove(0,1);
                Font = game.Content.Load<SpriteFont>($"./Assets/{path}");
                Debug.Log(Font == null);
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