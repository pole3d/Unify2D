using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;


namespace Unify2D.Core.Graphics
{
    public class SpriteRenderer : Renderer
    {
        public Color Color { get; set; } = Color.White;
        [JsonIgnore]
        public GameAsset Asset => _asset;

        [JsonProperty]
        GameAsset _asset;
        GameObject _go;
        Texture2D _texture;

        public void Initialize( Game game, GameObject go, string path)
        {
            _go = go;

            try
            {
                _texture = game.Content.Load<Texture2D>(   $"./Assets/{path}");
                _asset = new GameAsset(_texture, path);
                _go.BoundingSize = new Vector2(_texture.Width, _texture.Height);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }

        }

        public override void Load(Game game , GameObject go)
        {
            Initialize(game, go, _asset.Name);
        }

        public override void Draw()
        {
            if (_texture == null)
                return;

            GameCore.Current.SpriteBatch.Draw(_texture, _go.Position - _go.BoundingSize / 2, Color);
        }

   
    }
}
