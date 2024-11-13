using System.Collections.Generic;
using ImGuiNET;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class SpriteFontPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance) //instance = component
        {
            if (instance is UIText text == false)
            {
                return;
            }
            
            if (text.Font == null)
            {
                DrawFoldoutFont(text, true);
                return;
            }
            
            DrawFoldoutFont(text);
        }

        internal int _currentFoldoutItem = 0;
        private void DrawFoldoutFont(UIText text, bool forceInitialize = false)
        {
            List<string> fontsNames = ["Arial"];
            List<string> fontsPaths = [@"C:\\Windows\\Fonts\arial.ttf"];
            
            //get all the fonts in the assets
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
            
            //if the text font is null, initialize it
            if (forceInitialize)
            {
                text.Initialize(text.GameObject, fontsPaths[0]);
            }
            
            bool combo = ImGui.Combo("font", ref _currentFoldoutItem,  fontsNames.ToArray(), fontsNames.Count);
            if (combo)
            {
                text.Initialize(text.GameObject, fontsPaths[_currentFoldoutItem]);
            }
        }
    }
}
