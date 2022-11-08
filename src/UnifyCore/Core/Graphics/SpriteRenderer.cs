using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core.Graphics
{
    public class SpriteRenderer : Renderer
    {
        public Color Color { get; set; } = Color.White;

        [JsonProperty]
        GameAsset _asset;
        GameObject _go;
        Texture2D _texture;

        public void Initialize( Game game, GameObject go, string path)
        {
            _go = go;

            try
            {
                _texture = game.Content.Load<Texture2D>(path);
                _asset = new GameAsset(_texture, _texture.Name);
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
