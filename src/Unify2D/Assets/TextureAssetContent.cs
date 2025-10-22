﻿using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
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
            if (_asset.Path != "null")
                RawAsset = GameCore.Current.ResourcesManager.GetTexture(_asset.Path, false);
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
    }
}