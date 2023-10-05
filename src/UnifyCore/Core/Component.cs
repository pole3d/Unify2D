using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core
{
    public class Component
    {
        protected GameObject _gameObject;

        public void Initialize(GameObject go)
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
    }
}
