using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="Component"/> class is an abstract base class that represents
    /// a component attached to a <see cref="GameObject"/>. Each <see cref="Component"/>
    /// holds a reference to its parent game object, and a <see cref="GameObject"/>, in turn,
    /// may have references to multiple <see cref="Component"/>s. This class serves as a foundation
    /// for creating specialized <see cref="Component"/>s with shared functionality.
    /// </summary>
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
