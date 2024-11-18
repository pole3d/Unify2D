using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Unify2D.Core
{
    public class SceneData
    {
        public List<GameObject> GameObjects => _gameObjects;
        public List<PrefabInstance> PrefabInstances => _prefabInstances;

        private List<GameObject> _gameObjects;
        private List<PrefabInstance> _prefabInstances;

        public SceneData(List<GameObject> gameObjects, List<PrefabInstance> prefabInstances)
        {
            _gameObjects = gameObjects;
            _prefabInstances = prefabInstances;
        }

        internal SceneData(GameObject gameObject)
        {
            _gameObjects = new List<GameObject>(1) { gameObject };
        }

        public void PrefabInstancesToGameObjects()
        {
            foreach (PrefabInstance prefabInstance in _prefabInstances) {
                GameObject go = JsonConvert.DeserializeObject<GameObject>(File.ReadAllText($"{GameCore.Current.Game.AssetsPath}{prefabInstance.PrefabAssetPath}")); //Don't use GameObject.Instantiate, deserialized gameObjects must not be added to the current core yet
                go.ApplyOverridesFromPrefabInstance(prefabInstance);
                _gameObjects.Add(go);
            }
            _prefabInstances.Clear();
        }
    }
}