using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Builder
{
    internal class GameBuilder
    {
        const string TemplatePath = "./GameTemplate";
        const string BuildPath = "./Build";
        const string ExeName = "UnifyGame.exe";


        public void Build()
        {
            foreach (var file in Directory.GetFiles(TemplatePath))
            {
                string fileName = Path.GetFileName(file);
                string newPath = Path.Combine( BuildPath , fileName );
                File.Copy(file, newPath, true);

            }
        }

        public void StartBuild()
        {
            Process.Start(Path.Combine(BuildPath,ExeName));
        }
    }
}
