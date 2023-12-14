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
            foreach (GameCoreInfo coreInfo in _gameEditor.GameCoresInfo)
            {
                if (coreInfo.AssetType == GameCoreInfo.Type.Scene)
                {
                    string text = JsonConvert.SerializeObject(_gameEditor.GameCoreInfoScene.GameCore.GameObjects, settings);
                    File.WriteAllText(ToolsEditor.CombinePath(_gameEditor.ProjectPath, $"./{sceneName}.scene"), text);
                }
                else if (coreInfo.AssetType == GameCoreInfo.Type.Prefab)
                {
                    Asset asset = _gameEditor.AssetManager.Find(coreInfo.AssetPath, true);
                    if (asset != null)
                        ((PrefabAssetContent)asset.AssetContent).Save(coreInfo.GameCore.GameObjects[0].GetRoot()); //Not so ideal, todo we should find a way to cache the root gameObject of a core (after branch merge)
                }
            }
        }

        public void LoadScene(string sceneName)
        {
            List<GameObject> gameObjects = null;
            try
            {
                string text = File.ReadAllText(ToolsEditor.CombinePath(_gameEditor.ProjectPath, $"./{sceneName}.scene"));
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
                Selection.SelectObject(null);
                _gameEditor.GameCoresInfo.Remove(_gameEditor.GameCoreInfoScene);
                _gameEditor.SetSceneCore(new GameCoreInfo(
                    new GameCore(_gameEditor),
                    $"./{sceneName}.scene"));
                _gameEditor.GameCoreInfoScene.GameCore.LoadScene(_gameEditor, gameObjects);
            }
        }

        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }
    }
}
