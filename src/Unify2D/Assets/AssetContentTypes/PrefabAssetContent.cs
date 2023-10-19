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
            GameObject go = InstantiateGameObject(editor);
            GameCore.Current.AddGameObject(go);
            GameEditor.Instance.SelectObject(go);
        }

        public GameObject InstantiateGameObject(GameEditor editor)
        {
            // Get serialized text
            _serializedText = File.ReadAllText($"{GameEditor.Instance.AssetsPath}{_asset.FullPath}");
            // Make so type name should be read
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            // Create gameObject
            return JsonConvert.DeserializeObject<GameObject>(_serializedText, settings);
        }
    }
}