using Microsoft.Xna.Framework.Input;

namespace Unify2D.Inputs
{
    internal struct Input
    {
        public Keys Key { get; private set; }
        public bool PressControl { get; private set; }

        public Input(Keys key, bool pressControl = false)
        {
            Key = key;
            PressControl = pressControl;
        }
    }
}
