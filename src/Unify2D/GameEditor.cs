using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        #region singleton 

        public static GameEditor Instance => s_instance;
              
        private static GameEditor s_instance;

        #endregion


        const string AssetsFolder = "./Assets";

        public string ProjectPath => _settings.Data.CurrentProjectPath;
        public string AssetsPath => !String.IsNullOrEmpty(ProjectPath) ? Path.Combine(ProjectPath, AssetsFolder) : String.Empty;

        public Scripting.Scripting Scripting => _scripting;
        public ImGuiRenderer.Renderer Renderer => _imGuiRenderer;

        private GraphicsDeviceManager _graphics;
        private ImGuiRenderer.Renderer _imGuiRenderer;

        Scripting.Scripting _scripting;


        List<Toolbox.Toolbox> _toolboxes = new List<Toolbox.Toolbox>();


        private GameCore _core;
        public GameCore GameCore => _core;

        GameObject _selected;
        InspectorToolbox _inspectorToolbox;
        ScriptToolbox _scriptToolbox;
        GameToolbox _gameToolbox;


        SelectedState _selectState;
        bool _showSelectPath;
        GameEditorSettings _settings;

        enum SelectedState
        {
            None,
            Select,
            Drag
        }

        public GameEditor()
        {
            s_instance = this;

            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferMultiSampling = true;

            IsMouseVisible = true;

        }

        protected override void Initialize()
        {
            _core = new GameCore();
            GameCore.SetCurrent(_core);

            _settings = new GameEditorSettings();
            _settings.Load(this);

            _scripting = new Unify2D.Scripting.Scripting();
            _scripting.Load(this);


            Load();

            _scriptToolbox = new ScriptToolbox();
            _inspectorToolbox = new InspectorToolbox();
            _gameToolbox = new GameToolbox();

            _toolboxes.Add(new AssetsToolbox());
            _toolboxes.Add(new HierarchyToolbox());

            _toolboxes.Add(_scriptToolbox);
            _toolboxes.Add(_inspectorToolbox);
            _toolboxes.Add(_gameToolbox);

            foreach (var item in _toolboxes)
            {
                item.Initialize(this);
            }

            _imGuiRenderer = new ImGuiRenderer.Renderer(this);
            _imGuiRenderer.RebuildFontAtlas();
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 60;
            _graphics.ApplyChanges();


            base.Initialize();
        }



        protected override void LoadContent()
        {
            _core.Initialize(GraphicsDevice);

            base.LoadContent();
        }


        public void SelectObject(object go)
        {
            if ( go is Asset asset)
            {
                if ( asset.AssetContent is ScriptAssetContent script)
                {
                    _scriptToolbox.SetObject(asset);
                    return;
                }
            }

            if ( go is GameObject)
                _selected = go as GameObject;

            if (_inspectorToolbox != null)
                _inspectorToolbox.SetObject(go);
        }

        public void UnSelectObject()
        {
            _selected = null;

            if (_inspectorToolbox != null)
                _inspectorToolbox.SetObject(null);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var mouseState = Mouse.GetState();

            if (_selectState == SelectedState.None && IsMouseInGameWindow())
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    Vector2 worldPosition = GetWorldMousePosition();

                    foreach (var item in _core.GameObjects)
                    {
                        if (worldPosition.X >= item.Position.X - item.BoundingSize.X / 2 && worldPosition.X <= item.Position.X + item.BoundingSize.X / 2
                            && worldPosition.Y >= item.Position.Y - item.BoundingSize.Y / 2 && worldPosition.Y <= item.Position.Y + item.BoundingSize.Y / 2)
                        {
                            SelectObject(item);

                            Num.Vector2 mousePosition = new Num.Vector2(mouseState.X, mouseState.Y);
                            Num.Vector2 goPosition = _gameToolbox.WorldToUI(item.Position);

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
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    _selectState = SelectedState.None;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // Draw our UI
            ImGuiLayout(gameTime);

            Popups();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();
        }

        private void DrawMainMenuBarUI()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Load project"))
                    {
                        _showSelectPath = true;
                    }
                    if (ImGui.MenuItem("Show Explorer"))
                    {
                        Process.Start("explorer.exe", _settings.Data.CurrentProjectPath);
                    }
                    if (ImGui.MenuItem("Build"))
                        Build();
                    if (ImGui.MenuItem("Save"))
                    {
                        Save();
                    }
                    if (ImGui.MenuItem("Load"))
                    {
                        Load();
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }

        void Popups()
        {
            if (_showSelectPath)
            {
                ImGui.OpenPopup("open-project");
                _showSelectPath = false;
            }

            if (ImGui.BeginPopupModal("open-project"))
            {
                var picker = FilePicker.GetFolderPicker(this, ProjectPath);
                picker.RootFolder = "C:\\";
                picker.OnlyAllowFolders = true;
                if (picker.Draw())
                {
                    _settings.Data.CurrentProjectPath = picker.SelectedFile;
                    Load();
                    foreach (var item in _toolboxes)
                    {
                        item.Reset();
                    }

                    FilePicker.RemoveFilePicker(this);
                }
                ImGui.EndPopup();
            }
        }


        public bool IsMouseInGameWindow()
        {
            return _gameToolbox.IsMouseInWindow();
        }

        public Vector2 GetWorldMousePosition()
        {
            return _gameToolbox.GetMousePosition();
        }

        protected virtual void ImGuiLayout(GameTime gameTime)
        {
            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            DrawMainMenuBarUI();

            foreach (var item in _toolboxes)
            {
                item.Show();
            }

            ImGui.ShowDemoWindow();
        }

        private void Build()
        {
            GameBuilder builder = new GameBuilder();
            builder.Build(_core , this);
            builder.StartBuild();
        }

        public void Circle()
        {
            if (_selected == null)
                return;

            var p0 = ImGui.GetItemRectMin();
            var p1 = ImGui.GetItemRectMax();

            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(p0, p1);

            uint color = MakeColor32(50, 255, 50, 255);

            if (_selectState == SelectedState.Drag)
            {
                color = MakeColor32(255, 255, 50, 255);
            }

            drawList.AddCircle(_gameToolbox.WorldToUI(_selected.Position),
                      8, color, 64, 3);
            drawList.PopClipRect();
        }


        void Save()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            string text = JsonConvert.SerializeObject(_core.GameObjects, settings);

            File.WriteAllText(Path.Combine(ProjectPath, "./test.scene"), text);
        }

        void Load()
        {
            _core.GameObjects.Clear();

            SelectObject(null);

            List<GameObject> gameObjects = null;
            try
            {
                string text = File.ReadAllText(Path.Combine(ProjectPath, "./test.scene"));
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                gameObjects = JsonConvert.DeserializeObject<List<GameObject>>(text, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (gameObjects != null)
            {
                Content.RootDirectory = ProjectPath;
                _core.LoadScene(this, gameObjects);
            }
        }

        protected override void UnloadContent()
        {
            _settings.Save();
        }

        public static uint MakeColor32(byte r, byte g, byte b, byte a) { uint ret = a; ret <<= 8; ret += b; ret <<= 8; ret += g; ret <<= 8; ret += r; return ret; }
    }




}



