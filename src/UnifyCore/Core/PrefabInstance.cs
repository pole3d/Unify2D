using System.IO;
using Newtonsoft.Json;

namespace Unify2D.Core
{
    public class PrefabInstance : Object
    {
        [JsonIgnore] public string PrefabAssetPath => _prefabAssetPath;
        [JsonIgnore] public GameObject LinkedGameObject => _gameObject;
        
        [JsonProperty] private string _prefabAssetPath;
        // todo here: list of overrides
        [JsonIgnore] GameObject _gameObject;

        public PrefabInstance(string assetPath)
        {
            _prefabAssetPath = assetPath;
        }

        public GameObject InstantiateAndLinkGameObject()
        {
            _gameObject = GameObject.Instantiate(_prefabAssetPath);
            _gameObject.LinkToPrefabInstance(this);
            return _gameObject;
        }
    }
}
