﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;
using Unify2D.Tools;
using static System.Net.Mime.MediaTypeNames;

namespace Unify2D.Assets
{
    internal class ScriptAssetContent : AssetContent
    {
        public string Content = String.Empty;
        public string Path => Tools.ToolsEditor.CombinePath(GameEditor.Instance.AssetsPath, _asset.FullPath);
        Asset _asset;

        public ScriptAssetContent(Asset asset)
        {
            _asset = asset;
        }

        public override void Load()
        {
            base.Load();

            try
            {
                using (FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    using var sr = new StreamReader(stream, Encoding.UTF8);

                    Content = sr.ReadToEnd();
                }

            }
            catch (Exception)
            {

            }


        }

        internal void Save()
        {
            string path = ToolsEditor.CombinePath(GameEditor.Instance.AssetsPath, _asset.FullPath);
            File.WriteAllText(path, Content);
        }
    }
}
