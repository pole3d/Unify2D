using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Unify2D.ImGuiRenderer
{
    /// <summary>
    /// Input wrapper,
    /// use this in the editor instead of imgui directly
    /// </summary>
    public sealed class WindowInput
    {
        private readonly Game _game;
        private int _rawScroll;
        private int _scroll;
        private bool _initialized;
        public bool IsActive => _game.IsActive;
        public KeyboardState Keyboard { get; private set; }
        public int FocusVersion { get; private set; }
        public MouseState Mouse { get; private set; }
        private bool _wasActive;

        public WindowInput(Game game) => _game = game;

        public void Capture()
        {
            var mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            var keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            bool active = IsActive;
            // Discard scrolling and shortcut presses accumulated outside the application.
            if (_initialized && active && _wasActive) _scroll += mouse.ScrollWheelValue - _rawScroll;
            if (!_initialized || active != _wasActive) FocusVersion++;
            Keyboard = active ? keyboard : default;
            Mouse = active
                ? new MouseState(mouse.X, mouse.Y, _scroll, mouse.LeftButton, mouse.MiddleButton,
                    mouse.RightButton, mouse.XButton1, mouse.XButton2)
                : new MouseState(-1000000, -1000000, _scroll, ButtonState.Released, ButtonState.Released,
                    ButtonState.Released, ButtonState.Released, ButtonState.Released);
            _rawScroll = mouse.ScrollWheelValue;
            _wasActive = active;
            _initialized = true;
        }
    }
}
