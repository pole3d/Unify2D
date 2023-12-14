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

        const string AssetsFolder = "./Assets";

        #region Properties
        public string ProjectPath => _settings.Data.CurrentProjectPath;
        public string AssetsPath => !string.IsNullOrEmpty(ProjectPath) ? ToolsEditor.CombinePath(ProjectPath, AssetsFolder) : string.Empty;
        
        public GameEditorSettings Settings => _settings;
        public Scripting.Scripting Scripting => _scripting;
        public ImGuiRenderer.Renderer GuiRenderer => _imGuiRenderer;
        internal InspectorToolbox InspectorToolbox => _inspectorToolbox;
        internal ScriptToolbox ScriptToolbox => _scriptToolbox;
        internal GameToolbox GameToolbox => _gameToolbox;
        public GameCoreInfo GameCoreInfoScene => _coreInfoScene; 
        public List<GameCoreInfo> GameCoresInfo => _coresInfo;

        public SceneEditorManager SceneEditorManager => _sceneEditorManager;
        #endregion

        #region Fields
        bool _projectLoaded;
        GraphicsDeviceManager _graphics;
        GameEditorUI _gameEditorUI;
        ImGuiRenderer.Renderer _imGuiRenderer;
        Scripting.Scripting _scripting;
        GameEditorSettings _settings;
        SceneEditorManager _sceneEditorManager;
        
        GameCoreInfo _coreInfoScene;
        List<GameCoreInfo> _coresInfo = new List<GameCoreInfo>();

        List<ToolboxBase> _toolboxes = new List<ToolboxBase>();
        
        InspectorToolbox _inspectorToolbox;
        ScriptToolbox _scriptToolbox;
        GameToolbox _gameToolbox;
        HierarchyToolbox _hierarchyToolbox;
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
            
            //Create game core and load scene content
            _coreInfoScene = new GameCoreInfo(
                new GameCore(this),
                "./test.scene");
            _coresInfo.Add(_coreInfoScene);
            GameCore.SetCurrent(_coreInfoScene.GameCore);

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
            _gameToolbox.SetCore(_coreInfoScene);
            _hierarchyToolbox = new HierarchyToolbox();
            _hierarchyToolbox.SetCore(_coreInfoScene);

            _toolboxes.Add(new AssetsToolbox());
            _toolboxes.Add(_hierarchyToolbox);

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
            _coreInfoScene.GameCore.Initialize(GraphicsDevice);

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
            builder.Build(_coreInfoScene.GameCore, this);
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
//             string text = JsonConvert.SerializeObject(_coreInfoScene.GameCore.GameObjects, settings);
//
//             File.WriteAllText(ToolsEditor.CombinePath(ProjectPath, "./test.scene"), text);
//         }
//
//         public void LoadScene()
//         {
//             _projectLoaded = true;
//
//             _coreInfoScene.GameCore.GameObjects.Clear();
//
//             SelectObject(null);
//
//             List<GameObject> gameObjects = null;
//             try
//             {
//                 string text = File.ReadAllText(_coreInfoScene.AssetPath);
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
//                 _coreInfoScene.GameCore.LoadScene(this, gameObjects);
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

        internal void SetSceneCore(GameCoreInfo sceneCoreInfo)
        {
            sceneCoreInfo.GameCore.Initialize(GraphicsDevice);
            
            _coreInfoScene = sceneCoreInfo;
            _gameToolbox.SetCore(sceneCoreInfo);
            _hierarchyToolbox.SetCore(sceneCoreInfo);
            
            GameCore.SetCurrent(sceneCoreInfo.GameCore);
        }

        internal void OpenPrefab(PrefabAssetContent content)
        {
            GameCoreInfo prefabCoreInfo = new GameCoreInfo(
                new GameCore(this),
                content.Asset.FullPath);
            _coresInfo.Add(prefabCoreInfo);
            prefabCoreInfo.GameCore.Initialize(GraphicsDevice);
            
            _gameToolbox.SetCore(prefabCoreInfo);
            _hierarchyToolbox.SetCore(prefabCoreInfo);

            GameCore.SetCurrent(prefabCoreInfo.GameCore);
            
            prefabCoreInfo.GameCore.AddGameObjectImmediate(content.InstantiateGameObject(this));
        }

        internal void CloseGameCore(GameCoreInfo gameCoreInfo)
        {
            _coresInfo.Remove(gameCoreInfo);
            GameCoreInfo replaceCore = _coresInfo.Count == 0 ? _coreInfoScene : _coresInfo[_coresInfo.Count - 1];
            if (_gameToolbox.Tag == gameCoreInfo)
                _gameToolbox.SetCore(replaceCore);
            if (_hierarchyToolbox.Tag == gameCoreInfo)
                _hierarchyToolbox.SetCore(replaceCore);
            GameCore.SetCurrent(replaceCore.GameCore);
        }
    }
}



