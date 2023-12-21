using System.Collections.Generic;

namespace Unify2D.Core
{
    public class GameCoreContent
    {
        public List<GameObject> GameObjects => _gameObjects;
        public List<PrefabInstance> PrefabInstances => _prefabInstances;

        private List<GameObject> _gameObjects;
        private List<PrefabInstance> _prefabInstances;

        internal GameCoreContent(List<GameObject> gameObjects, List<PrefabInstance> prefabInstances)
        {
            _gameObjects = gameObjects;
            _prefabInstances = prefabInstances;
        }

        public void PrefabInstancesToGameObjects()
        {
            foreach (PrefabInstance prefabInstance in _prefabInstances)
            {
                _gameObjects.Add(GameObject.Instantiate(prefabInstance.PrefabAssetPath));
            }
        }
    }
}
