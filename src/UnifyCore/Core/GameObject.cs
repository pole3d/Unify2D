using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core.Graphics;

namespace Unify2D.Core
{
    public class GameObject
    {
        public Vector2 Position{ get; set; }
        public string Name { get; set; }

        Renderer _renderer;
        List<Component> _components;

        public GameObject()
        {
            _components = new List<Component>();
            Name = "GameObject";

            GameCore.Current.AddGameObject(this);
        }


        public T AddComponent<T>() where T : new()
        {
            T component = new();
            if ( component is Renderer renderer)
            {
                _renderer = renderer;
            }

            return component;
        }

        public bool HasRenderer()
        {
            return _renderer != null;
        }

        internal void Draw()
        {
            if ( _renderer != null )
                _renderer.Draw();
        }
    }
}
