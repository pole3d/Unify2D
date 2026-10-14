using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Unify2D.Physics;
using Unify2D.Core.Tools;
using UnifyCore;
using UnifyCore.Core.Tweens;
using System;

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
        public ResourcesManager ResourcesManager { get; private set; }
        public AssetsManager AssetsManager { get; private set; }

        public List<GameObject> GameObjects => SceneManager.Instance.CurrentScene.GameObjects;

        public PhysicsSettings PhysicsSettings { get; private set; }
        public SceneManager SceneManager { get; }
        public TweenManager Tweens { get; } = new TweenManager();
        public Unify2D.Core.Graphics.Camera MainCamera { get; set; }
        public Unify2D.Builder.GameSettings Settings { get; }
        public Newtonsoft.Json.Serialization.ISerializationBinder SerializationBinder { get; set; }
        public bool IsPlaySession { get; set; }
        public float DeltaTime { get; private set; }

        static GameCore s_current;

        private Game _game;
        private bool _ownsResources;
        private bool _graphicsResourcesDisposed;



        public GameCore(Game game, SceneManager sceneManager = null, Unify2D.Builder.GameSettings settings = null)
        {
            _game = game;
            SceneManager = sceneManager ?? Unify2D.SceneManager.Instance;
            Settings = settings ?? Unify2D.Builder.GameSettings.Instance;
            PhysicsSettings = new PhysicsSettings();
        }

        public void Initialize(GraphicsDevice graphicsDevice, ResourcesManager sharedResources = null)
        {
            GraphicsDevice = graphicsDevice;

            SpriteBatch = new SpriteBatch(graphicsDevice);
            _ownsResources = sharedResources == null;
            _graphicsResourcesDisposed = false;
            ResourcesManager = sharedResources ?? new ResourcesManager();
            AssetsManager = new AssetsManager();

            InitPhysics();
        }

        public void DisposeGraphicsResources()
        {
            if (_graphicsResourcesDisposed) return;
            _graphicsResourcesDisposed = true;
            SpriteBatch?.Dispose();
            if (_ownsResources) ResourcesManager?.Dispose();
        }

        public void InitPhysics()
        {
            PhysicsSettings ??= new PhysicsSettings();

            PhysicsSettings.Init();
        }

        public void BeginDraw()
        {
            BeginDraw(Matrix.Identity);
        }

        public void BeginDraw(Matrix matrix)
        {
            SpriteBatch.Begin(SpriteSortMode.Deferred,
                        BlendState.NonPremultiplied, SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        matrix);
        }

        public void DrawGizmo()
        {
            Gizmo.SetColor(Color.White);
            if (SceneManager.Instance.CurrentScene != null)
            {
                foreach (var item in SceneManager.Instance.CurrentScene.GameObjects)
                {
                    item.DrawGizmo();
                }
            }
        }
        public void EndDraw()
        {
            SpriteBatch.End();

            if (!IsPlaySession)
                TweenManager.Instance.Update(DeltaTime);
        }

        public void SetDeltaTime(float timeSeconds)
        {
            DeltaTime = timeSeconds;
        }
    }
}
