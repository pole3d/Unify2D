using System;
using System.IO;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Assets
{
    internal class ScriptAssetContent : AssetContent
    {
        public string Content = String.Empty;

        public ScriptAssetContent(Asset asset) : base(asset) { }
        
        public override void Load()
        {
            base.Load();

            Content = File.ReadAllText($"{GameEditor.Instance.AssetsPath}{_asset.FullPath}");
        }

        public override void OnDragDroppedInGame(GameEditor editor)
        {
            GameObject go = new GameObject() { Name = _asset.Name };
            GameCore.Current.AddGameObject(go);
            //TODO : Add component to the gameObject
            editor.SelectObject(go);
        }

        internal void Save()
        {
            string path = ToolsEditor.CombinePath(GameEditor.Instance.AssetsPath, _asset.FullPath);
            File.WriteAllText(path, Content);
        }
    }
}
