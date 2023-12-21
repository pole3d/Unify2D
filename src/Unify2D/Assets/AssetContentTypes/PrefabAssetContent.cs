using System;
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
            // GameObject go = InstantiateGameObject();
            // GameCore.Current.AddGameObjectImmediate(go);
            // Selection.SelectObject(go);
            PrefabInstance pi = InstantiatePrefab();
            GameCore.Current.AddPrefabInstance(pi);
            Selection.SelectObject(pi.LinkedGameObject);
        }

        public GameObject InstantiateGameObject()
        {
            // Get serialized text
            _serializedText = File.ReadAllText($"{GameEditor.Instance.AssetsPath}{_asset.FullPath}");
            // Make so type name should be read
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            // Create gameObject
            return JsonConvert.DeserializeObject<GameObject>(_serializedText, settings);
        }

        public PrefabInstance InstantiatePrefab()
        {
            return new PrefabInstance(_asset.FullPath);
        }

        internal void Save(GameObject gameObject)
        {
            // Make so type name should be written in serialized data
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            // Write serialized data to file
            _serializedText = JsonConvert.SerializeObject(gameObject, settings);
            File.WriteAllText(_asset.FullPath, _serializedText);
        }
    }
}