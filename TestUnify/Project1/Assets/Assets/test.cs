using System.Net.Http.Headers;
using Unify2D.Core;
using Input = Microsoft.Xna.Framework.Input;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GameAssembly
{
       
    class TestA : Component
    {         
  
          

        public float Speed { get; set; }
        public float Speed7 { get; set; }  
        public string Text { get; set; }
        public override void Update(GameCore game)
        {  
            Vector2 direction = new Vector2();  

            if (Input.Keyboard.GetState().IsKeyDown(Input.Keys.Up)) 
            {
                direction -= Vector2.UnitY;    
            }
            if (Input.Keyboard.GetState().IsKeyDown(Input.Keys.Down))
            {
                direction += Vector2.UnitY;
            }
            if (Input.Keyboard.GetState().IsKeyDown(Input.Keys.Right))
            {
                direction += Vector2.UnitX;  
            }
            if (Input.Keyboard.GetState().IsKeyDown(Input.Keys.Left))
            {
                direction -= Vector2.UnitX;
            }

            if (direction.LengthSquared() > 0)
            {
                direction.Normalize();
                _gameObject.Position += direction * Speed * game.DeltaTime;
            }
        }

    }
}