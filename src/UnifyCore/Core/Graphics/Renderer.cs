using Microsoft.Xna.Framework;

namespace Unify2D.Core.Graphics
{
    /// <summary>
    /// The <see cref="Renderer"/> class, an abstract extension of <see cref="Component"/>,
    /// it serves as a base class for visual elements in the game world. 
    /// It introduces the abstract <see cref="Draw"/> function, allowing derived renderers to define
    /// their specific drawing logic. This class is intended to be inherited by specialized
    /// renderer types, such as sprite or mesh renderers, providing a foundation for visual
    /// representation within the game environment.
    /// </summary>
    public abstract class Renderer : Component
    {

        public override void Load(Game game, GameObject go)
        {

        }

        public abstract void Draw();
    }
}
