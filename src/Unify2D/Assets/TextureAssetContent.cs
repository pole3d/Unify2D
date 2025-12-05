using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.IO;
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

            if (string.IsNullOrEmpty(_asset.Path) == false)
                RawAsset = GameCore.Current.ResourcesManager.GetTexture( Path.Combine(  _asset.Path , _asset.Name ) + _asset.Extension);
            else
            {
                //Texture2D baseRectangle = new Texture2D(GameCore.Current.GraphicsDevice, 100, 100);

                //Color[] colors = new Color[100 * 100];

                //for (int i = 0; i < colors.Length; i++)
                //{
                //    colors[i] = Color.White;
                //}

                //baseRectangle.SetData(colors);
                //RawAsset = (Texture2D)baseRectangle;
            }
        }
    }
}