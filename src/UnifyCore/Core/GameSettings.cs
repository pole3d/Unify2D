using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Unify2D.Builder
{
    public class GameSettings
    {
        private static GameSettings _instance;


        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSettings();
                }

                return _instance;
            }
        }

        public List<string> ScenesInGame = new List<string>();
        public void AddSceneToGame()
        {
            Scene currentScene = SceneManager.Instance.CurrentScene;

            if (ScenesInGame.Contains(currentScene.Name) == false)
                return;

            ScenesInGame.Add(currentScene.Name);
        }

        public void AddSceneToList(string sceneName)
        {
            if (ScenesInGame.Contains(sceneName) == true)
                return;

            ScenesInGame.Add(sceneName);
        }
    }
}
