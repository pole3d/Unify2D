using System;
using Unify2D.Core;

namespace Unify2D.Assets;

internal class GameCoreInfo
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

    public GameCoreInfo(GameCore core, string assetPath)
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