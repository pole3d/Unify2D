using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D
{
    [Serializable]
    public class GameEditorSettingsData
    {
        public string CurrentProjectPath;
    }

    public class GameEditorSettings
    {
        readonly string SettingsFilename = "./unify.settings";

        public GameEditorSettingsData Data => _data;

        GameEditorSettingsData _data;
        GameEditor _editor ;

        public void Load(GameEditor editor)
        {
            _editor = editor;

            try
            {
                string text = File.ReadAllText(GetFilePath());

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                _data = JsonConvert.DeserializeObject<GameEditorSettingsData>(text, settings);

            }
            catch
            {
                _data = new GameEditorSettingsData();
            }
        }

        public void Save()
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                string content = JsonConvert.SerializeObject(_data, settings);
                File.WriteAllText(GetFilePath(), content);
            }
            catch
            {
      
            }
        }

        string GetFilePath()
        {
            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(exePath, SettingsFilename);
        }
    }
}
