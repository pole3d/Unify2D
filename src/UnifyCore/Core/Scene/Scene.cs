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
        public SceneInfo SceneInfo => _sceneInfo;

        public string Name => _sceneInfo.Name;
        public string Path => _sceneInfo.Path;
        public int RootCount => GameObjects.Count;
        public int BuildIndex { get; private set; }
        public List<GameObject> GameObjects { get; private set; } = new List<GameObject>();


        private SceneInfo _sceneInfo;
        private bool _isLoaded = false;

        //private bool _isDirty = false;

        [JsonIgnore] public List<Canvas> CanvasList => _canvasList;
        [JsonIgnore] private List<Canvas> _canvasList = new List<Canvas>();

        public EventSystem EventSystem => _eventSystem;
        private EventSystem _eventSystem;

        public IEnumerable<GameObject> GameObjectsWithChildren
        {
            get
            {
                foreach (var gameObject in GameObjects)
                {
                    yield return gameObject;
                    foreach (var child in gameObject.GetAllChildren())
                    {
                        yield return child;
                    }

                }
            }
        }

        private List<GameObject> _gameObjectsToDestroy = new List<GameObject>();

        public Scene(string name, string path)
        {
            SetSceneInfo(name, path);
        }

        public void LoadFromFile()
        {
            try
            {
                string text = File.ReadAllText(_sceneInfo.Path);
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                settings.Error += SilentErrors;
                settings.SerializationBinder = GameCore.Current.SerializationBinder ?? GuidTypeBinder.Instance;
                GameObjects = JsonConvert.DeserializeObject<List<GameObject>>(text, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        public void SetSceneInfo(string name, string path)
        {
            if (_sceneInfo == null)
                _sceneInfo = new SceneInfo(name, path);
            else
            {
                _sceneInfo.Name = name;
                _sceneInfo.Path = path;
            }
        }
        public void Initialize()
        {
            GameCore.Current.InitPhysics();

            foreach (GameObject gameObject in GameObjects)
            {
                try
                {
                    gameObject.Initialize(GameCore.Current.Game);
                }
                catch (Exception e)
                {
                    if (GameCore.Current.IsPlaySession) throw;
                    Debug.LogError(e.Message + e.StackTrace);
                }
            }


            _isLoaded = true;

            UpdateCanvasList();
            _canvasList.ForEach(x => x.UpdateList());
        }


        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            Console.WriteLine(e.ErrorContext.Error);
            e.ErrorContext.Handled = true;
        }

        /// <summary>
        /// Add a root game object to the scene after verifying that the same object does not already exist
        /// </summary>
        /// <param name="go">The game object to add</param>
        public void AddRootGameObject(GameObject go)
        {
            if (!GameObjects.Contains(go))
            {
                GameObjects.Add(go);
            }
        }

        /// <summary>
        /// Remove a root game object from the scene root list
        /// </summary>
        /// <param name="go">The game object to remove</param>
        public void RemoveRootGameObject(GameObject go)
        {
            GameObjects.Remove(go);
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
        public void Update(float elapsedTime)
        {
            if (_isLoaded == false)
                return;

            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObject go = GameObjects[i];
                go.Update(GameCore.Current);
            }

            //Use of DestroyImmediate to remove child object
            /// Removed Destroy Method because it only removed object from the scene list, if children, will not be removed.
            for (int i = 0; i < _gameObjectsToDestroy.Count; i++) DestroyImmediate(_gameObjectsToDestroy[i]);

            PhysicsSettings.World.Step(elapsedTime);

            if (GameCore.Current.IsPlaySession)
                GameCore.Current.Tweens.Update(elapsedTime);

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

        /// <summary>
        /// Remove a game object and all its children from the scene.
        /// </summary>
        /// <param name="gameObject">The game object to destroy</param>
        public void DestroyImmediate(GameObject gameObject)
        {
            // if a parent exist remove it from the parent children list
            if (gameObject.Parent != null) gameObject.Parent.Children?.Remove(gameObject);
            // remove it from the root game objects list
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

        public void UpdateCanvasList()
        {
            CanvasList.Clear();

            foreach (GameObject go in GameObjects)
            {
                Canvas canvas = go.GetComponent<Canvas>();
                if (canvas != null)
                {
                    CanvasList.Add(canvas);
                }
            }
        }

        public void AddEventSystem(EventSystem eventSystem)
        {
            _eventSystem = eventSystem;
        }
    }
}
