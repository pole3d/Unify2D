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
        
        public static ulong s_maxID = 0;

        public ulong UID{ get; set; }
        public string Name { get; set; }

        public Vector2 Position { 
            get { return GetParentPosition() + LocalPosition; } 
            set { LocalPosition = value - GetParentPosition(); } 
        }

        public Vector2 LocalPosition { get; set; }

        public Vector2 BoundingSize { get; set; } = new Vector2(30, 30);
        public List<GameObject> Children { get; set; }

        [JsonIgnore]
        public GameObject Parent { get; set; }

        [JsonIgnore]
        public IEnumerable<Component> Components => _components;

        List<Renderer> _renderers;

        [JsonProperty]
        List<Component> _components;

        private GameObject()
        {
           // UID = Guid.NewGuid().ToString();
            _components = new List<Component>();
            _renderers = new List<Renderer>();
           // Name = "GameObject";

           //GameCore.Current.AddRootGameObject(this);
        }

        public static GameObject Create()
        {
            GameObject go = new GameObject();
            go.UID = s_maxID++;
            GameCore.Current.AddRootGameObject(go);
            return go;
        }

        public static GameObject CreateChild(GameObject parent)
        {
            GameObject child = new GameObject();
            child.UID = s_maxID++;

            if (parent.Children == null)
                parent.Children = new List<GameObject>();

            child.Parent = parent;
            parent.Children.Add(child);
            return child;
        }



        internal void Load(Game game)
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.Parent = this;
                    child.Load(game);
                }
            }

            foreach (var component in _components)
            {
                component.Initialize(this);
                component.Load(game, this);

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
        internal void DrawGizmo()
        {
            foreach (var item in _components)
            {
                item.DrawGizmo();
            }
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
            if (item is Renderer renderer)
            {
                _renderers.Remove(renderer);
            }

            item.Destroy();
            _components.Remove(item);
        }

        public void ClearComponents()
        {
            foreach (var item in _components)
            {
                if (item is Renderer renderer)
                {
                    _renderers.Remove(renderer);
                }

                item.Destroy();
            }

            _components.Clear();
        }

        public void AddChild(GameObject child)
        {
            GameCore.Current.RemoveFromRoot(child);

            if (Children == null)
                Children = new List<GameObject>();

            child.Parent = this;
            Children.Add(child);
        }

        Vector2 GetParentPosition()
        {
            Vector2 position = Vector2.Zero;
            GameObject parent = Parent;

            while (parent != null)
            {
                position += Parent.LocalPosition;
                parent = parent.Parent;
            }

            return position;
        }
    }
}
