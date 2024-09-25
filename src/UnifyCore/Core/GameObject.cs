using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using Unify2D.Core.Graphics;

namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="GameObject"/> class represents an object in a game world,
    /// providing properties for its position, rotation, scale, name, and a list of components.
    /// This class serves as a fundamental building block for constructing scenes in a game.
    /// </summary>
    public class GameObject
    {
        public static ulong s_maxID = 0;
        public ulong UID { get; set; }



        public string Name { get; set; }

        public Vector2 Position
        {
            get { return GetParentPosition() + LocalPosition; }
            set
            {
                LocalPosition = value - GetParentPosition();
                m_positionUpdated = true;
            }
        }

        public Vector2 LocalPosition { get; set; }

        public float Rotation { get { return m_rotation; } set { m_rotation = value; m_rotationUpdated = true; } }
        public Vector2 Scale { get; set; } = new Vector2(1, 1);
        public Vector2 BoundingSize { get; set; } = new Vector2(30, 30);
        public bool PositionUpdated { get { return m_positionUpdated; } }
        public bool RotationUpdated { get { return m_rotationUpdated; } }
        public List<GameObject> Children { get; set; }
        [JsonIgnore]
        public GameObject Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<Component> Components => _components;

        private Vector2 m_position;
        private float m_rotation;
        private bool m_positionUpdated, m_rotationUpdated;




        List<Renderer> _renderers;

        [JsonProperty]
        List<Component> _components;

        public GameObject()
        {
            _components = new List<Component>();
            _renderers = new List<Renderer>();
            Name = "GameObject";

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

        public static void SetChild(GameObject parent, GameObject child)
        {
            if (parent.Children == null)
                parent.Children = new List<GameObject>();
            
            child.Parent = parent;
            parent.Children.Add(child);
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
                component.Load(game,this);

                if (component is Renderer renderer)
                {
                    _renderers.Add(renderer);
                }
            }

            foreach (var component in _components)
            {
                component.LateLoad(game, this);
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

            //ui
            if (component is Canvas canvas)
            {
                GameCore.Current.CanvasListList.Add(canvas);
            }
            else if (component is UIComponent)
            {
                if (HasCanvasInParents(out Canvas _) == false)
                {
                    if (Parent != null)
                    {
                        Parent.Children.Remove(this);
                        Parent = null;
                    }
                    
                    if (GameCore.Current.HasCanvas(out Canvas gameCoreCanvas) == false)
                    {
                        GameObject canvasGameObject = Create();
                        canvasGameObject.Name = "Canvas";
                        canvasGameObject.AddComponent<Canvas>();
                        
                        SetChild(canvasGameObject, this);
                    }
                    else
                    {
                        SetChild(gameCoreCanvas.GameObject, this);
                    }
                }
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
            
            ///To be refactored as FixedUpdate later
            foreach (var item in _components)
            {
                item.PhysicsUpdate(core);
            }

            m_positionUpdated = false;
            m_rotationUpdated = false;
        }

        public void RemoveComponent(Component item)
        {
            DestroyComponent(item);

            _components.Remove(item);
        }

        public void ClearComponents()
        {
            foreach (var item in _components)
            {
                DestroyComponent(item);
            }

            _components.Clear();
        }
        
        private void DestroyComponent(Component component)
        {
            if (component is Renderer renderer)
            {
                _renderers.Remove(renderer);
            }
            
            if (component is Canvas canvas)
            {
                Debug.Log("remove from canvas list");
                GameCore.Current.CanvasListList.Remove(canvas);
            }

            component.Destroy();
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

        public bool HasCanvasInParents(out Canvas canvas)
        {
            canvas = null;

            GameObject currentParent = Parent;
            while (currentParent != null)
            {
                foreach (var component in currentParent._components)
                {
                    if (component is Canvas parentCanvas)
                    {
                        canvas = parentCanvas;
                        return true;
                    }
                }
                currentParent = currentParent.Parent;
            }

            return false;
        }
    }
}
