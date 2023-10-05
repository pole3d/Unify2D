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

        public override void OnDragDroppedInGame(GameEditor editor)
        {
            _serializedText = File.ReadAllText(_asset.FullPath);
            GameObject go = JsonConvert.DeserializeObject<GameObject>(_serializedText, new JsonSerializerSettings());
            editor.SelectObject(go);
        }
    }
}