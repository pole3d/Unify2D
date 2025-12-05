using Microsoft.Xna.Framework.Graphics;
using Unify2D.Core;

namespace Unify2D.Assets
{
    internal class FontAssetContent : AssetContent
    {
        public FontAssetContent() : base(null)
        {
        }

        public FontAssetContent(Asset asset) : base(asset)
        {
        }

        public override void Load()
        {
            if (string.IsNullOrEmpty(_asset.Path) == false)
                RawAsset = GameCore.Current.ResourcesManager.GetFont(_asset.Path);
            else
            {
                RawAsset = null;
            }
        }
    }
}
