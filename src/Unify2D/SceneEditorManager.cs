using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unify2D.Core;
using Unify2D.Tools;
using System.IO;

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
            string text = JsonConvert.SerializeObject(_gameEditor.GameCore.GameObjects, settings);

            File.WriteAllText(ToolsEditor.CombinePath(_gameEditor.ProjectPath, $"./{sceneName}.scene"), text);
        }

        public void LoadScene(string sceneName)
        {
            ClearScene();

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
                _gameEditor.GameCore.LoadScene(_gameEditor, gameObjects);
            }
        }

        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }


        private void ClearScene()
        {
            _gameEditor.GameCore.GameObjects.Clear();

            Selection.UnSelectObject();
        }
    }
}
