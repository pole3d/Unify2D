using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Num = System.Numerics;

namespace UnifyGame
{
    /// <summary>
    /// The main editor of Unify2D
    /// </summary>
    public class UnifyGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private Unify2D.ImGuiRenderer.Renderer _imGuiRenderer;

        GameCore _core;


        public UnifyGame()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferMultiSampling = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.PreferMultiSampling = true;
            _graphics.SynchronizeWithVerticalRetrace = false;
            
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _core = new GameCore(this);
            GameCore.SetCurrent(_core);

            _imGuiRenderer = new Unify2D.ImGuiRenderer.Renderer(this);
            _imGuiRenderer.RebuildFontAtlas();
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

            Window.AllowUserResizing = true;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _core.Initialize(GraphicsDevice);

            _core.GameObjects.Clear();
            try
            {
                string text = File.ReadAllText("./test.scene");
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;


                _core.LoadScene(this, JsonConvert.DeserializeObject<List<GameObject>>(text, settings));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't load test.scene" + ex.ToString());
            }

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _core.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {
            if ( Camera.Main == null)
            {
                Console.WriteLine( "There's no camera on this scene" );
                GameObject go = GameObject.Create();
                go.AddComponent<Camera>();
            }

            Camera camera = Camera.Main;

            GraphicsDevice.Clear(camera.Background);

            _core.BeginDraw(camera.Matrix);
            _core.Draw();
            _core.EndDraw();

            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout();


            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();
        }




        protected virtual void ImGuiLayout()
        {
            //  ImGui.ShowDemoWindow();
        }


    }


}



