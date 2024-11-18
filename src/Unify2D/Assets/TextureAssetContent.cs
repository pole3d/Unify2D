using System;
using System.IO;
using Newtonsoft.Json;
using Unify2D.Assets;

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
    }
}