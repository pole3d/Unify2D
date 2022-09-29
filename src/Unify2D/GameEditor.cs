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

        GameObject _selected;

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
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

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

        bool[] _hierarchy = new bool[100];

        public static uint ToColor32(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }
        int _selectedHierarchy;
        protected virtual void ImGuiLayout()
        {
            foreach (var item in _toolboxes)
            {
                item.Show();
            }

            ImGui.ShowDemoWindow();

            _renderTargetId = _imGuiRenderer.BindTexture(_sceneRenderTarget);




            ImGui.Begin("GAME", ImGuiWindowFlags.None);
            _gameWindowPosition = ImGui.GetWindowPos();
            _gameWindowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
            _gameWindowSize.Y -= _bottomOffset;

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Num.Vector2.Zero);
            ImGui.Image(_renderTargetId, ImGui.GetContentRegionAvail() - new Num.Vector2(0, _bottomOffset));
            Circle();

            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var ptr = ImGui.AcceptDragDropPayload("ASSET");
                    if (ptr.NativePtr != null)
                    {
                        Asset asset = Clipboard.Content as Asset;
                        GameObject go = new GameObject();
                        _selected = go;
                        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                        renderer.Initialize(this, go, asset.FullPath);
                    }
                }
            }
            ImGui.EndDragDropTarget();
            var mouseState = GetMousePosition();
            ImGui.Text($" {mouseState.X}:{mouseState.Y}");
            ImGui.PopStyleVar();

            ImGui.End();

            ImGui.Begin("Inspector");
            if (_selected != null)
            {
                string name = _selected.Name;
                ImGui.InputText("name", ref name, 40);
                _selected.Name = name;
                Num.Vector2 position = new Num.Vector2(_selected.Position.X, _selected.Position.Y);
                ImGui.InputFloat2("position", ref position);
                _selected.Position = new Vector2(position.X, position.Y);
            }
            ImGui.End();

            ImGui.Begin("Hierarchy");
            //if (ImGui.TreeNode("Trees"))
            //{
            //    if (ImGui.TreeNode("Child"))
            //    {
            //    }
            //    ImGui.TreePop();

            //}

            int i = 0;
            foreach (var item in _core.GameObjects)
            {
                if (ImGui.Selectable($"{i++} : {item.Name}", _hierarchy[i]))
                {
                    for (int j = 0; j < _hierarchy.Length; j++)
                    {
                        _hierarchy[j] = false;
                    }

                    _hierarchy[i] = true;
                    _selected = item;
                }
            }


            ImGui.TreePop();
            ImGui.End();


        }

        private static void Circle()
        {
            var p0 = ImGui.GetItemRectMin();
            var p1 = ImGui.GetItemRectMax();

            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(p0, p1);

            var io = ImGui.GetIO();


            drawList.AddCircle(new Num.Vector2(p0.X + 100, p0.Y + 100),
                      50, MakeColor32(50, 255, 50, 255), 64, 5);
            drawList.PopClipRect();
        }

        public static uint MakeColor32(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }
    }


}



