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

        public virtual void LateLoad(Game game, GameObject go)
        {

        }

        public virtual void Update(GameCore game)
        {

        }


        ///To be replaced by FixedUpdate later on
        public virtual void PhysicsUpdate(GameCore game)
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
