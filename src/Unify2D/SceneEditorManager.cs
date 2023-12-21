using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unify2D.Core;
using Unify2D.Tools;
using System.IO;
using Unify2D.Assets;

namespace Unify2D
{
    public class SceneEditorManager
    {
        GameEditor _gameEditor;

        public SceneEditorManager(GameEditor editor)
        {
            _gameEditor = editor;
        }

        public void Save(string sceneName)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            foreach (GameCoreViewer coreViewer in _gameEditor.GameCoreViewers)
            {
                if (coreViewer.AssetType == GameCoreViewer.Type.Scene)
                {
                    string text = JsonConvert.SerializeObject(_gameEditor.GameCoreViewerScene.GameCore.GetAsContent(), settings);
                    File.WriteAllText(ToolsEditor.CombinePath(_gameEditor.ProjectPath, $"./{sceneName}.scene"), text);
                }
                else if (coreViewer.AssetType == GameCoreViewer.Type.Prefab)
                {
                    Asset asset = _gameEditor.AssetManager.Find(coreViewer.AssetPath, true);
                    if (asset != null)
                        ((PrefabAssetContent)asset.AssetContent).Save(coreViewer.GameCore.GameObjects[0].GetRoot()); //Not so ideal, todo find a way to cache the root gameObject of a core (after branch merge)
                }
            }
        }

        public void LoadScene(string sceneName)
        {
            SceneData sceneData = null;
            try
            {
                string text = File.ReadAllText(ToolsEditor.CombinePath(_gameEditor.ProjectPath, $"./{sceneName}.scene"));
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                settings.Error += SilentErrors;
                sceneData = JsonConvert.DeserializeObject<SceneData>(text, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (sceneData != null)
            {
                Selection.SelectObject(null);
                _gameEditor.GameCoreViewers.Remove(_gameEditor.GameCoreViewerScene);
                _gameEditor.SetSceneCore(new GameCoreViewer(
                    new GameCore(_gameEditor),
                    $"./{sceneName}.scene"));
                _gameEditor.GameCoreViewerScene.GameCore.LoadScene(_gameEditor, sceneData);
            }
        }

        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }
    }
}
