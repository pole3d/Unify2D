using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using SDL2;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// Drag and dropp 
    /// </summary>
    public static class TextureDropHelper
    {
        public const string AssetPayloadType = "ASSET";
        public static bool IsTextureAsset(Asset asset)
        {
            if (asset == null || asset.IsDirectory) return false;
            if (asset.AssetContent is TextureAssetContent) return true;

            return false;
        }

        public static unsafe Asset GetDroppedAsset(ImGuiPayloadPtr payload)
        {
            if (payload.NativePtr == null) return null;
            if (Clipboard.Content is Asset asset) return asset;
            return null;
        }

        // Draws the Drag and Drop zone for textures 
        public static void DrawDropZone(
            Texture2D currentTexture,
            string currentNameOrPath,
            PropertyInfo property,
            object instance)
        {

            // random size for the moment
            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.20f, 0.25f, 0.30f, 0.7f));

            float availWidth = ImGui.GetContentRegionAvail().X;
            string buttonText;

            // area drag and drop image on sprite renderer (inspector) 
            if (currentTexture != null)
                buttonText = $"[ Deposer image ici ] : {Path.GetFileName(currentNameOrPath)}";
            else
                buttonText = "[ Glisser deposer image ici]";

            ImGui.Button($"{buttonText}");

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload(AssetPayloadType);
                Asset assetToTest = GetDroppedAsset(payload);

                if (assetToTest != null && IsTextureAsset(assetToTest))
                    AssignAssetToComponent(assetToTest, property, instance);

                ImGui.EndDragDropTarget();
            }
        }

        public static void AssignAssetToComponent(Asset asset, PropertyInfo property, object instance)
        {
            if (asset == null || instance == null) return;

            GameAsset gameAsset = asset.ToGameAsset();

            if (instance is SpriteRenderer spriteRenderer)
            {
                if (spriteRenderer.GameObject == null)
                {
                    spriteRenderer.AssetTexture = gameAsset;
                    spriteRenderer.Texture = GameCore.Current.ResourcesManager.GetTexture(asset.FullPath);
                }
                else
                {
                    spriteRenderer.SetAsset(gameAsset);
                }
            }
        }
    }
}