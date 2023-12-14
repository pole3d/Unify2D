using System;
using System.Collections.Generic;
using System.Reflection;
using Unify2D.Tools;

namespace Unify2D.Assets
{
    internal class Asset
    {
        public string Name => _name;
        public string Extension => _extension;  
        public string Path  => _path; 
        public AssetContent AssetContent => _content;

        public string FullPath => _fullPath; 
        
        private string _name;
        private string _extension;
        private string _path;
        string _fullPath;
        AssetContent _content;

        internal Asset(string name, string extension, string path)
        {
            _name = name;
            _extension = extension;
            _path = path;

            _content = (AssetContent)Activator.CreateInstance(GameEditor.Instance.AssetManager.ExtensionToAssetType[extension], this);

            _fullPath = ToolsEditor.CombinePath(path, name + extension);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
