using System;
using System.IO;
using Newtonsoft.Json;
using Unify2D.Core;

namespace Unify2D.Assets
{
    /// <summary>
    /// The PrefabAssetContent class is responsible for managing the content of prefab assets.
    /// It provides functionality to save a GameObject to a file in a serialized format using JSON.
    /// </summary>
    internal class PrefabAssetContent : AssetContent
    {
        public GameObject InstantiatedGameObject { get; private set; }
        
        private string _serializedText;

        public PrefabAssetContent() : base(null) { }

        public PrefabAssetContent(Asset asset) : base(asset)
        {
        }

        public override void Load()
        {
            base.Load();
            InstantiateGameObjectOnLoad();
        }

        private void InstantiateGameObjectOnLoad()
        {
            PrefabInstance prefabInstance = new PrefabInstance($"{Asset.FullPath}");
            InstantiatedGameObject = prefabInstance.InstantiateAndLinkGameObject();
            InstantiatedGameObject.Name = Asset.Name;
            Asset.SetMegaPath(InstantiatedGameObject.GetOriginalAssetPath());
        }

        internal void Save(GameObject gameObject)
        {
            // Make so type name should be written in serialized data
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            
            // Write serialized data to new file
            _serializedText = JsonConvert.SerializeObject(gameObject, settings);
            File.WriteAllText(_asset.FullPath, _serializedText);
            
            Console.WriteLine($"Prefab {gameObject.Name} saved on file!");// to {Path.GetFullPath(_asset.FullPath)}");
        }
        
        // TODO: Merge both function because path problems
        internal void SavePrefab(GameObject gameObject)
        {
            // Make so type name should be written in serialized data
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            
            // Write serialized data to new file
            _serializedText = JsonConvert.SerializeObject(gameObject, settings);
            File.WriteAllText(_asset.MegaPath, _serializedText);
            
            Console.WriteLine($"Prefab {gameObject.Name} saved!");// to {Path.GetFullPath(_asset.FullPath)}");
        }
    }
}