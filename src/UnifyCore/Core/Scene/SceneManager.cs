using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unify2D.Core;
using System.IO;

namespace Unify2D
{
    public class SceneManager
    {
        private static SceneManager _instance;

        public static SceneManager Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new SceneManager();
                }

                return _instance;
            }
        }

        private Scene _currentScene;
        public Scene CurrentScene => _currentScene;

        public SceneManager()
        {
            _currentScene = new Scene();
        }

        public void Save(Scene scene)
        {
            if (scene.Name == null)
            {
                return;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            string sceneContent = JsonConvert.SerializeObject(CurrentScene.GameObjects, settings);

            File.WriteAllText(_currentScene.Path, sceneContent);
        }

        public void SaveCurrentScene()
        {
            Save(_currentScene);
        }

        public void LoadScene(string path)
        {
            _currentScene = new Scene(path);
            _currentScene.Init();
        }

        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }


        private void ClearScene()
        {
            CurrentScene.GameObjects.Clear();
        }
    }
}
