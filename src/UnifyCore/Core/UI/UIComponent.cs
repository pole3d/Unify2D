namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="n"/> class, an abstract extension of <see cref="Component"/>,
    /// it serves as a base class for UI elements in the game view.
    /// </summary>
    public abstract class UIComponent : Component
    {
        public abstract void Draw();
    }
}