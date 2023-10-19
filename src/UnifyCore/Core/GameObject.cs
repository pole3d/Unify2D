using ImGuiNET;
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
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; } = new Vector2(1, 1);
        public string Name { get; set; }
        public Vector2 BoundingSize { get; set; } = new Vector2(30, 30);

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
                component.Initialize(this);
                component.Load(game,this);

                if (component is Renderer renderer)
                {
                    _renderers.Add(renderer);
                }
            }
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

        public T GetComponent<T>() where T : Component
        {
            foreach (var item in Components)
            {
                if (item is T)
                {
                    return (item as T);
                }
            }

            return null;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T component = new();
            AddComponent(component);

            return component;
        }

        public void AddComponent(Component component)
        {
            if (component is Renderer renderer)
            {
                _renderers.Add(renderer);
            }

            component.Initialize(this);

            _components.Add(component);
        }

        internal void Update(GameCore core)
        {
            foreach (var item in _components)
            {
                item.Update(core);
            }
        }

        public void RemoveComponent(Component item)
        {
            _components.Remove(item);
        }

        public void DrawComponentGizmosSelected(ImDrawListPtr drawList)
        {
            foreach (var c in Components)
            {
                c.DrawGizmoOnSelected(drawList);
            }
        }
    }
}
