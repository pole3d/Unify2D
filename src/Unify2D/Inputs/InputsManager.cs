using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Unify2D.Inputs
{
    /// <summary>
    /// The <see cref="InputsManager"/> class
    /// handles all of the keyboard inputs within the editor,
    /// such as shortcuts (save, play, open scene...).
    /// </summary>
    internal class InputsManager
    {
        private KeyboardState _previousKeyboardState;
        private GameEditor _gameEditor;

        private Dictionary<Input, System.Action> _inputs = new Dictionary<Input, System.Action>();

        public void Initialize(GameEditor gameEditor)
        {
            _gameEditor = gameEditor;
            _inputs.Add(new Input(Keys.S, true), SaveCurrentScene);
            _inputs.Add(new Input(Keys.O, true), LoadScene);
            _inputs.Add(new Input(Keys.F5), Build);
        }

        public void Update(GameTime gameTime)
        {
            // Poll input
            KeyboardState currentState = Keyboard.GetState();

            ProcessKeys(currentState);

            // Set the previous keyboard state for the next frame
            _previousKeyboardState = currentState;
        }


        private void ProcessKeys(KeyboardState state)
        {
            foreach (var inputMethodPair in _inputs)
            {
                var input = inputMethodPair.Key;
                var key = input.Key;
                if (state.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key) && (input.PressControl == false || state.IsKeyDown(Keys.LeftControl)))
                {
                    inputMethodPair.Value?.Invoke();
                }
            }
        }

        #region Methods
        private void SaveCurrentScene()
        {
            Debug.Log("Scene saved!");
            _gameEditor.GameEditorUI.SaveCurrentScene();
        }

        private void LoadScene()
        {
            _gameEditor.GameEditorUI.LoadScene();
        }

        private void Build()
        {
            _gameEditor.GameEditorUI.Build();
        }
        #endregion
    }
}
