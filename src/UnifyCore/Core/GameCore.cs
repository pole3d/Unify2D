﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Unify2D.Physics;
using Unify2D.Core.Tools;
using UnifyCore;
using UnifyCore.Core.Tweens;

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
        public float DeltaTime { get; private set; }

        static GameCore s_current;

        private Game _game;

        public GameCore(Game game)
        {
            _game = game;
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            SpriteBatch = new SpriteBatch(graphicsDevice);
            ResourcesManager = new ResourcesManager();
            AssetsManager = new AssetsManager();

            InitPhysics();
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

            TweenManager.Instance.Update(DeltaTime);
        }


    }
}
