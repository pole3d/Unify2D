using System;
using System.IO;
using System.Text;
using Unify2D.Core;
using Unify2D.Tools;
using static System.Net.Mime.MediaTypeNames;

namespace Unify2D.Assets
{
    internal class ScriptAssetContent : AssetContent
    {
        public string Content = String.Empty;
        public string Path => Tools.ToolsEditor.CombinePath(GameEditor.Instance.AssetsPath, _asset.FullPath);

        public ScriptAssetContent(Asset asset) : base(asset) { }
        
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

        public override void OnDragDroppedInGame(GameEditor editor)
        {
            GameObject go = new GameObject() { Name = _asset.Name };
            GameCore.Current.AddGameObject(go);
            //TODO : Add component to the gameObject
            Selection.SelectObject(go);
        }

        internal void Save()
        {
            string path = ToolsEditor.CombinePath(GameEditor.Instance.AssetsPath, _asset.FullPath);
            File.WriteAllText(path, Content);
        }
    }
}
