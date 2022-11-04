using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Assets
{
    internal class ScriptAssetContent : AssetContent
    {
        public string Content = String.Empty;
        Asset _asset;

        public ScriptAssetContent(Asset asset)
        {
            _asset = asset;
        }

        public override void Load()
        {
            base.Load();

            Content = File.ReadAllText(_asset.FullPath);
        }

        internal void Save()
        {
            File.WriteAllText(_asset.FullPath, Content);
        }
    }
}
