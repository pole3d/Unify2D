using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Assets
{
    /// <summary>
    /// The PrefabAssetContent class is responsible for managing the content of prefab assets.
    /// It provides functionality to save a GameObject to a file in a serialized format using JSON.
    /// </summary>
    internal class PrefabAssetContent : AssetContent
    {
        public GameObject InstantiatedGameObject { get; private set; }
        public List<GameObject> GameObjectsInstantiated => _gameObjectsInstantiated;
        public string SerializedText => _serializedText;
        
        
        private List<GameObject> _gameObjectsInstantiated = new List<GameObject>();
        private string _serializedText;

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
            
            Asset.SetMegaPath(InstantiatedGameObject.GetOriginalAssetPath());
        }

        internal void Save(GameObject gameObject)
        {
            // Make so type name should be written in serialized data
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
            // Write serialized data to new file
            _serializedText = JsonConvert.SerializeObject(gameObject, settings);
            File.WriteAllText( CoreTools.CombinePath(GameCore.Current.Game.AssetsPath, _asset.FullPath), _serializedText);
            
            Console.WriteLine($"Prefab {gameObject.Name} saved on file!");
        }
        
        internal void SavePrefab(GameObject gameObject)
        {
            Save(gameObject);
            
            // Update InstantiatedGameObject to show good infos
            InstantiatedGameObject = gameObject.DeepCopy();

            foreach (var go in _gameObjectsInstantiated)
            {
                go.UpdateFromPrefab(InstantiatedGameObject.DeepCopy());
            }
        }
        
        public void AddGoInstantiated(GameObject go)
        {
            _gameObjectsInstantiated.Add(go);
        }
    }
}