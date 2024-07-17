using ImGuiNET;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
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

        const string AssetsFolder = "\\Assets";

        #region Properties

        public string ProjectPath => _settings.Data.CurrentProjectPath;
        public string AssetsPath => !string.IsNullOrEmpty(ProjectPath) ? ToolsEditor.CombinePath(ProjectPath, AssetsFolder) : string.Empty;

        public GameCore GameCore => _core;
        public GameEditorSettings Settings => _settings;
        public Scripting.Scripting Scripting => _scripting;
        public ImGuiRenderer.Renderer GuiRenderer => _imGuiRenderer;

        public GameObject Selected => _selected;

        public SceneEditorManager SceneEditorManager => _sceneEditorManager;

        internal InspectorToolbox InspectorToolbox { get; private set; }
        internal ScriptToolbox ScriptToolbox { get; private set; }
        internal GameToolbox GameToolbox { get; private set; }
        internal HierarchyToolbox HierarchyToolbox { get; private set; }
        internal AssetsToolbox AssetsToolBox{ get; private set; }

        #endregion

        #region Fields
        GameCore _core;
        GraphicsDeviceManager _graphics;
        GameEditorUI _gameEditorUI;
        ImGuiRenderer.Renderer _imGuiRenderer;
        Scripting.Scripting _scripting;
        GameEditorSettings _settings;
        SceneEditorManager _sceneEditorManager;

        List<Toolbox.Toolbox> _toolboxes = new List<Toolbox.Toolbox>();

        GameObject _selected;
        bool _projectLoaded = false;

        List<(RenderTarget2D, IntPtr)> _unbindTargets = new List<(RenderTarget2D, IntPtr)>();
        #endregion

        #region Initialization

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
            HierarchyToolbox = new HierarchyToolbox();
            AssetsToolBox = new AssetsToolbox();

            _toolboxes.Add(AssetsToolBox);
            _toolboxes.Add(HierarchyToolbox);
            _toolboxes.Add(new ConsoleToolbox());

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

            foreach (var item in _toolboxes)
            {
                item.Update(gameTime);
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


            // Unbind old rendertarget, need to be done outside the drawcalls or it crashes
            if(_unbindTargets.Count > 0)
            {
                foreach (var tuple in _unbindTargets)
                {
                    tuple.Item1.Dispose(); // Rendertarget
                    GuiRenderer.UnbindTexture(tuple.Item2); // Id Pointer
                }
                _unbindTargets.Clear();
            }
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

        #region Tools

        public void ShowPopup(PopupBase popup)
        {
            _gameEditorUI.ShowPopup(popup);
        }

        public void HidePopup()
        {
            _gameEditorUI.HidePopup();
        }


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

        protected override void UnloadContent()
        {
            _settings.Save();
        }

        internal void UnbindTexture(RenderTarget2D sceneRenderTarget, IntPtr renderTargetId)
        {
            _unbindTargets.Add((sceneRenderTarget, renderTargetId));
        }
    }




}



