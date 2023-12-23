using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Unify2D.Core.Graphics;

namespace Unify2D.Core
{
    public class GameObject : Object
    {
        public Vector2 Position{ get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; } = new Vector2(1, 1);
        public Vector2 BoundingSize { get; set; } = new Vector2(30, 30);

        [JsonIgnore]
        public PrefabInstance PrefabInstance => _prefabInstance;

        [JsonIgnore]
        public IEnumerable<Component> Components => _components;

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
            if ( item is Renderer renderer)
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

        public GameObject GetRoot()
        {
            return this;
            // TODO VERY IMPORTANT: Implement this method after merge with the gameObject parenting branch
            // recursive : if the gameObject has no parent, return it, otherwise execute the method on its parent.
        }
        
        /// <summary>
        ///  Deserialize a prefab asset into a gameObject, load it and add it to the current core.
        /// </summary>
        public static GameObject Instantiate(string originalAssetName)
        {
            StringBuilder sb = new StringBuilder(originalAssetName);
            if (sb.ToString().StartsWith("/") == false)
                sb.Insert(0, "/");
            sb.Insert(0, GameCore.Current.Game.AssetsPath);
            if (sb.ToString().EndsWith(".prefab") == false)
                sb.Append(".prefab");
            // Get serialized text
            string serializedText = File.ReadAllText(sb.ToString());
            // Create gameObject
            GameObject go = JsonConvert.DeserializeObject<GameObject>(serializedText, s_serializerSettings);
            go.Load(GameCore.Current.Game);
            GameCore.Current.AddGameObject(go);
            return go;
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
