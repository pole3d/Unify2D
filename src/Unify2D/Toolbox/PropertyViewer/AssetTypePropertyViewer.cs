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
        string extension = GetAssetExtension();

        if (GameEditor.Instance?.AssetsToolBox?.Assets != null)
        {
            foreach (Asset asset in GameEditor.Instance.AssetsToolBox.Assets)
            {
                if (asset != null && !asset.IsDirectory && asset.FullPath != null &&
                    asset.FullPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    if (!dictionary.ContainsKey(asset.Name))
                    {
                        dictionary.Add(asset.Name, asset);
                    }
                }
            }
        }

        return dictionary;
    }

    private int _currentFoldoutIndex;
    private string _search = string.Empty;

    /// <summary>
    /// This method draws the foldout for the asset property with all matching files in the project.
    /// </summary>
    /// <param name="asset">The property asset</param>
    /// <param name="property">The property</param>
    /// <param name="instance">The component owning the property</param>
    protected void DrawFoldout(ref T asset, PropertyInfo property, object instance)
    {
        Dictionary<string, Asset> dictionary = GetAssetLists();
        if (dictionary.Count == 0)
            return;

        var dictionaryList = dictionary.ToList();

        if (_currentFoldoutIndex >= dictionaryList.Count)
            _currentFoldoutIndex = 0;

        int foldoutIndex = _currentFoldoutIndex;
        if (ImGui.BeginCombo($"{GetPropertyName()}##Combo_{property.Name}", dictionaryList[_currentFoldoutIndex].Key))
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

        if (selected.AssetContent != null)
        {
            T assetContent = selected.AssetContent.RawAsset as T;
            if (assetContent == null)
            {
                selected.AssetContent.Load();
            }

        // property.SetValue(instance, asset);
            SetAsset(selected.AssetContent.RawAsset as T, property, instance as Component);
        }
    }
}