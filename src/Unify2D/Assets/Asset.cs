using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Assets
{
    internal class Asset
    {
        public string Name => _name;
        public string Extension => _extension;  
        public string Path  => _path; 

        public string FullPath => _fullPath; 

        private string _name;
        private string _extension;
        private string _path;
        string _fullPath;


        public Asset(string name, string extension, string path)
        {
            _name = name;
            _extension = extension;
            _path = path;

            _fullPath = System.IO.Path.Combine(path, name + extension);
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
