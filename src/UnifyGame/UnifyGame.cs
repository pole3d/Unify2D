using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D;
using Unify2D.Builder;
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

            //Console.WriteLine(GameSettings.Instance.ScenesInGame.Count + " scene count");

            //SceneManager.Instance.LoadScene(0);

            _core.GameObjects.Clear();
            try
            {
                string path = Directory.GetCurrentDirectory();
                string pathJson = path+ "\\SceneJson.json";
                if (File.Exists(pathJson))
                {
                    //string json = File.ReadAllText(pathJson);
                    //List<SceneInfo> deserializedNames = System.Text.Json.JsonSerializer.Deserialize<List<SceneInfo>>(json);
                    //foreach (var name in deserializedNames)
                    //{
                    //    GameSettings.Instance.AddSceneToList(name);
                    //}
                    //SceneManager.Instance.LoadScene(GameSettings.Instance.ScenesInGame[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Can't load scene" + ex.ToString());
            }

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _core.Update(gameTime);
            SceneManager.Instance.CurrentScene.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {
            if ( Camera.Main == null)
            {
                Console.WriteLine( "There's no camera on this scene" );
                GameObject go = new GameObject();
                go.AddComponent<Camera>();
            }

            Camera camera = Camera.Main;

            if (camera != null)
            {
                GraphicsDevice.Clear(camera.Background);

                _core.BeginDraw(camera.Matrix);
                SceneManager.Instance.CurrentScene.Draw();
                _core.EndDraw();
            }
            else
            {
                GraphicsDevice.Clear(Color.Gray);
            }
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



