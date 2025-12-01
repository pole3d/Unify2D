using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unify2D.Core.Graphics;
using UnifyCore.Core;

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
                LocalPosition = value.Rotate(-Rotation) - GetParentPosition();
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

        public Bounds Bounds { get; set; } = new Bounds(30);
        public bool PositionUpdated { get { return m_positionUpdated; } }
        public bool RotationUpdated { get { return m_rotationUpdated; } }

        [JsonIgnore]
        public object Tag { get; set; }
        public string PrefabGUID { get; set; }
        public List<GameObject> Children { get; set; }
        [JsonIgnore]
        public GameObject Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<Component> Components => _components;
        public IEnumerable<UIComponent> UIComponents => _uiComponents;
        public int ComponentCount => _components.Count;

        private static string _originalAssetPath;

        private float m_rotation;
        private bool m_positionUpdated, m_rotationUpdated;

        private static JsonSerializerSettings s_serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }; //type name should be read

        List<Renderer> _renderers;
        List<UIComponent> _uiComponents;

        [JsonProperty]
        List<Component> _components;

        public GameObject()
        {
            _components = new List<Component>();
            _uiComponents = new List<UIComponent>();
            _renderers = new List<Renderer>();
            Name = "GameObject";
        }

        public static GameObject CreatePrefab(string guid)
        {
            var asset = GameCore.Current.AssetsManager.GetAsset(guid);
            var go = InstantiateFromPrefab(asset.Path);
            go.UID = s_maxID++;
            SceneManager.Instance.CurrentScene.AddRootGameObject(go);

            return go;
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

            //SceneManager.Instance.CurrentScene.AddRootGameObject(child);

            return child;
        }

        public static void SetChild(GameObject parent, GameObject child)
        {
            if (parent.Children == null)
                parent.Children = new List<GameObject>();

            child.Parent = parent;
            parent.Children.Add(child);
        }

        public IEnumerable<GameObject> GetAllChildren()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    yield return child;
                    foreach (var grandChild in child.GetAllChildren())
                    {
                        yield return grandChild;
                    }
                }
            }
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

            _uiComponents.Clear();
            foreach (Component component in _components)
            {
                component.Initialize(this);
                component.Load(game, this);

                if (component is Renderer renderer)
                {
                    _renderers.Add(renderer);
                }
                if (component is UIComponent uiComponent)
                {
                    _uiComponents.Add(uiComponent);
                }
            }

            foreach (var component in _components)
            {
                component.LateLoad(game, this);
            }
        }

        public Component GetComponent(int i) 
        {
            return _components[i];
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
            if (Children != null)
            {
                foreach (GameObject child in Children)
                {
                    if (child.HasRenderer())
                        return true;
                }
            }

            return _renderers.Count > 0;
        }

        public bool HasUIComponents()
        {
            return _uiComponents.Count > 0;
        }

        internal void Draw()
        {
            foreach (Renderer renderer in _renderers)
            {
                renderer.Draw();
            }

            if (Children != null)
            {
                foreach (GameObject child in Children)
                {
                    child.Draw();
                }
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

            //ui TODO : refactor this
            Scene scene = SceneManager.Instance.CurrentScene;
            var uicomponent = component as UIComponent;

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
            else if (uicomponent != null)
            {
                _uiComponents.Add(uicomponent);
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

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.Update(core);
                }
            }
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

            if (component is UIComponent uiComponent)
            {
                _uiComponents.Remove(uiComponent);
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

             sb.Insert(0, GameCore.Current.Game.AssetsPath);
 
             // Get serialized text
             string serializedText = File.ReadAllText(Path.GetFullPath(sb.ToString()));
             
             // Create gameObject
             GameObject go = JsonConvert.DeserializeObject<GameObject>(serializedText, s_serializerSettings);
             go.Initialize(GameCore.Current.Game);


            return go;
        }

        //public string GetOriginalAssetPath()
        //{
        //    return _originalAssetPath;
        //}

        internal void LinkToPrefabInstance(PrefabInstance prefabInstance)
        {
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

                for (int i = 0; i < updatedGameObject.ComponentCount;i++)
                {
                    var component = updatedGameObject.GetComponent(i);
                    foreach (var property in component.GetType().GetProperties())
                    {
                        if (property.Name == "GameObject")
                            continue;

                        if (property.CanWrite)
                        {
                            property.SetValue(_components[i], property.GetValue(component));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if a point is inside a gameObject's bounds
        /// </summary>
        /// <param name="point"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsPointInBounds(Vector2 point)
        {
            var anchor = Bounds.Pivot - new Vector2(0.5f);
            var origin = Bounds.PositionOffset;

            var sizeX = Bounds.BoundingSize.X * 0.5f * Scale.X;
            var sizeY = Bounds.BoundingSize.Y * 0.5f * Scale.Y;

            point = point.RotateAroundPoint(-Rotation, Position);

            return point.X > Position.X - origin.X - sizeX - (sizeX * 2f * anchor.X)
                && point.X < Position.X - origin.X + sizeX - (sizeX * 2f * anchor.X)
                && point.Y > Position.Y - origin.Y - sizeY - (sizeY * 2f * anchor.Y)
                && point.Y < Position.Y - origin.Y + sizeY - (sizeY * 2f * anchor.Y);
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

