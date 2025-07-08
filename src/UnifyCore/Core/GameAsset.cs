using Newtonsoft.Json;


namespace Unify2D.Core
{
    /// <summary>
    /// The <see cref="GameAsset"/> class represents a central reference for asset objects
    /// within the game, providing a name and a reference to an underlying object that needs
    /// to be cast into the appropriate type. This class is intended for use as a
    /// middle point for referencing and saving various types of assets.
    /// </summary>
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
