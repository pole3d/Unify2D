using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unify2D.Assets;
using Unify2D.Builder;
using Unify2D.Core;
using Unify2D.Toolbox;
using Unify2D.Toolbox.Popup;
using Unify2D.Tools;

namespace Unify2D
{
    /// <summary>
    /// The main class of Unify2D
    /// It represents the whole editor window 
    /// This class inherits from Game which creates the Game window, handles the gameloop, the assets...
    /// This class handles the different windows of the game editor
    /// 
    /// 
    /// </summary>
    public class GameEditor : Game
    {
        #region singleton 

        public static GameEditor Instance => s_instance;

        private static GameEditor s_instance;

        #endregion


        const string AssetsFolder = "./Assets";

        public string ProjectPath => _settings.Data.CurrentProjectPath;
        public string AssetsPath => !string.IsNullOrEmpty(ProjectPath) ? ToolsEditor.CombinePath(ProjectPath, AssetsFolder) : string.Empty;

        public GameCore GameCore => _core;
        public Scripting.Scripting Scripting => _scripting;
        public ImGuiRenderer.Renderer Renderer => _imGuiRenderer;
        public GameObject Selected => _selected;
        public GameEditorSettings Settings => _settings;


        GameCore _core;
        GraphicsDeviceManager _graphics;
        ImGuiRenderer.Renderer _imGuiRenderer;

        Scripting.Scripting _scripting;
        Stack<PopupBase> _popups = new Stack<PopupBase>();

        List<Toolbox.Toolbox> _toolboxes = new List<Toolbox.Toolbox>();


        GameObject _selected;
        InspectorToolbox _inspectorToolbox;
        ScriptToolbox _scriptToolbox;
        GameToolbox _gameToolbox;

        GameEditorSettings _settings;


        public GameEditor()
        {
            s_instance = this;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = true;

            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _core = new GameCore(this);
            GameCore.SetCurrent(_core);

            _settings = new GameEditorSettings();
            _settings.Load(this);

            _scripting = new Scripting.Scripting();
            _scripting.Load(this);

            _imGuiRenderer = new ImGuiRenderer.Renderer(this);
            _imGuiRenderer.RebuildFontAtlas();
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;

            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 60;
            _graphics.ApplyChanges();

            InitializeToolBoxes();

            ShowPopup(new LauncherPopup());

            base.Initialize();
        }

        void InitializeToolBoxes()
        {
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
        }

        protected override void LoadContent()
        {
            _core.Initialize(GraphicsDevice);

            base.LoadContent();
        }

        public void SelectObject(object go)
        {
            if (go is Asset asset)
            {
                if (asset.AssetContent is ScriptAssetContent script)
                {
                    _scriptToolbox.SetObject(asset);
                    return;
                }
            }

            if (go is GameObject)
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

            foreach (var item in _toolboxes)
            {
                item.Update();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // Draw our UI
            DrawImGuiLayout(gameTime);

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
                        ShowPopup(new LauncherPopup());
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
                        LoadScene();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.MenuItem("Play"))
                {
                    Build();
                }

                ImGui.EndMainMenuBar();
            }
        }

        void Popups()
        {
            if (_popups.Count > 0)
            {
                _popups.Peek().Draw(this);
            }
        }

        public void ShowPopup(PopupBase popup)
        {
            _popups.Push(popup);
        }

        internal void HidePopup()
        {
            _popups.Pop();
        }

        protected virtual void DrawImGuiLayout(GameTime gameTime)
        {
            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            DrawMainMenuBarUI();

            foreach (var item in _toolboxes)
            {
                item.Draw();
            }

            ImGui.ShowDemoWindow();

        }

        private void Build()
        {
            GameBuilder builder = new GameBuilder();
            builder.Build(_core, this);
            builder.StartBuild();
        }




        void Save()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            string text = JsonConvert.SerializeObject(_core.GameObjects, settings);

            File.WriteAllText(ToolsEditor.CombinePath(ProjectPath, "./test.scene"), text);
        }

        public void LoadScene()
        {
            _core.GameObjects.Clear();

            SelectObject(null);

            List<GameObject> gameObjects = null;
            try
            {
                string text = File.ReadAllText(ToolsEditor.CombinePath(ProjectPath, "./test.scene"));
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                settings.Error += SilentErrors;
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

        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }

        protected override void UnloadContent()
        {
            _settings.Save();
        }




    }




}



