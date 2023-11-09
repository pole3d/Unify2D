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

        #region Properties

        public string ProjectPath => _settings.Data.CurrentProjectPath;
        public string AssetsPath => !string.IsNullOrEmpty(ProjectPath) ? ToolsEditor.CombinePath(ProjectPath, AssetsFolder) : string.Empty;

        public GameCore GameCore => _core;
        public GameEditorSettings Settings => _settings;
        public Scripting.Scripting Scripting => _scripting;
        public ImGuiRenderer.Renderer GuiRenderer => _imGuiRenderer;

        public GameObject Selected => _selected;

        public SceneEditorManager SceneEditorManager => _sceneEditorManager;


        #endregion

        #region Fields
        GameCore _core;
        GraphicsDeviceManager _graphics;
        GameEditorUI _gameEditorUI;
        ImGuiRenderer.Renderer _imGuiRenderer;
        Scripting.Scripting _scripting;
        GameEditorSettings _settings;
        SceneEditorManager _sceneEditorManager;

        Stack<PopupBase> _popups = new Stack<PopupBase>();
        List<Toolbox.Toolbox> _toolboxes = new List<Toolbox.Toolbox>();
        internal InspectorToolbox InspectorToolbox { get; private set; }
        internal ScriptToolbox ScriptToolbox { get; private set; }
        internal GameToolbox GameToolbox { get; private set; }

        GameObject _selected;
        bool _projectLoaded = false;
        #endregion

        #region Initialization

        bool _showSelectPath;

        public GameEditor()
        {
            s_instance = this;

            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = true;

            IsMouseVisible = true;

            _gameEditorUI = new GameEditorUI(this);
            _sceneEditorManager = new SceneEditorManager(this);
        }

        protected override void Initialize()
        {
            _core = new GameCore(this);
            GameCore.SetCurrent(_core);

            _settings = new GameEditorSettings();
            _settings.Load(this);

            Content.RootDirectory = ProjectPath;

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
            ScriptToolbox = new ScriptToolbox();
            InspectorToolbox = new InspectorToolbox();
            GameToolbox = new GameToolbox();

            _toolboxes.Add(new AssetsToolbox());
            _toolboxes.Add(new HierarchyToolbox());

            _toolboxes.Add(ScriptToolbox);
            _toolboxes.Add(InspectorToolbox);
            _toolboxes.Add(GameToolbox);

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

        #endregion

        #region Update / Draw 

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (GameToolbox == null) return;

            Selection.Update(gameTime);

            foreach (var item in _toolboxes)
            {
                item.Update();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // Draw our UI
            DrawImGuiLayout(gameTime);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();
        }


        protected virtual void DrawImGuiLayout(GameTime gameTime)
        {
            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            _gameEditorUI.DrawMainMenuBarUI();

            foreach (var item in _toolboxes)
            {
                item.Draw();
            }

            _gameEditorUI.DrawPopup();
        }

        #endregion

        #region Selection

        public void SelectObject(object go)
        {
            if (go is Asset asset)
            {
                if (asset.AssetContent is ScriptAssetContent script)
                {
                    ScriptToolbox.SetObject(asset);
                    return;
                }
            }

            if (go is GameObject)
                _selected = go as GameObject;

            if (InspectorToolbox != null)
                InspectorToolbox.SetObject(go);
        }

        public void UnSelectObject()
        {
            _selected = null;

            if (InspectorToolbox != null)
                InspectorToolbox.SetObject(null);
        }

        #endregion



        #region Tools

        public void ShowPopup(PopupBase popup)
        {
            _gameEditorUI.ShowPopup(popup);
        }

        public void HidePopup()
        {
            _gameEditorUI.HidePopup();
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
                        LoadScene();
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }

        //void Popups()
        //{
        //    if (_showSelectPath)
        //    {
        //        ImGui.OpenPopup("open-project");
        //        _showSelectPath = false;
        //    }

        //    if (ImGui.BeginPopupModal("open-project"))
        //    {
        //        var picker = FilePicker.GetFolderPicker(this, ProjectPath);
        //        picker.RootFolder = "C:\\";
        //        picker.OnlyAllowFolders = true;
        //        if (picker.Draw())
        //        {
        //            _settings.Data.CurrentProjectPath = picker.SelectedFile;
        //            LoadScene();
        //            foreach (var item in _toolboxes)
        //            {
        //                item.Reset();
        //            }

        //            FilePicker.RemoveFilePicker(this);
        //        }
        //        ImGui.EndPopup();
        //    }

        //    if (_popups.Count > 0)
        //    {
        //        _popups.Peek().Draw(this);
        //    }
        //}


        public bool IsMouseInGameWindow()
        {
            return GameToolbox.IsMouseInWindow();
        }

        public Vector2 GetWorldMousePosition()
        {
            return GameToolbox.GetMousePosition();
        }

        public void Build()
        {
            GameBuilder builder = new GameBuilder();
            builder.Build(_core, this);
            builder.StartBuild();
        }

        #endregion
        void Save()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            string text = JsonConvert.SerializeObject(_core.GameObjects, settings);

            File.WriteAllText(Path.Combine(ProjectPath, "./test.scene"), text);
        }

        public void LoadScene()
        {
            _projectLoaded = true;
            InitializeToolBoxes();

            _core.GameObjects.Clear();

            Selection.SelectObject(null);

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
    }




}



