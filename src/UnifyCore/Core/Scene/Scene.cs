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
                settings.SerializationBinder = GuidTypeBinder.Instance;
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

            try
            {
                foreach (GameObject gameObject in GameObjects)
                {
                    gameObject.Initialize(GameCore.Current.Game);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + e.StackTrace);
            }


            _isLoaded = true;

            UpdateCanvasList();
            _canvasList.ForEach(x => x.UpdateList());
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

            PhysicsSettings.World.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

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
