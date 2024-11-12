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
        public AssetContent AssetContent  => _content;
        public bool IsDirectory => _isDirectory;
        public string FullPath => _fullPath;
        public List<Asset> Children => new(_children);

        private string _name;
        private string _extension;
        private string _path;
        private bool _isDirectory;
        private List<Asset> _children = new();
        string _fullPath;
        AssetContent _content;

        public Asset(string name, string extension, string path, bool isDirectory = false)
        {
            _name = name;
            _extension = extension;
            _path = path;
            _isDirectory = isDirectory;

            if ( _extension == ".cs")
            {
                _content = new ScriptAssetContent(this);
            }

            _fullPath = ToolsEditor.CombinePath(path, name + extension);
        }

        public Asset(string name, string path, bool isDirectory = false)
        {
            _name = name;
            _path = path;
            _isDirectory = isDirectory;
            
            _fullPath = ToolsEditor.CombinePath(path, name);
        }
        
        public void AddChild(Asset child)
        {
            if (_isDirectory)
            {
                _children.Add(child);
                Debug.Log($"Add {child.Name} as child of {_name}");
            }
        }

        public void RemoveChild(Asset child)
        {
            if(_isDirectory)
                _children.Remove(child);
        }

        public void SetPath(string path)
        {
            _path = path;
            _fullPath = ToolsEditor.CombinePath(path, _name);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
