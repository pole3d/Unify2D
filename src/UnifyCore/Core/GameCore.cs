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

        BlendState _blendState;
  
        public void Draw()
        {
            SpriteBatch.Begin( SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            foreach (var item in _gameObjects)
            {
                item.Draw();
            }
            SpriteBatch.End();
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            SpriteBatch = new SpriteBatch(graphicsDevice);
        }

        public void LoadScene(Game game,  List<GameObject> gameObjects)
        {
            foreach (var item in gameObjects)
            {
                item.Load(game);
            }
        }
    }
}
