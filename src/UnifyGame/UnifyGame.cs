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
        const string JsonFolderSceneName = "\\SceneJson.json";

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
          //  _core.GameObjects.Clear();

            #region Load scene with json
            try
            {
                string currentPath = Directory.GetCurrentDirectory();
                string pathJson = currentPath + JsonFolderSceneName;
                if (File.Exists(pathJson))
                {
                    List<SceneInfo> deserializedJsonScene = System.Text.Json.JsonSerializer.Deserialize<List<SceneInfo>>(File.ReadAllText(pathJson));

                    for (int i = 0; i < deserializedJsonScene.Count; i++)
                    {
                        SceneInfo sceneInfo = deserializedJsonScene[i];
                        sceneInfo.Path = currentPath + "\\" + sceneInfo.Path;
                        sceneInfo.BuildIndex = i;
                        GameSettings.Instance.AddSceneToList(sceneInfo);
                    }

                    if (GameSettings.Instance.ScenesSave.Count > 0)
                        SceneManager.Instance.LoadScene(GameSettings.Instance.ScenesSave[0].Name);
                }
                else
                    Console.WriteLine("Problem with your folder json, here your path : " + pathJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Can't load scene" + ex.ToString());
            }
            #endregion

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (SceneManager.Instance.CurrentScene != null)
                SceneManager.Instance.CurrentScene.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (Camera.Main == null)
            {
                Console.WriteLine("There's no camera on this scene");
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



