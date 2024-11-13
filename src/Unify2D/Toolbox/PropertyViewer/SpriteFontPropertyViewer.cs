using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using Unify2D.Assets;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class SpriteFontPropertyViewer : AssetTypePropertyViewer<SpriteFont>
    {
        private const string ArialFontPath = @"C:\\Windows\\Fonts\arial.ttf";
        
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

        protected override string GetAssetExtension()
        {
            return ".ttf";
        }

        public override void InitializeProperty(ref SpriteFont asset, PropertyInfo property, object instance)
        {
            asset = GetAssetFromPath(ArialFontPath);
            property.SetValue(instance, asset);
        }

        protected override (List<string> names, List<string> paths) GetAssetLists()
        {
            (List<string> names, List<string> paths) assetLists = base.GetAssetLists();

            List<string> spriteFontNames = ["Arial"];
            spriteFontNames.AddRange(assetLists.names);
            
            List<string> spriteFontPaths = [ArialFontPath];
            spriteFontPaths.AddRange(assetLists.paths);
            
            return (spriteFontNames, spriteFontPaths);
        }
    }
}
