using System;
using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using Unify2D.Assets;
using Unify2D.Core;

namespace Unify2D.Toolbox;

public abstract class AssetTypePropertyViewer<T> : PropertyViewer where T : class
{
    protected abstract T GetAssetFromPath(string path);
    protected abstract string GetAssetExtension(); 
    public abstract void InitializeProperty(ref T asset, PropertyInfo property, object instance);
    
    public override void Draw(PropertyInfo property, object instance)
    {
        T asset = property.GetValue(instance) as T;

        if (asset == null)
        {
            InitializeProperty(ref asset, property, instance);
            return;
        }
        
        DrawFoldout(ref asset, property, instance);
    }

    protected virtual (List<string> names, List<string> paths) GetAssetLists()
    {
        List<string> assetNames = new();
        List<string> assetPaths = new();
        
        List<Asset> assets = GameEditor.Instance.AssetsToolBox.Assets;
        string extension = GetAssetExtension();
        foreach (Asset asset in assets)
        {
            string name = asset.Name;
            string path = asset.FullPath;
            if (path.EndsWith(extension))
            {
                path = path.Remove(0,1);
                path = $"{GameCore.Current.Game.Content.RootDirectory}/Assets/{path}";
                    
                assetNames.Add(name);
                assetPaths.Add(path);
            }
        }

        return (assetNames, assetPaths);
    }

    protected int CurrentFoldoutIndex;
    protected void DrawFoldout(ref T asset, PropertyInfo property, object instance)
    {
        (List<string> names, List<string> paths) lists = GetAssetLists();
        
        bool combo = ImGui.Combo("font", ref CurrentFoldoutIndex,  lists.names.ToArray(), lists.names.Count);
        if (combo)
        {
            asset = GetAssetFromPath(lists.paths[CurrentFoldoutIndex]);
            property.SetValue(instance, asset);
        }
    }
    
}