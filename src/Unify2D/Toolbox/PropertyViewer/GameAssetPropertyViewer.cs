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
            GameAsset value = property.GetValue(instance) as GameAsset;
            string name = "none";
            string newName = "none";


            if (value == null)
            {
                ImGui.InputText("path", ref newName, 50);
            }
            else
            {
                name = value.Name;
                newName = value.Name;
                ImGui.InputText("path", ref newName, 50);
            }

            if (newName == name)
                return;

            name = newName;

            if (name != "none")
            { 
                GameEditor.Instance.AssetsToolBox.TryGetAssetFromPath(name, out var asset);

                if (asset != null && (value == null || asset != value.Asset))
                {
                    if (instance is SpriteRenderer spriteRenderer)
                    {
                        if (spriteRenderer.GameObject == null) // is prefab TODO : better system to detect
                        {
                            property.SetValue(instance,asset.ToGameAsset());
                        }
                        else
                            spriteRenderer.Initialize(GameCore.Current.Game, spriteRenderer.GameObject, asset.ToGameAsset());
                    }
                    if (instance is UIImage imageRenderer)
                    {
                        if (imageRenderer.GameObject == null) // is prefab TODO : better system to detect
                        {
                            property.SetValue(instance, asset.ToGameAsset());
                        }
                        else
                            imageRenderer.Initialize(GameCore.Current.Game, imageRenderer.GameObject, asset.ToGameAsset());
                    }
                }
            }

            if (value != null)
            {
                Texture2D texture = value.Asset as Texture2D;
                TextureBound textureBound = GameEditor.Instance.InspectorToolbox.GetTextureBound(texture);
                if (textureBound == null)
                {
                    IntPtr ptr = GameEditor.Instance.GuiRenderer.BindTexture(texture);

                    textureBound = new TextureBound { IntPtr = ptr, Texture = texture };
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
