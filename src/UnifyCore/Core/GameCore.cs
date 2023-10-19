using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core
{
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

        static GameCore s_current;

        List<GameObject> _gameObjects;
        List<GameObject> _gameObjectsToDestroy = new List<GameObject>();
        Game _game;

        public GameCore(Game game)
        {
            _game = game;
            _gameObjects = new List<GameObject>();
        }

        public void AddGameObject(GameObject go)
        {
            _gameObjects.Add(go);
        }

        public void Draw()
        {
            Draw(Matrix.Identity);
        }
        public void Draw(Matrix matrix)
        {
            //SpriteBatch.Begin( SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            
            SpriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.NonPremultiplied,
                        null,
                        null,
                        null,
                        null,
                        matrix);
            


            foreach (var item in _gameObjects)
            {
                item.Draw();
            }
            SpriteBatch.End();
        }

        public void Destroy(GameObject item)
        {
            _gameObjectsToDestroy.Remove(item);
        }

        public void DestroyImmediate(GameObject item)
        {
            _gameObjects.Remove(item);
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            SpriteBatch = new SpriteBatch(graphicsDevice);
        }

        public void LoadScene(Game game, List<GameObject> gameObjects)
        {
            foreach (var item in gameObjects)
            {
                AddGameObject(item);
                item.Load(game);
            }
        }

        public void Update()
        {
            foreach (var item in _gameObjects)
            {
                item.Update(this);
            }

            foreach (var item in _gameObjectsToDestroy)
            {
                _gameObjects.Remove(item);
            }

            _gameObjectsToDestroy.Clear();
        }
    }
}
