using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unify2D.Assets
{
    public class AssetManager
    {
        internal Dictionary<string, Type> ExtensionToAssetType => _extensionToAssetType;
        internal List<Asset> Assets => _assets;
        internal int NbOfFiles => _nbOfFiles;
        
        private GameEditor _editor;
        private Dictionary<string, Type> _extensionToAssetType = new Dictionary<string, Type>();
        private Dictionary<Type, string> _assetTypeToExtension = new Dictionary<Type, string>()
        {
            { typeof(ScriptAssetContent), ".cs" },
            { typeof(PrefabAssetContent), ".prefab" },
            // { typeof(TextureAssetContent), ".png" }
        };
        
        private List<Asset> _assets = new List<Asset>();
        private int _nbOfFiles;

        internal AssetManager(GameEditor editor)
        {
            _editor = editor;
            
            foreach (KeyValuePair<Type,string> pair in _assetTypeToExtension)
            {
                _extensionToAssetType.Add(pair.Value, pair.Key);
            }
        }

        internal void RefreshDatabase()
        {
            _assets.Clear();
            string path = _editor.AssetsPath;

            if (String.IsNullOrEmpty(path))
                return;

            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            var files = Directory.GetFiles(path);
            _nbOfFiles = files.Length;

            foreach (var file in files)
            {
                string relativeFile = file.Replace(path, string.Empty);

                // _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeFile),
                //     Path.GetExtension(relativeFile),
                //     Path.GetDirectoryName(relativeFile)));
            }
        }
        
        internal Asset CreateAsset<T>(string name) where T : AssetContent, new()
        {
            string extension = _assetTypeToExtension[typeof(T)];
            StringBuilder nameSb = new StringBuilder(name);
            int safeguard = 0;
            // Append number in filename if file already exists
            while (File.Exists(Path.Combine(_editor.AssetsPath, $"{nameSb}{extension}")))
            {
                if (++safeguard >= int.MaxValue)
                    throw new Exception("Too many files with the same name, or potentially stuck in an infinite loop. Prefab save failed.");

                char lastChar = nameSb[nameSb.Length - 1];
                if (char.IsDigit(lastChar))
                {
                    nameSb.Length--;
                    if (lastChar == '9')
                        nameSb.Append("10");
                    else
                        nameSb.Append((char)(lastChar + 1));
                }
                else
                    nameSb.Append('1');
            }

            string filePath = Path.Combine(_editor.AssetsPath, $"{nameSb}{extension}");
            File.Create(filePath).Close();

            // Create the asset and initialize its content
            Asset asset = new Asset(nameSb.ToString(), extension, _editor.AssetsPath);
            asset.AssetContent = (T)Activator.CreateInstance(typeof(T), asset); // Ensure AssetContent is correctly instantiated with the Asset

            // Refresh the AssetsToolbox
            _editor.AssetsToolBox.Reset();

            return asset;
        }

        internal Asset Find(string path, bool isFullPath = false)
        {
            if (isFullPath == false)
                path = Path.Combine(_editor.AssetsPath, path);
            foreach (Asset asset in Assets)
            {
                if (asset.FullPath == path)
                    return asset;
            }

            throw new Exception($"No Asset found at path \"{path}\"");
        }
    }
}
