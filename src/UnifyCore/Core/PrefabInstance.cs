namespace Unify2D.Core
{
    public class PrefabInstance : Object
    {
        public string PrefabAssetPath => _prefabAssetPath;
        public GameObject LinkedGameObject => _gameObject;
        
        private string _prefabAssetPath;
        private GameObject _gameObject;

        public PrefabInstance(string assetPath)
        {
            _prefabAssetPath = assetPath;
        }
        
        internal GameObject InstantiateAndLinkGameObject()
        {
            _gameObject = GameObject.Instantiate(_prefabAssetPath);
            _gameObject.LinkToPrefabInstance(this);
            //apply overrides here
            return _gameObject;
        }
    }
}
