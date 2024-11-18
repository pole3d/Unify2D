using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unify2D.Core;
using System.IO;
using UnifyCore;
using Unify2D.Builder;

namespace Unify2D
{
    public class SceneManager
    {
        private static SceneManager _instance;

        private Scene _currentScene;

        public static SceneManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SceneManager();
                }

                return _instance;
            }
        }


        public Scene CurrentScene => _currentScene;
        public int SceneCountInGameSettings => GameSettings.Instance.ScenesInGame.Count;

        public SceneManager()
        {
            _currentScene = new Scene();
        }

        #region Save/Load
        public void Save(Scene scene)
        {
            if (scene.Name == null)
                return;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            string sceneContent = JsonConvert.SerializeObject(scene.GameObjects, settings);

            File.WriteAllText(scene.Path, sceneContent);
        }

        public void SaveCurrentScene()
        {
            Save(_currentScene);
        }
        public void LoadScene(string sceneName)
        {
            ClearScene();

            _currentScene = GetSceneByName(sceneName);
            _currentScene.Init();
        }

        public void LoadSceneWithPath(string scenePath)
        {
            ClearScene();

            _currentScene = new Scene(scenePath);
            _currentScene.Init();
        }
        public void LoadScene(int sceneBuildIndex)
        {
            ClearScene();

            _currentScene = GetSceneByBuildIndex(sceneBuildIndex);

            _currentScene.Init();
        }
        public void LoadNextSceneInBuild()
        {
            ClearScene();

            _currentScene = GetSceneByBuildIndex(_currentScene.BuildIndex + 1);
            _currentScene.Init();
        }
        #endregion



        #region Function
        public Scene GetActiveScene()
        {
            return CurrentScene;
        }


        /// <summary>
        /// Get the Scene at index in the SceneManager's list of loaded Scenes.
        public Scene GetSceneAt(int index)
        {
            return new Scene(GameSettings.Instance.ScenesSave[index].Path);
        }

        //}

        /// Get a Scene from a build index.
        /// Get a Scene struct from a build index.
        /// </summary>
        public Scene GetSceneByBuildIndex(int buildIndex)
        {
            return new Scene(GameSettings.Instance.ScenesSave[buildIndex].Path);
        }

        /// <summary>
        /// Searches through the Scenes loaded for a Scene with the given name.
        /// </summary>
        public Scene GetSceneByName(string name)
        {
            foreach (SceneInfo scene in GameSettings.Instance.ScenesSave)
            {
                if (scene.Name != name)
                    continue;
                return new Scene(scene.Path);
            }
            Console.WriteLine("No scene with the name : " + name);
            return null;
        }


        public Scene GetSceneByPath(string scenePath)
        {
            return new Scene(scenePath);
        }


        private void ClearScene()
        {
            CurrentScene.ClearScene();
        }

        public void AddGameObject(GameObject go)
        {
            if (CurrentScene.GameObjects.Contains(go))
                return;

            CurrentScene.GameObjects.Add(go);
        }

        private void SilentErrors(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }
        #endregion
    }
}
