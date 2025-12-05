using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Core.Graphics;

namespace Unify2D.Toolbox
{
    internal class GameAssetPropertyViewer : PropertyViewer
    {
        private int _currentFoldoutIndex;
        private string _search = string.Empty;

        protected virtual string GetPropertyName() => "";

        public virtual void SetAsset(object asset, PropertyInfo propertyInfo, Component component) {}

        public override void Draw(PropertyInfo property, object instance)
        {
            var assets = GameEditor.Instance.EditorAssetManager.Assets;
            var dictionary = new Dictionary<string, Asset>();

            //Burp burp ugly, rework please 🤒
            var extension = new List<string> { ".png", ".bmp", ".gif", ".jpg", ".jpeg", ".tga", ".tif", ".tiff", ".dds" };

            foreach (var item in assets)
            {
                if (extension.Contains(item.Extension))
                    dictionary.Add(item.FullPath, item);
            }

            var dictionaryList = dictionary.ToList();

            int foldoutIndex = _currentFoldoutIndex;
            if (ImGui.BeginCombo($"{GetPropertyName()}", dictionaryList[_currentFoldoutIndex].Key))
            {
                ImGui.InputText("search", ref _search, 100);
                Dictionary<string, Asset> filteredFileDictionary = new();
                foreach (KeyValuePair<string, Asset> pair in dictionaryList)
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

            object assetContent = selected.AssetContent.RawAsset as object;
            if (assetContent == null)
            {
                selected.AssetContent.Load();
            }

            SetAsset(selected.AssetContent.RawAsset as object, property, instance as Component);

            AssignAsset(selected, property, instance as Component);
        }

        void AssignAsset(Asset selected, PropertyInfo property, Component component)
        {
            property.SetValue(component, selected);
        }

        //public override void Draw(PropertyInfo property, object instance)
        //{
        //    object valueTemp = property.GetValue(instance);
        //    GameAsset value = valueTemp as GameAsset;
        //    string name = "none";
        //    string newName = "none";

        //    if (value == null)
        //    {
        //        //ImGui.InputText("path", ref newName, 50);
        //    }
        //    else
        //    {
        //        name = value.Path;
        //        newName = value.Path;
        //        //ImGui.InputText("path", ref newName, 50);
        //    }

        //    if (newName == name)
        //        return;

        //    name = newName;

        //    if (name != "none")
        //    {
        //        GameEditor.Instance.AssetsToolBox.TryGetAssetFromPath(name, out var asset);

        //        if (asset != null && (value == null || asset != value.Asset))
        //        {
        //            if (instance is SpriteRenderer spriteRenderer)
        //            {
        //                if (spriteRenderer.GameObject == null) // is prefab TODO : better system to detect
        //                {
        //                    property.SetValue(instance, asset.ToGameAsset());
        //                }
        //                else
        //                    spriteRenderer.Initialize(GameCore.Current.Game, spriteRenderer.GameObject, asset.ToGameAsset());
        //            }
        //            if (instance is UIImage imageRenderer)
        //            {
        //                if (imageRenderer.GameObject == null) // is prefab TODO : better system to detect
        //                {
        //                    property.SetValue(instance, asset.ToGameAsset());
        //                }
        //                else
        //                    imageRenderer.Initialize(GameCore.Current.Game, imageRenderer.GameObject, asset.ToGameAsset());
        //            }
        //        }
        //    }

        //    if (value != null)
        //    {
        //        Texture2D texture = value.Asset as Texture2D;
        //        TextureBound textureBound = GameEditor.Instance.InspectorToolbox.GetTextureBound(texture);
        //        if (textureBound == null)
        //        {
        //            IntPtr ptr = GameEditor.Instance.GuiRenderer.BindTexture(texture);

        //            textureBound = new TextureBound { IntPtr = ptr, Texture = texture };
        //            GameEditor.Instance.InspectorToolbox.AddTextureBound(textureBound);

        //        }
        //        else
        //        {
        //            ImGui.Image(textureBound.IntPtr, new System.Numerics.Vector2(40, 40));
        //        }
        //    }
        //}
    }
}
