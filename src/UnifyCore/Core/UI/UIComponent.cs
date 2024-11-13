
using Microsoft.Xna.Framework;

namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="n"/> class, an abstract extension of <see cref="Component"/>,
    /// it serves as a base class for UI elements in the game view.
    /// </summary>
    public abstract class UIComponent : Component
    {
        public Vector2 Origin { get; set; }

        public enum AnchorType
        {
            UpperLeft = 0,
            UpperCenter = 1,
            UpperRight = 2,
            MiddleLeft = 3,
            MiddleCenter = 4,
            MiddleRight = 5,
            LowerLeft = 6,
            LowerCenter = 7,
        }
        public AnchorType Anchor { get; set; }
        
        public abstract void Draw();
    }
}