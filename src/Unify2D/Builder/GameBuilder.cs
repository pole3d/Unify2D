using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;

namespace Unify2D.Builder
{
    internal class GameBuilder
    {
        const string TemplatePath = "./GameTemplate";
        const string AssetsPath = "./Assets";
        const string BuildPath = "./Build";
        const string ExeName = "UnifyGame.exe";

        GameCore _core;

        public void Build( GameCore core)
        {
            _core = core;

            if (Directory.Exists(BuildPath) == false)
                Directory.CreateDirectory(BuildPath);

            if (Directory.Exists( Path.Combine( BuildPath,AssetsPath)) == false)
                Directory.CreateDirectory(Path.Combine(BuildPath, AssetsPath));

            foreach (var file in Directory.GetFiles(TemplatePath))
            {
                string fileName = Path.GetFileName(file);
                string newPath = Path.Combine( BuildPath , fileName );
                File.Copy(file, newPath, true);
            }

            foreach (var file in Directory.GetFiles(AssetsPath))
            {
                string fileName = Path.GetFileName(file);
                string newPath = Path.Combine(BuildPath,AssetsPath, fileName);
                File.Copy(file, newPath, true);
            }

            Save();
        }

        void Save()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            string text = JsonConvert.SerializeObject(_core.GameObjects, settings);

            File.WriteAllText(Path.Combine(BuildPath,"test.scene"), text);
        }

        public void StartBuild()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine(BuildPath, ExeName);
            startInfo.WorkingDirectory = BuildPath;

            Process.Start(startInfo);
        }
    }
}
