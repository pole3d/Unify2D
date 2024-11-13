using System;
using Unify2D.Core;

namespace Unify2D.Assets
{
    /// <summary>
    /// The GameCoreViewer class is responsible for managing the visualization and interaction
    /// with GameCore assets in the project. It determines the type of asset (Scene or Prefab)
    /// based on the file extension and provides access to the associated GameCore instance.
    /// </summary>
    public class GameCoreViewer
    {
        public enum Type
        {
            None,
            Scene,
            Prefab
        }

        public GameCore GameCore => _gameCore;
        public string AssetPath => _assetPath;
        public Type AssetType => _assetType;

        public GameCoreViewer(GameCore core, string assetPath)
        {
            _gameCore = core;
            _assetPath = assetPath;
            
            if (assetPath.EndsWith(".prefab"))
                _assetType = Type.Prefab;
            else if (assetPath.EndsWith(".scene"))
                _assetType = Type.Scene;
            else
                throw new Exception("Unknown asset type for a GameCore. Very weird!");
        }
    
        private GameCore _gameCore;
        private string _assetPath;
        private Type _assetType;
    }
}