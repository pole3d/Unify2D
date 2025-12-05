using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class SpriteFontPropertyViewer : AssetTypePropertyViewer<SpriteFont>
    {
        private const string ArialFontPath = @"C:\Windows\Fonts\arial.ttf";
        
        protected override SpriteFont GetAssetFromPath(string path)
        {
            TtfFontBakerResult fontBakeResult = TtfFontBaker.Bake(File.ReadAllBytes(path),
                25,
                1024,
                1024,
                new[]
                {
                    CharacterRange.BasicLatin,
                    CharacterRange.Latin1Supplement,
                    CharacterRange.LatinExtendedA,
                    CharacterRange.Cyrillic
                }
            );
                
            SpriteFont font = fontBakeResult.CreateSpriteFont(GameCore.Current.GraphicsDevice);
            return font;
        }

        protected override string GetPropertyName() => "Font";
        protected override string GetAssetExtension() => ".ttf";
       // public override (string name, string path) GetBaseAsset() => ("Arial", ArialFontPath);
        
        public override void SetAsset(SpriteFont asset, PropertyInfo propertyInfo, Component component)
        {
            propertyInfo.SetValue(component, asset);
        
            //find a way to make this dynamic
            if (component is UIText uiText)
            {
                uiText.SetFont(asset ?? GetInitializeAsset());
            }
        }

        public override SpriteFont GetInitializeAsset()
        {
            return GetAssetFromPath(ArialFontPath);
        }
    }
}
