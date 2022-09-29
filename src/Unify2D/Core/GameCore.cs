using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core
{
    class GameCore
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

        public SpriteBatch SpriteBatch { get; private set; }
        public List<GameObject> GameObjects => _gameObjects; 

        static GameCore s_current;

        List<GameObject> _gameObjects;

        public GameCore()
        {
            _gameObjects = new List<GameObject>();
        }

        internal void AddGameObject(GameObject go)
        {
            _gameObjects.Add(go);
        }

        internal void Draw()
        {
            SpriteBatch.Begin();
            foreach (var item in _gameObjects)
            {
                item.Draw();
            }
            SpriteBatch.End();
        }

        internal void Initialize(GraphicsDevice graphicsDevice)
        {
            SpriteBatch = new SpriteBatch(graphicsDevice);
        }
    }
}
