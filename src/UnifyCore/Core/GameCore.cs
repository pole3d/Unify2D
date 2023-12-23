using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Physics;

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

        protected List<GameObject> _gameObjects = new List<GameObject>();
        protected List<GameObject> _gameObjectsToInstantiate = new List<GameObject>();
        protected List<GameObject> _gameObjectsToDestroy = new List<GameObject>();
        private Game _game;

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

        public virtual void DestroyImmediate(GameObject item)
        {
            _gameObjects.Remove(item);
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
        public void Draw()
        {
            foreach (var item in _gameObjects)
            {
                item.Draw();
            }
        }
        public void DrawGizmo()
        {
            foreach (var item in _gameObjects)
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
        
        public virtual void LoadScene(Game game, SceneData data)
        {
            _gameObjects.Clear();
            foreach (var item in data.GameObjects)
            {
                AddGameObjectImmediate(item);
                item.Load(game);
            }
        }

        public void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var item in _gameObjects)
            {
                item.Update(this);
            }

            RefreshGameObjectListImmediate();
            
            PhysicsSettings.World.Step(DeltaTime);
        }
    }
}
