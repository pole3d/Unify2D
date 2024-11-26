using System.Collections.Generic;
using System.Linq;
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
    protected virtual Dictionary<string, string> GetAssetLists()
    {
        Dictionary<string, string> dictionary = new();
        (string name, string path) baseAsset = GetBaseAsset();
        dictionary.Add(baseAsset.name, baseAsset.path);
        
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
                    
                dictionary.Add(name,path);
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
        Dictionary<string, string> dictionary = GetAssetLists();
        var dictionaryList = dictionary.ToList();

        if (ImGui.BeginCombo($"{GetPropertyName()}", dictionaryList[_currentFoldoutIndex].Key))
        {
            ImGui.InputText("search", ref _search, 100);
            Dictionary<string, string> filteredFileDictionary = new();
            foreach (KeyValuePair<string, string> pair in dictionary)
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
                    _currentFoldoutIndex = i;
                }
                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus(); 
                }
            }
            ImGui.EndCombo();
        }
        
        asset = _currentFoldoutIndex == 0 ? GetInitializeAsset() : GetAssetFromPath(dictionaryList[_currentFoldoutIndex].Value);
        property.SetValue(instance, asset);
    }
    
}