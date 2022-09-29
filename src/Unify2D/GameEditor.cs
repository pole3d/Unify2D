using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.ImGuiRenderer;
using Unify2D.Toolbox;
using Unify2D.Tools;
using Num = System.Numerics;

namespace Unify2D
{
    /// <summary>
    /// The main editor of Unify2D
    /// </summary>
    public class GameEditor : Game
    {
        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer.Renderer _imGuiRenderer;


        List<Toolbox.Toolbox> _toolboxes = new List<Toolbox.Toolbox>();

        Num.Vector2 _gameWindowPosition;
        Num.Vector2 _gameWindowSize;
        readonly Num.Vector2 _gameWindowOffset = new Num.Vector2(8, 27);
        const float _bottomOffset = 20;

        Vector2 _gameResolution = new Vector2(1920, 1080);

        GameCore _core;

        public GameEditor()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferMultiSampling = true;

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _core = new GameCore();
            GameCore.SetCurrent(_core);

            _toolboxes.Add(new AssetsToolbox());

            foreach (var item in _toolboxes)
            {
                item.Initialize();
            }

            _imGuiRenderer = new ImGuiRenderer.Renderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 60;
            _graphics.ApplyChanges();

            _sceneRenderTarget = new RenderTarget2D(GraphicsDevice, 1920, 1080);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _core.Initialize(GraphicsDevice);

            base.LoadContent();
        }

        IntPtr _renderTargetId = IntPtr.Zero;
        RenderTarget2D _sceneRenderTarget;

 

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_sceneRenderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _core.Draw();

            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            // Draw our UI
            ImGuiLayout();

            //base.Draw(gameTime);
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();
        }

        public Vector2 GetMousePosition()
        {
            var mouseState = Mouse.GetState();
            Num.Vector2 mousePosition = new Num.Vector2(mouseState.X, mouseState.Y);
            mousePosition -= (_gameWindowPosition + _gameWindowOffset);

            Vector2 size = new Vector2(_gameWindowSize.X, _gameWindowSize.Y);
            Vector2 result = new Vector2(mousePosition.X, mousePosition.Y);
            result /= size;

            result.X = MathHelper.Clamp(result.X, 0, 1);
            result.Y = MathHelper.Clamp(result.Y, 0, 1);

            result *= _gameResolution;

            result.X = MathF.Round(result.X);
            result.Y = MathF.Round(result.Y);

            return result;
        }

        protected virtual void ImGuiLayout()
        {
            foreach (var item in _toolboxes)
            {
                item.Show();
            }

            //ImGui.ShowDemoWindow();

            _renderTargetId = _imGuiRenderer.BindTexture(_sceneRenderTarget);




            ImGui.Begin("GAME", ImGuiWindowFlags.None);
            _gameWindowPosition = ImGui.GetWindowPos();
            _gameWindowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
            _gameWindowSize.Y -= _bottomOffset;
            //if (ImGui.BeginMenuBar())
            //{
            //    if (ImGui.BeginMenu("File"))
            //    {
            //        if (ImGui.MenuItem("Cut", "CTRL+X")) { }
            //        ImGui.EndMenu();
            //    }
            //    ImGui.EndMenuBar();
            //}

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Num.Vector2.Zero);
            ImGui.Image(_renderTargetId, ImGui.GetContentRegionAvail() - new Num.Vector2(0, _bottomOffset));

         

            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var ptr = ImGui.AcceptDragDropPayload("ASSET");
                    if (ptr.NativePtr != null)
                    {
                        Asset asset = Clipboard.Content as Asset;
                        GameObject go = new GameObject();
                        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                        renderer.Initialize(this, asset.FullPath);
                    }
                }
            }
            ImGui.EndDragDropTarget();
            var mouseState = GetMousePosition();
            ImGui.Text($" {mouseState.X}:{mouseState.Y} + {_gameWindowPosition}");
            ImGui.PopStyleVar();

            ImGui.End();
        }


    }
}



