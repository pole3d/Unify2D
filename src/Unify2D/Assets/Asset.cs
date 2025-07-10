using System;
using System.Collections.Generic;
using Unify2D.Core;

namespace Unify2D.Assets
{
    internal class Asset
    {
        public string GUID => _guid;

        public string Name => _name;
        public string Extension => _extension;  
        public string Path  => _path; 
        public AssetContent AssetContent  { get; set; }
        public bool IsDirectory => _isDirectory;
        public string FullPath => _fullPath;
        
        public string MegaPath { get; set; }
        public List<Asset> Children => new(_children);
        public Asset Parent => _parent;

        private string _guid;
        private string _name;
        private string _extension;
        private string _path;
        private bool _isDirectory;
        private List<Asset> _children = new();
        string _fullPath;
        private Asset _parent;

        public Asset(string guid,string name, string extension, string path, bool isDirectory = false)
        {
            _guid = guid;
            _name = name;
            _extension = extension;
            _path = path;
            _isDirectory = isDirectory;

            // Create the asset content depending on the extension. Example .jpg and .png will create a TextureAssetContent, .cs will create a ScriptAssetContent...
            if (String.IsNullOrEmpty(extension) == false)
            {
                AssetContent = (AssetContent)Activator.CreateInstance(GameEditor.Instance.AssetManager.ExtensionToAssetType[extension], this);
            }

            _fullPath = CoreTools.CombinePath(path, name + extension);
        }

        //public Asset(string name, string path, bool isDirectory = false)
        //{
        //    _name = name;
        //    _path = path;
        //    _isDirectory = isDirectory;
            
        //    _fullPath = CoreTools.CombinePath(path, name);
        //}
        
        public GameAsset ToGameAsset()
        {
            return new  GameAsset(GUID, AssetContent, _name , _fullPath);
        }

        public void AddChild(Asset child)
        {
            if (_isDirectory)
            {
                _children.Add(child);
                child._parent = this;
            }
        }

        public void SetName(string name)
        {
            _name = name;
            SetPath(_path);
        }

        public void SetPath(string path)
        {
            _path = path;
            _fullPath = CoreTools.CombinePath(path, _name + _extension);
        }

        public void SetMegaPath(string megaPath)
        {
            MegaPath = megaPath;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}