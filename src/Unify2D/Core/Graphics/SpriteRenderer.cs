using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core.Graphics
{
    class SpriteRenderer : Renderer
    {
        Texture2D _texture;

        public void Initialize( Game game, string pictures)
        {
            _texture = game.Content.Load<Texture2D>(pictures);
        }

        public override void Draw(GameEditor game)
        {
            game.spriteBatch.Draw(_texture, new Vector2(10, 10), Color.White);

        }
    }
}
