using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Core
{
    class GameAsset
    {
        public string Name => _name;

        [JsonIgnore]
        public object _asset;

        [JsonProperty]
        private string _name;

        public GameAsset(object asset , string name)
        {
            _asset = asset;
            _name = name;
        }

    }
}
