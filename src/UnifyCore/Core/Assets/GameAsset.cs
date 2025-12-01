using Microsoft.Xna.Framework.Graphics;
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
        public string GUID => _guid;

        [JsonIgnore]
        public string Name => _name;

        [JsonIgnore]
        public object Asset => _asset;

        [JsonIgnore]
        public string Path => _path;

        [JsonProperty]
        private string _name;

        [JsonProperty]
        private string _guid;

        [JsonIgnore]
        object _asset;

        string _path;


        public GameAsset(string guid, object asset, string name, string path)
        {
            _guid = guid;
            _asset = asset;
            _name = name;
            _path = path;
        }


        public void Release()
        {

        }

        public void Load()
        {

        }

        public Texture2D LoadTexture()
        {
            return GameCore.Current.ResourcesManager.GetTexture(_path);
        }


        public void SetTexture(string path)
        {
            _path = path;
            LoadTexture();
        }


    }
}
