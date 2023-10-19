using Newtonsoft.Json;


namespace Unify2D.Core
{
    public class GameAsset
    {
        [JsonIgnore]
        public string Name => _name;

        [JsonIgnore]
        public object Asset => _asset;

        [JsonIgnore]
        object _asset;

        [JsonProperty]
        private string _name;

        public GameAsset(object asset , string name)
        {
            _asset = asset;
            _name = name;
        }

        public void Release()
        {

        }

        public void Acquire()
        {

        }

    }
}
