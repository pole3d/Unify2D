using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unify2D.Core;

namespace Unify2D.Assets
{
    internal class PrefabAssetContent : AssetContent
    {
        private string _serializedText;
        
        public PrefabAssetContent(Asset asset) : base(asset) { }

        public override void Load()
        {
            base.Load();
            _serializedText = File.ReadAllText(_asset.FullPath);
        }

        public override void OnDragDroppedInGame(GameEditor editor)
        {
            GameObject go = JsonConvert.DeserializeObject<GameObject>(_serializedText, new JsonSerializerSettings());
            editor.SelectObject(go);
        }
    }
}