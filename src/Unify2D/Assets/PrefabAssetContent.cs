using System;
using System.IO;
using Newtonsoft.Json;
using Unify2D.Core;

namespace Unify2D.Assets
{
    internal class PrefabAssetContent : AssetContent
    {
        private string _serializedText;

        public PrefabAssetContent() : base(null) { }

        public PrefabAssetContent(Asset asset) : base(asset)
        {
        }
        internal void Save(GameObject gameObject)
        {
            // Make so type name should be written in serialized data
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            
            // Write serialized data to file
            _serializedText = JsonConvert.SerializeObject(gameObject, settings);
            File.WriteAllText($"{_asset.FullPath}", _serializedText);
            
            Console.WriteLine($"Prefab {gameObject.Name} saved to {Path.GetFullPath(_asset.FullPath)}");
        }
    }
}