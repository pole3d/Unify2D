using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core.Graphics;

namespace Unify2D.Core
{
    class GameObject
    {
        Renderer _renderer;
        List<Component> _components;

        public GameObject()
        {
            _components = new List<Component>();
        }

        internal T AddComponent<T>() where T : new()
        {
            T component = new T();
            if ( component is Renderer renderer)
            {
                _renderer = renderer;
            }

            return component;
        }

        internal void Draw(GameEditor game)
        {
            if ( _renderer != null )
                _renderer.Draw(game);
        }
    }
}
