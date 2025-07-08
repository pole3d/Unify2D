using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Unify2D.Assets
{
    public class AssetManager
    {
        internal Dictionary<string, Type> ExtensionToAssetType => _extensionToAssetType;
        internal List<Asset> Assets => _assets;
        internal int NbOfFiles => _nbOfFiles;
        
        private GameEditor _editor;
        
        private readonly Dictionary<string, Type> _extensionToAssetType = new Dictionary<string, Type>();
        
        // Add the supported extensions for each asset type here
        private readonly Dictionary<Type, List<string>> _assetTypeToExtension = new Dictionary<Type, List<string>>()
        {
            { typeof(ScriptAssetContent), new List<string> { ".cs" } },
            { typeof(FontAssetContent), new List<string> { ".ttf" } },
            { typeof(SceneAssetContent), new List<string> { ".scene" } },
            { typeof(PrefabAssetContent), new List<string> { ".prefab" } },
            { typeof(TextureAssetContent), new List<string> { ".png", ".jpg" } }
        };
        
        private List<Asset> _assets = new List<Asset>();
        private int _nbOfFiles;

        
        internal AssetManager(GameEditor editor)
        {
            _editor = editor;

            foreach (KeyValuePair<Type, List<string>> pair in _assetTypeToExtension)
            {
                foreach (string extension in pair.Value)
                {
                    _extensionToAssetType.Add(extension, pair.Key);
                }
            }
        }
        
        internal Asset CreateAsset<T>(string name) where T : AssetContent, new()
        {
            List<string> extensions = _assetTypeToExtension[typeof(T)];
            string extension = extensions[0]; // Utilisez la première extension par défaut
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
            Asset asset = new Asset(nameSb.ToString(), extension, "\\");
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

        internal bool IsAssetExtension(string extension)
        {
            return _extensionToAssetType.ContainsKey(extension);
        }
    }
}
