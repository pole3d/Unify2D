using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unify2D.Core;
using System.IO;
using UnifyCore;
using System.Text.Json;
using Unify2D.Builder;
using Unify2D;
using System.Runtime.CompilerServices;
using Genbox.VelcroPhysics.Tools.PathGenerator;
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

        public Scene CurrentScene => _currentScene;
        public int SceneCountInGameSettings => GameSettings.Instance.ScenesInGame.Count;
        private string _currentProjectPath;
        private string _sceneFolder;
        public SceneManager()
        {
        }

        public void CreateOrOpenSceneAtStart(string path, string sceneFolder)
        {
            _currentProjectPath = path;
            _sceneFolder = sceneFolder;

            string currentPath = path + sceneFolder;

            #region Load scene with json
            try
            {
                string pathJson = System.IO.Path.Combine(_currentProjectPath, JsonFolderSceneName);
                if (File.Exists(pathJson))
                {
                    List<SceneInfo> deserializedJsonScene = System.Text.Json.JsonSerializer.Deserialize<List<SceneInfo>>(File.ReadAllText(pathJson));

                    for (int i = 0; i < deserializedJsonScene.Count; i++)
                    {
                        SceneInfo sceneInfo = deserializedJsonScene[i];
                        sceneInfo.BuildIndex = i;
                        GameSettings.Instance.AddSceneToList(sceneInfo);
                    }

                    if (GameSettings.Instance.ScenesSave.Count > 0)
                        LoadScene(GameSettings.Instance.ScenesSave[0].Name);
                    else
                        CreateNewScene(path, sceneFolder);
                }
                else
                {
                    CreateNewScene(path, sceneFolder);
                    Console.WriteLine("Problem with your folder json, here your path : " + pathJson);
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

            File.WriteAllText(scene.Path, sceneContent);
        }
        public void CreateNewScene(string path, string sceneFolder)
        {
            _currentProjectPath = path;
            _sceneFolder = sceneFolder;

            string currentPath = path + sceneFolder;
            int count = 0;
            if (File.Exists(System.IO.Path.Combine(currentPath, "SampleScene.scene")))
            {
                while (File.Exists(System.IO.Path.Combine(currentPath, "SampleScene_" + count + ".scene")))
                    count++;

                _currentScene = new Scene(System.IO.Path.Combine(currentPath, "SampleScene_" + count + ".scene"), true);
            }
            else
                _currentScene = new Scene(System.IO.Path.Combine(currentPath, "SampleScene.scene"), true);

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

        private const string AssetsPath = "Assets";
        const string JsonFolderSceneName = "SceneJson.json";

        public void SaveAllSceneToJson()
        {
            SaveCurrentScene();
            if (Directory.Exists(_currentProjectPath))
            {
                List<SceneInfo> listSceneToJson = new List<SceneInfo>();
                try
                {
                    // Récupérer tous les fichiers .scene dans le répertoire et ses sous-répertoires
                    foreach (string item in Directory.GetFiles(_currentProjectPath, "*.scene", SearchOption.AllDirectories))
                    {
                        string name = item.Substring(item.LastIndexOf('\\') + 1);
                        //string path = item.Substring(item.IndexOf('\\') + 1);
                        SceneInfo scene = new SceneInfo(name, item);
                        listSceneToJson.Add(scene);
                    }
                    string json = System.Text.Json.JsonSerializer.Serialize(listSceneToJson);
                    string pathJson = System.IO.Path.Combine(_currentProjectPath, JsonFolderSceneName);
                    Console.WriteLine("path json : " + pathJson);

                    if (File.Exists(pathJson))
                        File.Delete(pathJson);
                    
                    File.Create(pathJson).Close();
                    File.WriteAllText(pathJson, json);
                    FileInfo jsonFile = new FileInfo(pathJson);
                    jsonFile.Attributes = FileAttributes.Hidden;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Une erreur s'est produite : " + ex.Message);
                }
            }
        }
        #endregion



        #region Function
        public Scene GetActiveScene()
        {
            return CurrentScene;
        }


        /// <summary>
        /// Get the Scene at index in the SceneManager's list of loaded Scenes.
        /// <summary>
        public Scene GetSceneAt(int index)
        {
            return new Scene(GameSettings.Instance.ScenesSave[index].Path);
        }

        /// </summary>
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
            Debug.Log("No scene with the name : " + name);
            return null;
        }


        public Scene GetSceneByPath(string scenePath)
        {
            return new Scene(scenePath);
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
