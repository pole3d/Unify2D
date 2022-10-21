using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Assets;
using Unify2D.Builder;
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
        InspectorToolbox _inspector;

        SelectedState _selectState;

        enum SelectedState
        {
            None,
            Select,
            Drag
        }

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

            _inspector = new InspectorToolbox();
            _toolboxes.Add(new AssetsToolbox());

            _toolboxes.Add(_inspector);

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

            _sceneRenderTarget = new RenderTarget2D(GraphicsDevice, (int)_gameResolution.X, (int)_gameResolution.Y);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _core.Initialize(GraphicsDevice);

            base.LoadContent();
        }

        IntPtr _renderTargetId = IntPtr.Zero;
        RenderTarget2D _sceneRenderTarget;


        void SelectGameObject(GameObject go)
        {
            _selected = go;
            _inspector.SetGameObject(go);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var mouseState = Mouse.GetState();

            if (_selectState == SelectedState.None)
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    Vector2 worldPosition = GetWorldMousePosition();

                    foreach (var item in _core.GameObjects)
                    {
                        if (worldPosition.X >= item.Position.X - item.BoundingSize.X / 2 && worldPosition.X <= item.Position.X + item.BoundingSize.X / 2
                            && worldPosition.Y >= item.Position.Y - item.BoundingSize.Y / 2 && worldPosition.Y <= item.Position.Y + item.BoundingSize.Y / 2)
                        {
                            SelectGameObject(item);

                            Num.Vector2 mousePosition = new Num.Vector2(mouseState.X, mouseState.Y);
                            Num.Vector2 goPosition = WorldToUI(item.Position);

                            Num.Vector2 direction = mousePosition - goPosition;
                            if (direction.Length() < 10)
                            {
                                _selectState = SelectedState.Drag;
                            }


                        }

                    }
                }
            }
            else if (_selectState == SelectedState.Drag)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && _selected != null)
                {
                    _selected.Position = GetWorldMousePosition();
                }
                if ( mouseState.LeftButton == ButtonState.Released)
                {
                    _selectState = SelectedState.None;
                }
            }
        }

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

        public Vector2 GetWorldMousePosition()
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
                        SelectGameObject(go);
                        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                        renderer.Initialize(this, go, asset.FullPath);
                    }
                }
            }
            ImGui.EndDragDropTarget();
            var mouseState = GetWorldMousePosition();
            var mouse = Mouse.GetState();
            Num.Vector2 mousePosition = new Num.Vector2(mouse.X, mouse.Y);
            ImGui.Text($" {mouseState.X}:{mouseState.Y} /  {mousePosition.X}:{mousePosition.Y}");
            ImGui.PopStyleVar();

            ImGui.End();



            ImGui.Begin("Hierarchy");

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
                    SelectGameObject(item);
                }
            }


            ImGui.TreePop();
            ImGui.End();



            if ( ImGui.Button("Build"))
                Build();
            if (ImGui.Button("Save"))
            {
                Save();
            }
            if (ImGui.Button("Load"))
            {
                Load();
            }

        }

        private void Build()
        {
            GameBuilder builder = new GameBuilder();
            builder.Build(_core);
            builder.StartBuild();
        }

        private  void Circle()
        {
            if (_selected == null)
                return;

            var p0 = ImGui.GetItemRectMin();
            var p1 = ImGui.GetItemRectMax();

            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(p0, p1);

            uint color = MakeColor32(50, 255, 50, 255);

            if ( _selectState == SelectedState.Drag)
            {
                color = MakeColor32(255, 255, 50, 255);
            }
 
            drawList.AddCircle( WorldToUI(_selected.Position),
                      8, color, 64, 3);
            drawList.PopClipRect();
        }

        Num.Vector2 WorldToUI(Vector2 world)
        {
            Num.Vector2 result = new Num.Vector2(world.X, world.Y);

            float x = world.X / _gameResolution.X;
            float y = world.Y / _gameResolution.Y;

            x *= _gameWindowSize.X;
            y *= _gameWindowSize.Y;

            result = _gameWindowPosition + _gameWindowOffset + new Num.Vector2(x,y);
            return result;
        }

        void Save()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            string text = JsonConvert.SerializeObject(_core.GameObjects ,settings);

            File.WriteAllText("./test.scene", text);
        }

        void Load()
        {
            _core.GameObjects.Clear();

            SelectGameObject(null);

            string text = File.ReadAllText("./test.scene");
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;

            _core.LoadScene(this,  JsonConvert.DeserializeObject<List<GameObject>>(text, settings));
        }

        public static uint MakeColor32(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }
    }




}



