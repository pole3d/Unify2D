using System;
using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using Unify2D.Assets;
using Unify2D.Core;

namespace Unify2D.Toolbox;

/// <summary>
/// This class is used to draw a property in the inspector toolbox that display a foldout with a list of specific assets from the project files.
/// </summary>
/// <typeparam name="T">The type of the asset for this property</typeparam>
public abstract class AssetTypePropertyViewer<T> : PropertyViewer where T : class
{
    protected abstract T GetAssetFromPath(string path);
    protected abstract string GetPropertyName();
    protected abstract string GetAssetExtension(); //TODO add the possibility to have multiple extension, maybe create a subclass "extension to asset" ? 
    public abstract T GetInitializeAsset();
    public abstract (string name, string path) GetBaseAsset();
    
    /// <summary>
    /// This method is called from the inspector toolbox to draw the property.
    /// </summary>
    /// <param name="property">The property to draw</param>
    /// <param name="instance">The component owning this property</param>
    public override void Draw(PropertyInfo property, object instance)
    {
        T asset = property.GetValue(instance) as T;

        if (asset == null)
        {
            asset = GetInitializeAsset();
            property.SetValue(instance, asset);
            return;
        }
        
        DrawFoldout(ref asset, property, instance);
    }

    /// <summary>
    /// This method search in the assets of the project and get all assets with the right extension.
    /// </summary>
    /// <returns>A tuple with 2 list : names with asset names, paths with assets paths</returns>
    protected virtual (List<string> names, List<string> paths) GetAssetLists()
    {
        (string name, string path) baseAsset = GetBaseAsset();
        List<string> assetNames = new(){baseAsset.name};
        List<string> assetPaths = new(){baseAsset.path};
        
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
    /// <summary>
    /// This method draw the foldout for the asset property with all the files in the project that can be chosen.
    /// </summary>
    /// <param name="asset">The property asset</param>
    /// <param name="property">The property</param>
    /// <param name="instance">The component owning the property</param>
    protected void DrawFoldout(ref T asset, PropertyInfo property, object instance)
    {
        (List<string> names, List<string> paths) lists = GetAssetLists();
        
        bool combo = ImGui.Combo(GetPropertyName(), ref CurrentFoldoutIndex,  lists.names.ToArray(), lists.names.Count);
        if (combo)
        {
            if (CurrentFoldoutIndex == 0)
            {
                asset = GetInitializeAsset();
            }
            else
            {
                asset = GetAssetFromPath(lists.paths[CurrentFoldoutIndex]);
            }
            
            property.SetValue(instance, asset);
        }
    }
    
}