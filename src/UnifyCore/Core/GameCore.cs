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

        public GraphicsDevice GraphicsDevice { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public List<GameObject> GameObjects => SceneManager.Instance.CurrentScene.GameObjects;

        public List<Canvas> CanvasList => _canvasList;
        public PhysicsSettings PhysicsSettings { get; private set; }
        public float DeltaTime { get; private set; }

        static GameCore s_current;

        Game _game;

        private List<Canvas> _canvasList = new List<Canvas>();
        
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
            GraphicsDevice = graphicsDevice;
            
            SpriteBatch = new SpriteBatch(graphicsDevice);

            InitPhysics();
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
            _canvasList.RemoveAll(x => GameObjects.Contains(x.GameObject) == false);
            
            canvas = _canvasList[0];
            
            return true;
        }
    }
}