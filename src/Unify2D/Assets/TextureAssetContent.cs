using Genbox.VelcroPhysics.Dynamics.Solver;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Unify2D.Core;
using Unify2D.Toolbox;

namespace Unify2D.Assets
{
    internal class TextureAssetContent : AssetContent
    {
        public TextureAssetContent() : base(null)
        {
        }

        public TextureAssetContent(Asset asset) : base(asset)
        {
        }


        public override void Load()
        {
            if (string.IsNullOrEmpty(_asset.FullPath) == false)
                RawAsset = GameCore.Current.ResourcesManager.GetTexture(_asset.FullPath);
            else
            {
                Texture2D baseRectangle = new Texture2D(GameCore.Current.GraphicsDevice, 100, 100);

                Color[] colors = new Color[100 * 100];

                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.White;
                }

                baseRectangle.SetData(colors);
                RawAsset = (Texture2D)baseRectangle;
            }
        }

        public override void Show(InspectorToolbox inspectorToolbox)
        {

            PropertyInfo nameProperty = RawAsset.GetType().GetProperty("Name");
            ImGui.Text($"{nameProperty.Name}: {nameProperty.GetValue(RawAsset)}");

            PropertyInfo heightProperty = RawAsset.GetType().GetProperty("Height");
            var height = heightProperty.GetValue(RawAsset);
            ImGui.Text($"{heightProperty.Name}: {height}");

            PropertyInfo widthProperty = RawAsset.GetType().GetProperty("Width");
            var width = widthProperty.GetValue(RawAsset);
            ImGui.Text($"{widthProperty.Name}: {width}");

            Texture2D texture = GameCore.Current.ResourcesManager.GetTexture(_asset.FullPath);
            // Here improve performance to avoid binding the same texture everytime
            IntPtr ptr = GameEditor.Instance.GuiRenderer.BindTexture(texture);

            var windowSize = ImGui.GetContentRegionAvail();
            var imageSize = new System.Numerics.Vector2(int.Parse(width.ToString()), int.Parse(height.ToString()));
            var scale = Math.Min(windowSize.X / imageSize.X, windowSize.Y / imageSize.Y);
            var newSize = new System.Numerics.Vector2(imageSize.X * scale, imageSize.Y * scale);
            ImGui.Image(ptr, newSize);
        }
    }
}