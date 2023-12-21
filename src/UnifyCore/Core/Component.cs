using Microsoft.Xna.Framework;

namespace Unify2D.Core
{
    public class Component
    {
        protected GameObject _gameObject;

        public virtual void Initialize(GameObject go)
        {
            _gameObject = go;
        }

        public virtual void Load(Game game, GameObject go)
        {
        }

        public virtual void Update(GameCore game)
        {
        }

        internal virtual void Destroy()
        {
            
        }

        internal virtual void DrawGizmo()
        {
            
        }
    }
}
