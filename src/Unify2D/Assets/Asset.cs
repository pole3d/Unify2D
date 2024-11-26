using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Tools;

namespace Unify2D.Assets
{
    internal class Asset
    {
        public string Name => _name;
        public string Extension => _extension;  
        public string Path  => _path; 
        public AssetContent AssetContent  { get; set; }
        public bool IsDirectory => _isDirectory;

        public string FullPath => _fullPath; 

        private string _name;
        private string _extension;
        private string _path;
        private bool _isDirectory;
        string _fullPath;


        public Asset(string name, string extension, string path, bool isDirectory = false)
        {
            _name = name;
            _extension = extension;
            _path = path;
            _isDirectory = isDirectory;

            // Create the asset content depending on the extension. Example .jpg and .png will create a TextureAssetContent, .cs will create a ScriptAssetContent...
            AssetContent = (AssetContent)Activator.CreateInstance(GameEditor.Instance.AssetManager.ExtensionToAssetType[extension], this);

            _fullPath = ToolsEditor.CombinePath(path, name + extension);
        }

        public Asset(string name, string path, bool isDirectory = false)
        {
            _name = name;
            _path = path;
            _isDirectory = isDirectory;
            
            _fullPath = ToolsEditor.CombinePath(path, name);
        }

        public void SetPath(string path)
        {
            _path = path;
            _fullPath = ToolsEditor.CombinePath(path, _name);
        }

        public void SetFullPath(string fullpath)
        {
            _fullPath = fullpath;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}