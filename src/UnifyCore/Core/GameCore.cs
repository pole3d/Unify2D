using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Unify2D.Physics;
using Unify2D.Core.Tools;

namespace Unify2D.Core
{
    /// <summary>
    /// Represents the core game engine
    /// This core is used for the game and the editor
    /// It manages the components and the helps the rendering 
    /// </summary>
    public class GameCore
    {
        public static GameCore Current
        {
            get
            {
                return s_current;
            }
        }

        public static void SetCurrent(GameCore core)
        {
            s_current = core;
        }

        public Game Game => _game;

        public SpriteBatch SpriteBatch { get; private set; }
        public List<GameObject> GameObjects => _gameObjects;

        public PhysicsSettings PhysicsSettings { get; private set; }
        public float DeltaTime { get; private set; }

        static GameCore s_current;

        Game _game;
        
        protected List<GameObject> _gameObjects = new List<GameObject>();
        protected List<GameObject> _gameObjectsToInstantiate = new List<GameObject>();
        protected List<GameObject> _gameObjectsToDestroy = new List<GameObject>();

        private List<PrefabInstance> _prefabInstances = new List<PrefabInstance>();


        public GameCore(Game game)
        {
            _game = game;
        }

        public void InitPhysics()
        {
            if (PhysicsSettings == null)
                PhysicsSettings = new PhysicsSettings();

            PhysicsSettings.Init();
        }
        
        #region OldWayGameObjects
        public void AddGameObject(GameObject go) {
            _gameObjectsToInstantiate.Add(go);
        }
        
        public void AddGameObjectImmediate(GameObject go)
        {
            _gameObjects.Add(go);
        }
        public void Destroy(GameObject item)
        {
            _gameObjectsToDestroy.Add(item);
        }
        public void DestroyImmediate(GameObject item)
        {
            // Remove item from gameObjects list
            _gameObjects.Remove(item);
            
            // Remove prefab instance
            if (item.PrefabInstance != null)
                _prefabInstances.Remove(item.PrefabInstance);
        }
        public void RefreshGameObjectListImmediate()
        {
            while (_gameObjectsToInstantiate.Count > 0)
            {
                AddGameObjectImmediate(_gameObjectsToInstantiate[0]);
                _gameObjectsToInstantiate.RemoveAt(0);
            }
            
            foreach (var item in _gameObjectsToDestroy)
            {
                DestroyImmediate(item);
            }
            _gameObjectsToDestroy.Clear();
        }
        
        #endregion

        public void BeginDraw()
        {
            BeginDraw(Matrix.Identity);
        }
        public void BeginDraw(Matrix matrix)
        {
            SpriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.NonPremultiplied,
                        null,
                        null,
                        null,
                        null,
                        matrix);
        }
        public void DrawGizmo()
        {
            Gizmo.SetColor(Color.White);
            foreach (var item in SceneManager.Instance.CurrentScene.GameObjects)
            {
                item.DrawGizmo();
            }
        }
        public void EndDraw()
        {
            SpriteBatch.End();
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            SpriteBatch = new SpriteBatch(graphicsDevice);

            InitPhysics();
        }

        public void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        
        public void AddPrefabInstance(PrefabInstance pi)
        {
            _prefabInstances.Add(pi);
            if (pi.LinkedGameObject == null)
            {
                pi.InstantiateAndLinkGameObject();
            }
        }
    
        public void LoadScene(Game game, SceneData data)
        {
            _gameObjects.Clear();
            foreach (var item in data.GameObjects)
            {
                AddGameObjectImmediate(item);
                item.Init(game);
            }
            
            _prefabInstances.Clear();
            foreach (PrefabInstance item in data.PrefabInstances)
            {
                AddPrefabInstance(item);
            }
        }
        
        public SceneData GetSceneData()
        {
            // Set Name property of prefab instances
            foreach (PrefabInstance prefabInstance in _prefabInstances)
            {
                if (string.IsNullOrEmpty(prefabInstance.Name) && prefabInstance.LinkedGameObject != null)
                    prefabInstance.Name = prefabInstance.LinkedGameObject.Name;
            }

            // Don't serialize gameObjects from prefab instances
            List<GameObject> gameObjects = new List<GameObject>(_gameObjects);
            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                if (gameObjects[i].PrefabInstance != null)
                    gameObjects.RemoveAt(i);
            }

            return new SceneData(gameObjects, _prefabInstances);
        }

    }
}
