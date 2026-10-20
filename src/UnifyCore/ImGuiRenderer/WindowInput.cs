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
        internal PlatformWindows PlatformWindows { get; set; }
        public bool IsActive => PlatformWindows?.IsActive ?? _game.IsActive;
        public KeyboardState Keyboard { get; private set; }
        public int FocusVersion { get; private set; }
        public MouseState Mouse { get; private set; }
        private bool _wasActive;

        public WindowInput(Game game) => _game = game;

        public void Capture()
        {
            var mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            if (PlatformWindows != null)
            {
                uint buttons = SDL2.SDL.SDL_GetGlobalMouseState(out int x, out int y);
                PlatformWindows.UpdateMouseCapture(buttons);
                buttons &= PlatformWindows.MouseButtons;
                ButtonState Button(int index) => (buttons & (1u << (index - 1))) != 0 ? ButtonState.Pressed : ButtonState.Released;
                mouse = new MouseState(x, y, mouse.ScrollWheelValue, Button(1), Button(2), Button(3), Button(4), Button(5));
            }
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
