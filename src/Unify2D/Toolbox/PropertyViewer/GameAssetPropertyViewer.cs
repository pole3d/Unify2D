﻿using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Unify2D.Assets;
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

            if (value == null)
            {
                ImGui.InputText("path", ref name, 50);
            }
            else 
            {
                name = value.Name;
                ImGui.InputText("path", ref name, 50);
            }

            if (name != "none")
            {
                bool getAsset = GameEditor.Instance.AssetsToolBox.TryGetAssetFromPath(name, out Asset asset);

                if (getAsset && (value == null || asset != value.Asset))
                {
                    if (instance is SpriteRenderer spriteRenderer)
                    {
                        spriteRenderer.Initialize(GameCore.Current.Game, spriteRenderer.GameObject, name);
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
