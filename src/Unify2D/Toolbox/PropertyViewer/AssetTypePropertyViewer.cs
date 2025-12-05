using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Inputs;

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
    //public abstract (string name, string path) GetBaseAsset();
    public abstract void SetAsset(T asset, PropertyInfo propertyInfo, Component component);

    /// <summary>
    /// This method is called from the inspector toolbox to draw the property.
    /// </summary>
    /// <param name="property">The property to draw</param>
    /// <param name="instance">The component owning this property</param>
    public override void Draw(PropertyInfo property, object instance)
    {
        T asset = property.GetValue(instance) as T;

        //if (asset == null)
        //{
        //    asset = GetInitializeAsset();
        //    property.SetValue(instance, asset);
        //    return;
        //}

        DrawFoldout(ref asset, property, instance);
    }

    /// <summary>
    /// This method search in the assets of the project and get all assets with the right extension.
    /// </summary>
    /// <returns>A tuple with 2 list : names with asset names, paths with assets paths</returns>
    protected virtual Dictionary<string, Asset> GetAssetLists()
    {
        Dictionary<string, Asset> dictionary = new();
        //(string name, string path) baseAsset = GetBaseAsset();
        Asset baseAsset = new Asset("", "Rectangle", ".png", "null");
        dictionary.Add(baseAsset.Name, baseAsset);
        string extension = GetAssetExtension();

        if (baseAsset.Path.Length > 0 && File.Exists(baseAsset.Path + ".meta"))
        {
            Asset assetDictionary = new Asset(File.ReadAllText(baseAsset.Path + ".meta"), baseAsset.Name, extension, baseAsset.Path);
            dictionary.Add(baseAsset.Name, assetDictionary);
        }

        //Asset assetDictionary = new Asset(File.ReadAllText(baseAsset.path + ".meta"), baseAsset.name, "", baseAsset.path);
        //dictionary.Add(baseAsset.name, assetDictionary);

        List<Asset> assets = GameEditor.Instance.AssetsToolBox.Assets;
        foreach (Asset asset in assets)
        {
            string name = asset.Name;
            string path = asset.FullPath;
            if (path.EndsWith(extension))
            {
                path = path.Remove(0, 1);
                path = $"{GameCore.Current.Game.Content.RootDirectory}/Assets/{path}";

                Asset assetAdd = new Asset(File.ReadAllText(path + ".meta"), name, extension, path);
                dictionary.Add(name, assetAdd);
            }
        }

        return dictionary;
    }

    private int _currentFoldoutIndex;
    private string _search = string.Empty;
    /// <summary>
    /// This method draw the foldout for the asset property with all the files in the project that can be chosen.
    /// </summary>
    /// <param name="asset">The property asset</param>
    /// <param name="property">The property</param>
    /// <param name="instance">The component owning the property</param>
    protected void DrawFoldout(ref T asset, PropertyInfo property, object instance)
    {
        Dictionary<string, Asset> dictionary = GetAssetLists();
        var dictionaryList = dictionary.ToList();

        int foldoutIndex = _currentFoldoutIndex;
        if (ImGui.BeginCombo($"{GetPropertyName()}", dictionaryList[_currentFoldoutIndex].Key))
        {
            ImGui.InputText("search", ref _search, 100);
            Dictionary<string, Asset> filteredFileDictionary = new();
            foreach (KeyValuePair<string, Asset> pair in dictionary)
            {
                if (pair.Key.ToUpper().Contains(_search.ToUpper()) == false) continue;
                filteredFileDictionary.Add(pair.Key, pair.Value);
            }

            for (int i = 0; i < dictionary.Count; i++)
            {
                bool isSelected = (_currentFoldoutIndex == i);
                bool isInFilter = string.IsNullOrEmpty(_search) || filteredFileDictionary.ContainsKey(dictionaryList[i].Key);
                if (isInFilter && ImGui.Selectable(dictionaryList[i].Key, isSelected))
                {
                    foldoutIndex = i;
                }
                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }

        if (foldoutIndex == _currentFoldoutIndex) return;
        _currentFoldoutIndex = foldoutIndex;

        var selected = dictionaryList[_currentFoldoutIndex].Value;

        T assetContent = selected.AssetContent.RawAsset as T;
        if (assetContent == null)
        {
            selected.AssetContent.Load();
        }

        SetAsset(selected.AssetContent.RawAsset as T, property, instance as Component);

        // property.SetValue(instance, asset);
    }


}