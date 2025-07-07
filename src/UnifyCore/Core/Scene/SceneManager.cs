using Newtonsoft.Json;
using System;
using Unify2D.Core;
using System.IO;
using UnifyCore;
using System.Text.Json;
using Unify2D.Builder;
using Unify2D;
using System.Runtime.CompilerServices;
using Genbox.VelcroPhysics.Tools.PathGenerator;
using static System.Formats.Asn1.AsnWriter;
using System.Collections.Generic;
namespace UnifyCore
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

        const string JsonFolderSceneName = "SceneJson.json";
        public Scene CurrentScene => _currentScene;
        public int SceneCountInGameSettings => GameSettings.Instance.ScenesInGame.Count;
        private string _currentProjectPath;
        private string _sceneFolder;
        public SceneManager()
        {
        }

        public void CreateOrOpenSceneAtStart(string path, string sceneFolder)
        {
            //TODO check only once
            GetAllSceneInProject();

            _currentProjectPath = path;
            _sceneFolder = sceneFolder;

            string currentPath = path + sceneFolder;

            #region Load old scene with json
            try
            {
                string pathJson = System.IO.Path.Combine(_currentProjectPath, JsonFolderSceneName);

                bool sceneLoaded = false;

                if (File.Exists(pathJson))
                {
                    SceneInfo deserializedJsonScene = System.Text.Json.JsonSerializer.Deserialize<SceneInfo>(File.ReadAllText(pathJson));

                    SceneInfo sceneInfo = deserializedJsonScene;

                    if (sceneInfo != null && File.Exists(sceneInfo.Path))
                    {
                        sceneLoaded = true;
                        LoadSceneWithPath(sceneInfo.Path);
                    }
                }

                if ( sceneLoaded == false)
                {
                    if (GameSettings.Instance.ScenesSave.Count > 0)
                        LoadSceneWithPath(GameSettings.Instance.ScenesSave[0].Path);
                    else
                        CreateNewScene(path, sceneFolder);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Can't load scene" + ex.ToString());
            }
            #endregion
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

            string directory = System.IO.Path.GetDirectoryName(scene.Path);

            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            File.WriteAllText(scene.Path, sceneContent);
        }
        public void CreateNewScene(string path, string sceneFolder = "")
        {
            _currentProjectPath = path;
            _sceneFolder = sceneFolder;

            const string defaultName = "SampleScene";
            const string sceneExtension = ".scene";


            string currentPath = path + sceneFolder;
            int count = 0;
            if (File.Exists(System.IO.Path.Combine(GameCore.Current.Game.AssetsPath,$"{defaultName}{sceneExtension}")))
            {
                while (File.Exists(System.IO.Path.Combine(GameCore.Current.Game.AssetsPath, $"{defaultName}_" + count + $"{sceneExtension}")))
                    count++;

                _currentScene = new Scene(System.IO.Path.Combine(GameCore.Current.Game.AssetsPath, $"{defaultName}_" + count + $"{sceneExtension}"));
            }
            else
                _currentScene = new Scene(System.IO.Path.Combine(GameCore.Current.Game.AssetsPath, $"{defaultName}{sceneExtension}"));

            _currentScene.Init();
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

            _currentScene = new Scene(scenePath,false);
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


        public void SaveCurrentSceneToJson()
        {
            if (Directory.Exists(_currentProjectPath))
            {
                SceneInfo currentSceneToJson;
                try
                {
                    currentSceneToJson = new SceneInfo(_currentScene.Name, _currentScene.Path);

                    string json = System.Text.Json.JsonSerializer.Serialize(currentSceneToJson);
                    string pathJson = System.IO.Path.Combine(_currentProjectPath, JsonFolderSceneName);

                    // TODO WTF
                    if (File.Exists(pathJson))
                        File.Delete(pathJson);

                    File.WriteAllText(pathJson, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Une erreur s'est produite : " + ex.Message);
                }
            }
        }
        #endregion



        #region Function
        private void GetAllSceneInProject()
        {
            //Récupérer tous les fichiers .scene dans le répertoire et ses sous - répertoires
            foreach (string path in Directory.GetFiles(GameCore.Current.Game.AssetsPath, "*.scene", SearchOption.AllDirectories))
            {
                string name = path.Substring(path.LastIndexOf('\\') + 1);
                SceneInfo scene = new SceneInfo(name, path);
                GameSettings.Instance.AddSceneToList(scene);
            }
        }

        public Scene GetActiveScene()
        {
            return CurrentScene;
        }


        /// <summary>
        /// Get the Scene at index in the SceneManager's list of loaded Scenes.
        /// <summary>
        public Scene GetSceneAt(int index)
        {
            return new Scene(GameSettings.Instance.ScenesSave[index].Path,false);
        }

        /// </summary>
        /// Get a Scene from a build index.
        /// Get a Scene struct from a build index.
        /// </summary>
        public Scene GetSceneByBuildIndex(int buildIndex)
        {
            return new Scene(GameSettings.Instance.ScenesSave[buildIndex].Path, false);
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
                return new Scene(scene.Path, false);
            }
            Debug.Log("No scene with the name : " + name);
            return null;
        }


        public Scene GetSceneByPath(string scenePath)
        {
            return new Scene(scenePath,false);
        }


        private void ClearScene()
        {
            if (CurrentScene != null)
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
