using Unify2D.Core;
using Input = Microsoft.Xna.Framework.Input;

namespace GameAssembly
{

 
    class TestA : Component
    {
        public float Speed { get; set; }
  public float SpeedO { get; set; }
        public override void Update(GameCore game)
        {
            if (Input.Keyboard.GetState().IsKeyDown(Input.Keys.Up) )
            {
                _gameObject.Position -= Microsoft.Xna.Framework.Vector2.UnitY * Speed;
            }
            if (Input.Keyboard.GetState().IsKeyDown(Input.Keys.Down) )
            {
                _gameObject.Position += Microsoft.Xna.Framework.Vector2.UnitY * Speed;
            }
            if (Input.Keyboard.GetState().IsKeyDown(Input.Keys.Right) )
            {
                _gameObject.Position += Microsoft.Xna.Framework.Vector2.UnitX * Speed;
            }
            if (Input.Keyboard.GetState().IsKeyDown(Input.Keys.Left) )
            {
                _gameObject.Position -= Microsoft.Xna.Framework.Vector2.UnitX * Speed;
            }

        }
    }

}