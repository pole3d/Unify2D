using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core.Graphics;

namespace Unify2D.Core
{
    public class GameObject
    {
        public Vector2 Position{ get; set; }
        public string Name { get; set; }
        public Vector2 BoundingSize { get; set; } = new Vector2(30, 30);


        [JsonIgnore]
        public IEnumerable<Component> Components => _components;

        private static JsonSerializerSettings s_serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }; //type name should be read
        
        List<Renderer> _renderers;

        [JsonProperty]
        List<Component> _components;

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
        
        public static GameObject Instantiate(string originalAssetName)
        {
            // Get serialized text
            string serializedText = System.IO.File.ReadAllText($"./Assets/{originalAssetName}.prefab");
            // Create gameObject
            GameObject go = JsonConvert.DeserializeObject<GameObject>(serializedText, s_serializerSettings);
            go.Load(GameCore.Current.Game);
            GameCore.Current.AddGameObject(go);
            return go;
            // TODO: LIST GAMEOBJECTS TO ADD
        }
    }
}
