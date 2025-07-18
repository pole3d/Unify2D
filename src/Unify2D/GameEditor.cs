﻿using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Unify2D.Assets;
using Unify2D.Builder;
using Unify2D.Core;
using Unify2D.Toolbox;
using Unify2D.Toolbox.Popup;
using UnifyCore;

namespace Unify2D
{
    /// <summary>
    /// The main class of Unify2D
    /// It represents the whole editor window 
    /// This class inherits from Game which creates the Game window, handles the gameloop, the assets...
    /// This class handles the different windows of the game editor
    /// </summary>
    public class GameEditor : Game
    {
        #region singleton

        public static GameEditor Instance => s_instance;

        private static GameEditor s_instance;

        #endregion

        //public const string AssetsFolder = "\\Assets";
        public const string ScenesFolder = "\\Assets\\Scenes";

        #region Properties

        public string ProjectPath => _settings.Data.CurrentProjectPath;

        public override string AssetsPath => !string.IsNullOrEmpty(ProjectPath)
            ? CoreTools.CombinePath(ProjectPath, AssetsFolder)
            : string.Empty;

        public GameEditorSettings Settings => _settings;
        public Scripting.Scripting Scripting => _scripting;
        public ImGuiRenderer.Renderer GuiRenderer => _imGuiRenderer;

       // public GameObject Selected => _selected;

        public SceneManager SceneEditorManager => _sceneEditorManager;

        internal InspectorToolbox InspectorToolbox { get; private set; }
        internal ScriptToolbox ScriptToolbox { get; private set; }
        internal GameToolbox GameToolbox { get; private set; }
        internal HierarchyToolbox HierarchyToolbox { get; private set; }
        internal AssetsToolbox AssetsToolBox { get; private set; }
        internal SpriteEditorToolbox SpriteEditorToolbox { get; private set; }
        internal AssetManager AssetManager { get; private set; }

        public GameCoreViewer GameCoreViewerScene => _coreViewerScene;
        public List<GameCoreViewer> GameCoreViewers => _coreViewers;

        #endregion

        #region Fields

        GraphicsDeviceManager _graphics;
        GameEditorUI _gameEditorUI;
        ImGuiRenderer.Renderer _imGuiRenderer;
        Scripting.Scripting _scripting;
        GameEditorSettings _settings;
        SceneManager _sceneEditorManager;
        GameCoreViewer _coreViewerScene;
        List<GameCoreViewer> _coreViewers = new List<GameCoreViewer>();


        List<Toolbox.Toolbox> _toolboxes = new List<Toolbox.Toolbox>();

        //GameObject _selected;
       // bool _projectLoaded = false;

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
            _sceneEditorManager = SceneManager.Instance;

            Exiting += (object _, EventArgs _) => { _sceneEditorManager.SaveCurrentSceneToJson(); };
        }

        protected override void Initialize()
        {
            _settings = new GameEditorSettings();
            _settings.Load(this);

            AssetManager = new AssetManager(this);

            // Create game core and load scene content / Not useful for now
            _coreViewerScene = new GameCoreViewer(new GameCoreEditor(this), ".scene");
            _coreViewers.Add(_coreViewerScene);

            // Set the current game core
            GameCore.SetCurrent(_coreViewerScene.GameCore);

            Content.RootDirectory = ProjectPath;

            _scripting = new Scripting.Scripting();
            _scripting.Load(this);

            _imGuiRenderer = new ImGuiRenderer.Renderer(this);
            _imGuiRenderer.RebuildFontAtlas();
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;

            Window.AllowUserResizing = true;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 60;
            _graphics.ApplyChanges();


            ShowPopup(new LauncherPopup());

            base.Initialize();
        }

        void InitializeToolBoxes()
        {
            ScriptToolbox = new ScriptToolbox();
            InspectorToolbox = new InspectorToolbox();
            GameToolbox = new GameToolbox();
            GameToolbox.SetCore(_coreViewerScene);
            HierarchyToolbox = new HierarchyToolbox();
            HierarchyToolbox.SetCore(_coreViewerScene);
            AssetsToolBox = new AssetsToolbox();
            SpriteEditorToolbox = new SpriteEditorToolbox();

            _toolboxes.Add(AssetsToolBox);
            _toolboxes.Add(HierarchyToolbox);
            _toolboxes.Add(SpriteEditorToolbox);
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
            _coreViewerScene.GameCore.Initialize(GraphicsDevice);

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
            if (_unbindTargets.Count > 0)
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

            ImGui.DockSpaceOverViewport();

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


            if (builder.Build(_coreViewerScene.GameCore, this))
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


        // Waiting to resolve operation
        internal void OpenPrefab(PrefabAssetContent content)
        {
            GameCoreViewer prefabCoreViewer = new GameCoreViewer(new GameCore(this),
                content.Asset.FullPath);
            _coreViewers.Add(prefabCoreViewer);
            prefabCoreViewer.GameCore.Initialize(GraphicsDevice);

            GameToolbox.SetCore(prefabCoreViewer);
            HierarchyToolbox.SetCore(prefabCoreViewer);

            GameCore.SetCurrent(prefabCoreViewer.GameCore);

            if (content.IsLoaded == false)
                content.Load();

            // GameObject.Instantiate(content.Asset.FullPath);
        }

        // Waiting to resolve operation
        internal void CloseGameCore(GameCoreViewer gameCoreViewer)
        {
            _coreViewers.Remove(gameCoreViewer);
            GameCoreViewer replaceCore =
                _coreViewers.Count == 0 ? _coreViewerScene : _coreViewers[^1];
            if (GameToolbox.Tag == gameCoreViewer)
                GameToolbox.SetCore(replaceCore);
            if (HierarchyToolbox.Tag == gameCoreViewer)
                HierarchyToolbox.SetCore(replaceCore);
            GameCore.SetCurrent(replaceCore.GameCore);
        }

        internal void PreloadProject()
        {
            GameCore.Current.AssetsManager.Initialize(AssetsPath);
        }

        internal void ProjectLoaded()
        {
            InitializeToolBoxes();
            HidePopup();

        }


    }
}