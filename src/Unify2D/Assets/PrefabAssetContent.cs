using System;
using System.Collections.Generic;
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
        public string SerializedText => _serializedText;
        
        public List<GameObject> GameObjectsInstantiated => _gameObjectsInstantiated;
        
        private string _serializedText;
        private List<GameObject> _gameObjectsInstantiated = new List<GameObject>();

        public PrefabAssetContent() : base(null) { }

        public PrefabAssetContent(Asset asset) : base(asset)
        {
        }

        public override void Load()
        {
            if(IsLoaded) return;
            
            base.Load();
            InstantiateGameObjectOnLoad();
        }

        private void InstantiateGameObjectOnLoad()
        {
            PrefabInstance prefabInstance = new PrefabInstance($"{Asset.FullPath}");
            
            InstantiatedGameObject = prefabInstance.InstantiateAndLinkGameObject();
            InstantiatedGameObject.Name = Asset.Name;
            InstantiatedGameObject.Tag = this;
            // _gameObjectsInstantiated.Add(InstantiatedGameObject);
            // AddGameObject(InstantiatedGameObject);
            
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
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
            // Write serialized data to new file
            _serializedText = JsonConvert.SerializeObject(gameObject, settings);
            File.WriteAllText(_asset.MegaPath, _serializedText);

            foreach (var go in _gameObjectsInstantiated)
            {
                go.UpdateFromPrefab(InstantiatedGameObject.DeepCopy());
            }
            
            Console.WriteLine($"Prefab {gameObject.Name} saved!");// to {Path.GetFullPath(_asset.FullPath)}");
        }
        
        public void AddGoInstantiated(GameObject go)
        {
            _gameObjectsInstantiated.Add(go);
        }
    }
}