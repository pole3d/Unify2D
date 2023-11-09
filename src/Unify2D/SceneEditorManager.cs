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
                string text = JsonConvert.SerializeObject(coreInfo.GameCore.GameObjects, settings);
                File.WriteAllText(ToolsEditor.CombinePath(_gameEditor.ProjectPath, coreInfo.AssetPath), text);
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
                _gameEditor.SelectObject(null);
                _gameEditor.GameCoresInfo.Remove(_gameEditor.GameCoreInfoScene);
                _gameEditor.SetSceneCore(new GameCoreInfo(
                    new GameCore(_gameEditor, gameObjects),
                    $"./{sceneName}.scene"));
            }
        }

        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }
    }
}
