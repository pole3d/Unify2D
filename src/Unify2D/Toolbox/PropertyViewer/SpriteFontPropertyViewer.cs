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
    internal class SpriteFontPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance) //instance = component
        {
            SpriteFont font = property.GetValue(instance) as SpriteFont;
            
            if (font == null)
            {
                DrawFoldoutFont(ref font, property, instance, true);
                return;
            }
            
            DrawFoldoutFont(ref font, property, instance);
        }

        private int _currentFoldoutItem = 0;
        private void DrawFoldoutFont(ref SpriteFont font, PropertyInfo property, object instance, bool forceInitialize = false)
        {
            List<string> fontsNames = ["Arial"];
            List<string> fontsPaths = [@"C:\\Windows\\Fonts\arial.ttf"];
            List<Asset> assets = GameEditor.Instance.AssetsToolBox.Assets;
            foreach (Asset asset in assets)
            {
                string name = asset.Name;
                string path = asset.FullPath;
                if (path.EndsWith(".ttf"))
                {
                    path = path.Remove(0,1);
                    path = $"{GameCore.Current.Game.Content.RootDirectory}/Assets/{path}";
                    
                    fontsNames.Add(name);
                    fontsPaths.Add(path);
                }
            }
            
            if (forceInitialize)
            {
                font = GetSpriteFont(fontsPaths[0]);
                property.SetValue(instance, font);
            }
            
            bool combo = ImGui.Combo("font", ref _currentFoldoutItem,  fontsNames.ToArray(), fontsNames.Count);
            if (combo)
            {
                font = GetSpriteFont(fontsPaths[_currentFoldoutItem]);
                property.SetValue(instance, font);
            }
        }
        
        public static SpriteFont GetSpriteFont(string path)
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
    }
}
