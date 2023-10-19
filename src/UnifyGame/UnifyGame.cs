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
            _core.InitPhysics();

            _core.GameObjects.Clear();

            string text = File.ReadAllText("./test.scene");
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;

            _core.LoadScene(this, JsonConvert.DeserializeObject<List<GameObject>>(text, settings));

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            _core.Update(gameTime);        
            
            //Oumuamua
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _core.BeginDraw();
            _core.Draw();
            _core.EndDraw();

            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout();


            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();
        }

        //public Vector2 GetMousePosition()
        //{
        //    var mouseState = Mouse.GetState();
        //    Num.Vector2 mousePosition = new Num.Vector2(mouseState.X, mouseState.Y);
        //    mousePosition -= (_gameWindowPosition + _gameWindowOffset);

        //    Vector2 size = new Vector2(_gameWindowSize.X, _gameWindowSize.Y);
        //    Vector2 result = new Vector2(mousePosition.X, mousePosition.Y);
        //    result /= size;

        //    result.X = MathHelper.Clamp(result.X, 0, 1);
        //    result.Y = MathHelper.Clamp(result.Y, 0, 1);

        //    result *= _gameResolution;

        //    result.X = MathF.Round(result.X);
        //    result.Y = MathF.Round(result.Y);

        //    return result;
        //}



        protected virtual void ImGuiLayout()
        {
          //  ImGui.ShowDemoWindow();
        }

     
    }


}



