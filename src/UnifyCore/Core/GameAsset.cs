using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
