using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;


namespace Unify2D.Core.Graphics
{
    public class SpriteRenderer : Renderer
    {
        public Color Color { get; set; } = Color.White;
        public float LayerDepth { get; set; } = 0f;
        [JsonIgnore]
        public GameAsset Asset => _asset;

        [JsonProperty]
        GameAsset _asset;
        Texture2D _texture;

        public void Initialize( Game game, GameObject go, string path)
        {
            _gameObject = go;
            try
            {
                _texture = game.Content.Load<Texture2D>(   $"./Assets/{path}");
                _asset = new GameAsset(_texture, path);
                _gameObject.BoundingSize = new Vector2(_texture.Width, _texture.Height);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

        }

        internal override void Destroy()
        {
            _texture = null;
            _asset.Release();
        }

        public override void Load(Game game , GameObject go)
        {
            Initialize(game, go, _asset.Name);
        }

        public override void Draw()
        {
            if (_texture == null)
                return;

            GameCore.Current.SpriteBatch.Draw(_texture, _gameObject.Position, 
                null, Color, _gameObject.Rotation, _gameObject.BoundingSize / 2, _gameObject.Scale, 
                SpriteEffects.None, LayerDepth);
        }
    }
}
