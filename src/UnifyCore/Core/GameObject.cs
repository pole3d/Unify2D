using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using Unify2D.Core.Graphics;
using UnifyCore;

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

        private static string _originalAssetPath;
        
        private Vector2 m_position;
        private float m_rotation;
        private bool m_positionUpdated, m_rotationUpdated;



        [JsonIgnore]
        public PrefabInstance PrefabInstance => _prefabInstance;

        private static JsonSerializerSettings s_serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }; //type name should be read
        
        List<Renderer> _renderers;

        [JsonProperty]
        List<Component> _components;
        
        [JsonIgnore]
        private PrefabInstance _prefabInstance;

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
            SceneManager.Instance.CurrentScene.AddRootGameObject(go);
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
            
            SceneManager.Instance.CurrentScene.AddRootGameObject(child);
            
            return child;
        }

        public static void SetChild(GameObject parent, GameObject child)
        {
            if (parent.Children == null)
                parent.Children = new List<GameObject>();
            
            child.Parent = parent;
            parent.Children.Add(child);
        }

        public void Init(Game game)
        {
            if (Children != null)
            {
                foreach (GameObject child in Children)
                {
                    child.Parent = this;
                    child.Init(game);
                }
            }

            foreach (Component component in _components)
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
            foreach (Component item in Components)
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
            foreach (Renderer renderer in _renderers)
            {
                renderer.Draw();
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
            Scene scene = SceneManager.Instance.CurrentScene;
            if (component is Canvas canvas)
            {
                scene.CanvasList.Add(canvas);
            }
            else if (component is UIComponent)
            {
                bool hasCanvas = scene.HasCanvas(out Canvas gameCoreCanvas);
                if (hasCanvas && gameCoreCanvas.GameObject == this)
                {
                    Debug.Log("You can't add an UI element to a canvas");
                    component.Destroy();
                    return;
                }
                
                if (HasCanvasInParents(out Canvas _) == false)
                {
                    if (Parent != null)
                    {
                        Parent.Children.Remove(this);
                        Parent = null;
                    }
                    
                    if (hasCanvas == false)
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
            
            SceneManager.Instance.CurrentScene.UpdateCanvas();
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

            item.Destroy();
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
                SceneManager.Instance.CurrentScene.CanvasList.Remove(canvas);
            }

            component.Destroy();
            
            SceneManager.Instance.CurrentScene.UpdateCanvas();
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
        
        /// <summary>
        ///  Deserialize a prefab asset into a gameObject, load it and add it to the current core.
        /// </summary>
         public static GameObject InstantiateFromPrefab(string originalAssetName)
         {
              StringBuilder sb = new StringBuilder(originalAssetName);
             // if (sb.ToString().StartsWith("/") == false)
             //     sb.Insert(0, "/");
             sb.Insert(0, GameCore.Current.Game.AssetsPath);
             // if (sb.ToString().EndsWith(".prefab") == false)
             //     sb.Append(".prefab");
             
             // Set original asset path for prefab instance 
             _originalAssetPath = sb.ToString();
             
             // Get serialized text
             string serializedText = File.ReadAllText(Path.GetFullPath(sb.ToString()));
             
             // Create gameObject
             GameObject go = JsonConvert.DeserializeObject<GameObject>(serializedText, s_serializerSettings);
             go.Init(GameCore.Current.Game);

             
             return go;
         }

        public string GetOriginalAssetPath()
        {
            return _originalAssetPath;
        }

        internal void LinkToPrefabInstance(PrefabInstance prefabInstance)
        {
            _prefabInstance = prefabInstance;
            ApplyOverridesFromPrefabInstance(prefabInstance);
        }

        internal void ApplyOverridesFromPrefabInstance(PrefabInstance prefabInstance)
        {
            if (string.IsNullOrEmpty(prefabInstance.Name) == false)
                Name = prefabInstance.Name; //Temporary, overridden name should be saved in the override list, instead of using PrefabInstance.Name.
            // apply overrides here
        }
    }
}
