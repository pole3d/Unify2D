using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Assets
{
    internal class Asset
    {
        public string Name { get; set; }

        public Asset(string name)
        {
            Name = name;
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
