using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D
{
    /// <summary>
    /// Handles all of the keyboard inputs in the editor
    /// </summary>
    internal class InputsManager
    {
        private KeyboardState _previousKeyboardState;

        public void Update(GameEditor gameEditor, GameTime gameTime)
        {
            // Poll input
            KeyboardState currentState = Keyboard.GetState();

            // Check for presses
            if (currentState.IsKeyDown(Keys.LeftControl) && currentState.IsKeyDown(Keys.S) && _previousKeyboardState.IsKeyUp(Keys.S))
            {
                System.Console.WriteLine("Ctrl+S");
                gameEditor.GameEditorUI.SaveCurrentScene();
            }
            _previousKeyboardState = currentState;
        }
    }
}
