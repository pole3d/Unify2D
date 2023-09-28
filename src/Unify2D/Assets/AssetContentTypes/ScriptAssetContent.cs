using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;

namespace Unify2D.Assets
{
    internal class ScriptAssetContent : AssetContent
    {
        public string Content = String.Empty;

        public ScriptAssetContent(Asset asset) : base(asset) { }
        
        public override void Load()
        {
            base.Load();

            Content = File.ReadAllText(  $"{GameEditor.Instance.AssetsPath }{_asset.FullPath}" );
        }

        public override void OnDragDroppedInGame(GameEditor editor)
        {
            GameObject go = new GameObject() { Name = _asset.Name };
            editor.SelectObject(go);
        }

        internal void Save()
        {
            File.WriteAllText(_asset.FullPath, Content);
        }
    }
}
