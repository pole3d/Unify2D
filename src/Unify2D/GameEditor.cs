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
using Microsoft.Xna.Framework.Input;
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


        public const string AssetsFolder = "\\Assets";
        
        #region Properties
        public string ProjectPath => _settings.Data.CurrentProjectPath;
        public override string AssetsPath => !string.IsNullOrEmpty(ProjectPath) ? ToolsEditor.CombinePath(ProjectPath, AssetsFolder) : string.Empty;
        
        public GameEditorSettings Settings => _settings;
        public Scripting.Scripting Scripting => _scripting;
        public AssetManager AssetManager => _assetManager;
        public ImGuiRenderer.Renderer GuiRenderer => _imGuiRenderer;
        internal InspectorToolbox InspectorToolbox => _inspectorToolbox;
        internal ScriptToolbox ScriptToolbox => _scriptToolbox;
        internal GameToolbox GameToolbox => _gameToolbox;
        public GameCoreViewer GameCoreViewerScene => _coreViewerScene; 
        public List<GameCoreViewer> GameCoreViewers => _coreViewers;

        public SceneEditorManager SceneEditorManager => _sceneEditorManager;
        internal HierarchyToolbox HierarchyToolbox => _hierarchyToolbox;
        internal AssetsToolbox AssetsToolBox{ get; private set; }


        #endregion

        #region Fields
        bool _projectLoaded;
        GraphicsDeviceManager _graphics;
        GameEditorUI _gameEditorUI;
        ImGuiRenderer.Renderer _imGuiRenderer;
        Scripting.Scripting _scripting;
        AssetManager _assetManager;
        GameEditorSettings _settings;
        SceneEditorManager _sceneEditorManager;
        
        GameCoreViewer _coreViewerScene;
        List<GameCoreViewer> _coreViewers = new List<GameCoreViewer>();

        List<ToolboxBase> _toolboxes = new List<ToolboxBase>();
        
        InspectorToolbox _inspectorToolbox;
        ScriptToolbox _scriptToolbox;
        GameToolbox _gameToolbox;
        HierarchyToolbox _hierarchyToolbox;
     
        GameObject _selected;
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
            _settings = new GameEditorSettings();
            _settings.Load(this);

            Content.RootDirectory = ProjectPath;

            _scripting = new Scripting.Scripting();
            _scripting.Load(this);

            _assetManager = new AssetManager(this);
            
            //Create game core and load scene content
            _coreViewerScene = new GameCoreViewer(
                new GameCoreEditor(this),
                "./test.scene");
            _coreViewers.Add(_coreViewerScene);
            GameCore.SetCurrent(_coreViewerScene.GameCore);

            _imGuiRenderer = new ImGuiRenderer.Renderer(this);
            _imGuiRenderer.RebuildFontAtlas();
            ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;

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
            AssetsToolBox = new AssetsToolbox();

            _scriptToolbox = new ScriptToolbox();
            _inspectorToolbox = new InspectorToolbox();
            _gameToolbox = new GameToolbox();
            _gameToolbox.SetCore(_coreViewerScene);
            _hierarchyToolbox = new HierarchyToolbox();
            _hierarchyToolbox.SetCore(_coreViewerScene);

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
            
            GameCore.Current.RefreshGameObjectListImmediate();
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
            builder.Build(_coreViewerScene.GameCore, this);
            builder.StartBuild();
        }

// <<<<<<< HEAD
//         public void CircleSelected()
//         {
//             if (_selected == null)
//                 return;
//
//             var p0 = ImGui.GetItemRectMin();
//             var p1 = ImGui.GetItemRectMax();
//
//             var drawList = ImGui.GetWindowDrawList();
//             drawList.PushClipRect(p0, p1);
//
//             uint color = ToolsUI.ToColor32(50, 255, 50, 255);
//
//             if (_selectState == SelectedState.Drag)
//             {
//                 color = ToolsUI.ToColor32(255, 255, 50, 255);
//             }
//
//             drawList.AddCircle(_gameToolbox.WorldToUI(_selected.Position),
//                       8, color, 64, 3);
//             drawList.PopClipRect();
//         }
//
//
//         void Save()
//         {
//             JsonSerializerSettings settings = new JsonSerializerSettings();
//             settings.TypeNameHandling = TypeNameHandling.Auto;
//             settings.Formatting = Formatting.Indented;
//             string text = JsonConvert.SerializeObject(_coreViewerScene.GameCore.GameObjects, settings);
//
//             File.WriteAllText(ToolsEditor.CombinePath(ProjectPath, "./test.scene"), text);
//         }
//
//         public void LoadScene()
//         {
//             _projectLoaded = true;
//
//             _coreViewerScene.GameCore.GameObjects.Clear();
//
//             SelectObject(null);
//
//             List<GameObject> gameObjects = null;
//             try
//             {
//                 string text = File.ReadAllText(_coreViewerScene.AssetPath);
//                 JsonSerializerSettings settings = new JsonSerializerSettings();
//                 settings.TypeNameHandling = TypeNameHandling.Auto;
//                 settings.Error += SilentErrors;
//                 gameObjects = JsonConvert.DeserializeObject<List<GameObject>>(text, settings);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine(ex.Message);
//             }
//
//             if (gameObjects != null)
//             {
//                 Content.RootDirectory = ProjectPath;
//                 _coreViewerScene.GameCore.LoadScene(this, gameObjects);
//             }
//         }
//
//         private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
//         {
//             e.ErrorContext.Handled = true;   
//         }
// =======
        #endregion
        
        protected override void UnloadContent()
        {
            _settings.Save();
        }


        internal void SetSceneCore(GameCoreViewer sceneCoreViewer)
        {
            sceneCoreViewer.GameCore.Initialize(GraphicsDevice);
            if (_coreViewers.Contains(sceneCoreViewer) == false)
                _coreViewers.Add(sceneCoreViewer);
            
            _coreViewerScene = sceneCoreViewer;
            _gameToolbox.SetCore(sceneCoreViewer);
            _hierarchyToolbox.SetCore(sceneCoreViewer);
            
            GameCore.SetCurrent(sceneCoreViewer.GameCore);
        }

        internal void OpenPrefab(PrefabAssetContent content)
        {
            GameCoreViewer prefabCoreViewer = new GameCoreViewer(
                new GameCoreEditor(this),
                content.Asset.FullPath);
            _coreViewers.Add(prefabCoreViewer);
            prefabCoreViewer.GameCore.Initialize(GraphicsDevice);
            
            _gameToolbox.SetCore(prefabCoreViewer);
            _hierarchyToolbox.SetCore(prefabCoreViewer);

            GameCore.SetCurrent(prefabCoreViewer.GameCore);
            GameObject.Instantiate(content.Asset.FullPath);
        }

        internal void CloseGameCore(GameCoreViewer gameCoreViewer)
        {
            _coreViewers.Remove(gameCoreViewer);
            GameCoreViewer replaceCore =
                _coreViewers.Count == 0 ? _coreViewerScene : _coreViewers[_coreViewers.Count - 1];
            if (_gameToolbox.Tag == gameCoreViewer)
                _gameToolbox.SetCore(replaceCore);
            if (_hierarchyToolbox.Tag == gameCoreViewer)
                _hierarchyToolbox.SetCore(replaceCore);
            GameCore.SetCurrent(replaceCore.GameCore);
        }

        internal void UnbindTexture(RenderTarget2D sceneRenderTarget, IntPtr renderTargetId)
        {
            _unbindTargets.Add((sceneRenderTarget, renderTargetId));
        }
    }
}



