using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Unify2D.Core;
using Unify2D.Core.Graphics;

namespace Unify2D.Toolbox
{
    internal class GameAssetPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance)
        {
            object valueTemp = property.GetValue(instance);
            GameAsset value = valueTemp as GameAsset;
            string name = "none";
            string newName = "none";

            // Area drag and drop from files explorer 
            Texture2D currentTexture = null;
            if (instance is SpriteRenderer sr)
            {
                currentTexture = sr.Texture;
            }
            else if (instance is UIImage ui)
            {
                currentTexture = ui.Texture;
            }
            else if (value?.Asset is Texture2D tex)
            {
                currentTexture = tex;
            }
            else if (value != null && !string.IsNullOrEmpty(value.Path))
            {
                try
                {
                    currentTexture = GameCore.Current.ResourcesManager.GetTexture(value.Path);
                }
                catch { }
            }

            if (value == null)
            {
                ImGui.InputText("path", ref newName, 50);
            }
            else
            {
                name = value.Path;
                newName = value.Path == null ? String.Empty : value.Path;

                ImGui.InputText("path", ref newName, 50);
            }

            // Draw area drag and drop texture
            TextureDropHelper.DrawDropZone(
                property,
                instance
            );

            if (newName == name) return;

            if (newName != name && newName != "none")
            {
                name = newName;
                GameEditor.Instance.AssetsToolBox.TryGetAssetFromPath(name, out var asset);

                if (asset != null && (value == null || asset != value.Asset))
                {
                    if (instance is SpriteRenderer spriteRenderer)
                    {
                        if (spriteRenderer.GameObject == null) // is prefab TODO : better system to detect
                        {
                            property.SetValue(instance, asset.ToGameAsset());
                        }
                        else
                            spriteRenderer.SetAsset(asset.ToGameAsset());
                    }
                    if (instance is UIImage imageRenderer)
                    {
                        if (imageRenderer.GameObject == null) // is prefab TODO : better system to detect
                        {
                            property.SetValue(instance, asset.ToGameAsset());
                        }
                        else
                            imageRenderer.SetAsset(asset.ToGameAsset());
                    }
                    if (instance is UIText textRenderer)
                    {
                        if (textRenderer.GameObject == null) // is prefab TODO : better system to detect
                        {
                            property.SetValue(instance, asset.ToGameAsset());
                        }
                        else
                            textRenderer.SetAsset(asset.ToGameAsset());
                    }
                }
            }

            if (value != null && currentTexture != null)
            {
                TextureBound textureBound = GameEditor.Instance.InspectorToolbox.GetTextureBound(currentTexture);
                if (textureBound == null)
                {
                    IntPtr ptr = GameEditor.Instance.GuiRenderer.BindTexture(currentTexture);
                    textureBound = new TextureBound { IntPtr = ptr, Texture = currentTexture };
                    GameEditor.Instance.InspectorToolbox.AddTextureBound(textureBound);
                }
                else
                {
                    ImGui.Image(textureBound.IntPtr, new System.Numerics.Vector2(40, 40));
                }
            }
        }
    }
}