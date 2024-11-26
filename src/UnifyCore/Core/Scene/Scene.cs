using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Core;
using Unify2D.Physics;

namespace Unify2D
{
    public class Scene
    {
        private SceneInfo _sceneInfo;
        public SceneInfo SceneInfo => _sceneInfo;

        public string Name => _sceneInfo.Name;
        public string Path => _sceneInfo.Path;
        public int RootCount => GameObjects.Count;
        public int BuildIndex { get; private set; }
        public List<GameObject> GameObjects { get; private set; } = new List<GameObject>();

        private bool _isLoaded = false;

        private bool _isDirty = false;

        public List<Canvas> CanvasList => _canvasList;
        private List<Canvas> _canvasList = new List<Canvas>();

        public EventSystem EventSystem => _eventSystem;
        private EventSystem _eventSystem;

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

        public Scene(string path, bool save = false)
        {
            SaveSceneNameAndPath(System.IO.Path.GetFileName(path), path);

            if (save == true)
                SceneManager.Instance.Save(this);

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

        public void SaveSceneNameAndPath(string name, string path)
        {
            if (_sceneInfo == null)
                _sceneInfo = new SceneInfo(name, path);
            else
            {
                _sceneInfo.Name = name;
                _sceneInfo.Path = path;
            }
        }
        public void Init()
        {
            GameCore.Current.InitPhysics();
            foreach (GameObject gameObject in GameObjects)
            {
                gameObject.Init(GameCore.Current.Game);
            }

            _isLoaded = true;
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
            if (_isLoaded == false)
                return;

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObject go = GameObjects[i];
                go.Draw();

                Canvas canvas = go.GetComponent<Canvas>();
                if (canvas != null) canvas.Draw();
            }
        }
        public void Update(GameTime gameTime)
        {
            if (_isLoaded == false)
                return;

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObject go = GameObjects[i];
                go.Update(GameCore.Current);
            }

            for (int i = 0; i < _gameObjectsToDestroy.Count; i++)
                GameObjects.Remove(_gameObjectsToDestroy[i]);

            PhysicsSettings.World.Step(GameCore.Current.DeltaTime);

            _gameObjectsToDestroy.Clear();
        }
        public void UpdateCanvas()
        {
            _canvasList.ForEach(x => x.UpdateList());
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

        public bool HasCanvas(out Canvas canvas)
        {
            if (_canvasList == null)
            {
                _canvasList = new List<Canvas>();
            }

            canvas = null;
            if (_canvasList.Count <= 0) return false;

            _canvasList.RemoveAll(x => x == null);

            if (_canvasList.Count <= 0) return false;

            canvas = _canvasList[0];

            return true;
        }

        public void ClearScene()
        {
            _isLoaded = false;
            GameObjects.Clear();
        }

        public void AddEventSystem(EventSystem eventSystem)
        {
            _eventSystem = eventSystem;
        }
    }

    public class SceneInfo
    {
        public SceneInfo(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public int BuildIndex { get; set; }

    }
}
