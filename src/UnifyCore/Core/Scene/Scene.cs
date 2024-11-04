using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;
using Unify2D.Physics;

namespace Unify2D
{
    public class Scene
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<GameObject> GameObjects { get; set; } = new List<GameObject>();

        private List<GameObject> _gameObjectsToDestroy = new List<GameObject>();

        public Scene()
        {
        }

        public Scene(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);

            try
            {
                string text = File.ReadAllText(path);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                settings.Error += SilentErrors;
                GameObjects = JsonConvert.DeserializeObject<List<GameObject>>(text, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public void Init()
        {
            foreach (GameObject gameObject in GameObjects)
            {
                gameObject.Init(GameCore.Current.Game);
            }
        }


        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }

        public void AddRootGameObject(GameObject go)
        {
            GameObjects.Add(go);
        }

        public void Draw()
        {
            foreach (var item in GameObjects)
            {
                item.Draw();
            }
        }
        public void Update(GameTime gameTime)
        {
            foreach (var item in GameObjects)
            {
                item.Update(GameCore.Current);
            }

            foreach (var item in _gameObjectsToDestroy)
            {
                GameObjects.Remove(item);
            }

            PhysicsSettings.World.Step(GameCore.Current.DeltaTime);

            _gameObjectsToDestroy.Clear();
        }

        public void Destroy(GameObject gameObject)
        {
            _gameObjectsToDestroy.Add(gameObject);
        }

        public void DestroyImmediate(GameObject gameObject)
        {
            if (gameObject.Parent != null)
            {
                gameObject.Parent.Children.Remove(gameObject);
            }
            else
            {
                GameObjects.Remove(gameObject);
            }
        }
    }
}
