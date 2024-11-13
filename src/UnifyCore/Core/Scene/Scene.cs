using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unify2D.Core;
using Unify2D.Physics;

namespace Unify2D
{
    public class SceneInfo
    {
        public SceneInfo(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; set; }
        public string Path { get; set; }
    }
    public class Scene
    {
        private SceneInfo _sceneInfo;
        public string Name => _sceneInfo.Name;
        public string Path => _sceneInfo.Path;
        public int RootCount => GameObjects.Count;
        public int BuildIndex { get; private set; }

        public List<GameObject> GameObjects { get; private set; } = new List<GameObject>();

        public IEnumerable<GameObject> GameObjectsWithChildren
        {
            get
            {
                foreach (var gameObject in GameObjects)
                {
                    yield return gameObject;

                    if (gameObject.Children != null)
                    {
                        foreach (var child in gameObject.Children)
                        {
                            yield return child;
                        }
                    }
                }
            }
        }

        private List<GameObject> _gameObjectsToDestroy = new List<GameObject>();

        public Scene()
        {
        }

        public Scene(string path)
        {
            if (_sceneInfo == null)
                _sceneInfo = new SceneInfo(path, System.IO.Path.GetFileName(path));
            else
                SaveSceneNameAndPath(path, System.IO.Path.GetFileName(path));


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

        public void SaveSceneNameAndPath(string path, string name)
        {
            _sceneInfo.Path = path;
            _sceneInfo.Name = name;
        }
        public void Init()
        {
            GameCore.Current.InitPhysics();

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
            foreach (GameObject item in GameObjects)
            {
                item.Update(GameCore.Current);
            }

            foreach (GameObject item in _gameObjectsToDestroy)
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
                gameObject.Parent.Children.Remove(gameObject);
            else
                GameObjects.Remove(gameObject);
        }




    }
}
