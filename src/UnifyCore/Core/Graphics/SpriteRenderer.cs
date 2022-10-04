using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        Texture2D _texture;
        GameObject _go;

        public void Initialize( Game game, GameObject go, string pictures)
        {
            _go = go;
            _texture = game.Content.Load<Texture2D>(pictures);
        }

        public override void Draw()
        {
            GameCore.Current.SpriteBatch.Draw(_texture, _go.Position, Color);
        }
    }
}
