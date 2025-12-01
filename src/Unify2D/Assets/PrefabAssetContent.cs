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
            if (IsLoaded) return;

            base.Load();
            InstantiateGameObjectOnLoad();
        }

        public GameObject Instantiate(Scene scene)
        {
            Load();

            var newGameObject = InstantiatedGameObject.DeepCopy();

            newGameObject.ResetComponents();

            newGameObject.Tag = this;
            newGameObject.PrefabGUID = Asset.GUID;
            AddGoInstantiated(newGameObject);

            scene.AddRootGameObject(newGameObject);

            newGameObject.Initialize(GameCore.Current.Game);

            return newGameObject;
        }

        private void InstantiateGameObjectOnLoad()
        {
            PrefabInstance prefabInstance = new PrefabInstance($"{Asset.FullPath}");

            InstantiatedGameObject = prefabInstance.InstantiateAndLinkGameObject();

            InstantiatedGameObject.Name = Asset.Name;
            InstantiatedGameObject.Tag = this;

            // Asset.SetMegaPath(InstantiatedGameObject.GetOriginalAssetPath());
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
            File.WriteAllText(CoreTools.CombinePath(GameCore.Current.Game.AssetsPath, _asset.FullPath), _serializedText);

            Console.WriteLine($"Prefab {gameObject.Name} saved on file!");
        }

        internal void SavePrefab(GameObject gameObject)
        {
            Save(gameObject);

            // Update InstantiatedGameObject to show good infos
            InstantiatedGameObject = gameObject.DeepCopy();

            foreach (var go in _gameObjectsInstantiated)
            {
                if (gameObject == go) continue;

                go.UpdateFromPrefab(InstantiatedGameObject.DeepCopy());
                go.Initialize(GameCore.Current.Game);
            }
        }

        public void AddGoInstantiated(GameObject go)
        {
            _gameObjectsInstantiated.Add(go);
        }


        public static void LinkPrefabToInstance(GameObject go, EditorAssetManager assetManager)
        {
            if (String.IsNullOrEmpty(go.PrefabGUID))
            {
                Console.WriteLine("no prefab !");
                return;
            }

            PrefabAssetContent assetContent = (PrefabAssetContent)assetManager.GetAsset(go.PrefabGUID).AssetContent;
            go.Tag = assetContent;

            assetContent.AddGoInstantiated(go);

        }
    }
}