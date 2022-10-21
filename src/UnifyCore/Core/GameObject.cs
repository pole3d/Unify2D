using Microsoft.Xna.Framework;
using Newtonsoft.Json;
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
        public Vector2 BoundingSize{ get; set; }

        [JsonIgnore]
        public IEnumerable<Component> Components => _components;

        List<Renderer> _renderers;

        [JsonProperty]
        List<Component> _components;

        public GameObject()
        {
            _components = new List<Component>();
            _renderers = new List<Renderer>();
            Name = "GameObject";

            GameCore.Current.AddGameObject(this);
        }

        internal void Load(Game game)
        {
            foreach (var component in _components)
            {
                component.Load(game,this);
                if (component is Renderer renderer)
                {
                    _renderers.Add(renderer);
                }
            }
        }


        public T AddComponent<T>() where T : Component,new()
        {
            T component = new();
            if ( component is Renderer renderer)
            {
                _renderers.Add(renderer);
            }

            _components.Add(component);

            return component;
        }

        public bool HasRenderer()
        {
            return _renderers.Count > 0;
        }

        internal void Draw()
        {
            foreach (var item in _renderers)
            {
                item.Draw();
            }
        }

 
    }
}
