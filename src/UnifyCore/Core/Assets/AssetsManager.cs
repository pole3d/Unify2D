using System;
using System.Collections.Generic;
using System.IO;
using Unify2D;
using Unify2D.Core;

namespace UnifyCore
{
    public class AssetsManager
    {
        string _path;
        private HashSet<string> _extensionsToIgnore = new HashSet<string> { ".csproj", ".dll", ".sln", ".meta" };

        Dictionary<string, GameAsset> _assets = new Dictionary<string, GameAsset>();

        public AssetsManager()
        {

        }

        public GameAsset GetAsset(string guid)
        {
            if (_assets.TryGetValue(guid, out GameAsset asset))
                return asset;

            Unify2D.Debug.LogError($"can't find asset with guid {guid}");

            return null;
        }

        public void AddAsset(GameAsset asset)
        {
            _assets.Add(asset.GUID, asset);
        }

        public void Initialize(string path)
        {
            _path = path;
            _assets.Clear();

            string[] directories = Directory.GetDirectories(path);
            string[] filesInDirectory = Directory.GetFiles(path);

            foreach (string directory in directories)
                AddAssetsDirectory(directory);


            foreach (string file in filesInDirectory)
            {
                CreateAssetFromFile(file);
            }
        }

        private void AddAssetsDirectory(string directory)
        {
            string[] filesInDirectory = Directory.GetFiles($"{directory}");
            string[] directoriesInDirectory = Directory.GetDirectories($"{directory}");

            foreach (string file in filesInDirectory)
            {
                CreateAssetFromFile(file);

            }

            foreach (string dir in directoriesInDirectory)
            {
                AddAssetsDirectory(dir);
            }
        }

        private void CreateAssetFromFile(string file)
        {
            string relativeFile = file.Replace(_path, string.Empty);
            string extension = Path.GetExtension(relativeFile);

            if (_extensionsToIgnore.Contains(extension) == true)
                return;

            string pathMeta = file + ".meta";
            string uid = string.Empty;

            if (File.Exists(pathMeta) == false)
            {
                uid = Guid.NewGuid().ToString();
                File.WriteAllText(pathMeta, uid);
            }
            else
            {
                uid = File.ReadAllText(pathMeta);

                if (_assets.ContainsKey(uid))
                {
                    Debug.LogError($"asset duplication {file} - {uid}");
                    return;
                }
            }

            GameAsset newAsset = new GameAsset(uid, null, 
                Path.GetFileNameWithoutExtension(relativeFile),
                relativeFile);

            _assets.Add(uid, newAsset);
        }
    }
}
