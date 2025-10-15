using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
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
            get { return GetParentPosition() + LocalPosition.Rotate(Rotation); }
            set
            {
                LocalPosition = value - GetParentPosition();
                m_positionUpdated = true;
            }
        }

        public Vector2 LocalPosition { get; set; }

        public float Rotation
        {
            get { return GetParentRotation() + LocalRotation; }
            set
            {
                LocalRotation = value - GetParentRotation();
                m_rotationUpdated = true;
            }
        }
        public float LocalRotation
        {
            get { return m_rotation; }
            set
            {
                m_rotation = value;
                m_rotationUpdated = true;
            }
        }

        public Vector2 Scale = new Vector2(1, 1);

        public Vector2 BoundingSize { get; set; } = new Vector2(30, 30);
        public bool PositionUpdated { get { return m_positionUpdated; } }
        public bool RotationUpdated { get { return m_rotationUpdated; } }

        [JsonIgnore]
        public object Tag { get; set; }
        public List<GameObject> Children { get; set; }
        [JsonIgnore]
        public GameObject Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<Component> Components => _components;

        private static string _originalAssetPath;

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

        public void Initialize(Game game)
        {
            if (Children != null)
            {
                foreach (GameObject child in Children)
                {
                    child.Parent = this;
                    child.Initialize(game);
                }
            }

            foreach (Component component in _components)
            {
                component.Initialize(this);
                component.Load(game, this);

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

        public void ResetComponents()
        {
            var components = _components;
            _components = new List<Component>();

            foreach (var item in components)
            {
                AddComponent(item);
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
            else if (component is EventSystem eventSystem)
            {
                if (scene.EventSystem != null)
                {
                    return;
                }
                scene.AddEventSystem(eventSystem);
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
            component.Reset(this);

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
            GameObject current = Parent;

            while (current != null)
            {
                position += current.LocalPosition.Rotate(current.Rotation);
                current = current.Parent;
            }

            return position;
        }

        float GetParentRotation()
        {
            float rotation = 0;
            GameObject current = Parent;

            while (current != null)
            {
                rotation += current.LocalRotation;
                current = current.Parent;
            }

            return rotation;
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
            go.Initialize(GameCore.Current.Game);


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

        public void UpdateFromPrefab(GameObject updatedGameObject)
        {
            if (Tag != null)
            {
                // Apply overrides here
                foreach (var property in typeof(GameObject).GetProperties())
                {
                    if (property.CanWrite)
                    {
                        property.SetValue(this, property.GetValue(updatedGameObject));
                    }
                }

                foreach (var field in typeof(GameObject).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                {
                    field.SetValue(this, field.GetValue(updatedGameObject));
                }
            }
        }
    }
    public static class GameObjectExtensions
    {
        public static GameObject DeepCopy(this GameObject original)
        {
            // Serialize the original object to JSON
            string json = JsonConvert.SerializeObject(original, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            // Deserialize the JSON to a new object
            GameObject copy = JsonConvert.DeserializeObject<GameObject>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return copy;
        }
    }
}

