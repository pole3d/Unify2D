using Unify2D.Core;
using Input = Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GameAssembly
{
    class TestA : Component
    {
        public float Speed { get; set; }
        public float Speed11 { get; set; }

        public override void Update(GameCore game)
        {
            var state = Input.Keyboard.GetState();

            if (state.IsKeyDown(Input.Keys.Up) )
            {
                _gameObject.Position -= Vector2.UnitY * Speed;
            }
            if (state.IsKeyDown(Input.Keys.Down) )
            {
                _gameObject.Position += Vector2.UnitY * Speed;
            }
            if (state.IsKeyDown(Input.Keys.Right) )
            {
                _gameObject.Position += Vector2.UnitX * Speed;
            }
            if (state.IsKeyDown(Input.Keys.Left) )
            {
                _gameObject.Position -= Vector2.UnitX * Speed;
            }
        }
    }
}