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
            GameAsset value = property.GetValue(instance) as GameAsset; //get the current value of the property in component
            string name = "font null";

            name = value == null ? name : value.Name;
            ImGui.InputText("path", ref name, 50);

            if (name != "font null")
            {
                Asset asset = GameEditor.Instance.AssetsToolBox.GetAssetFromPath(name);
                
                if (asset != null && (value == null || asset != value.Asset))
                {
                    if (instance is UIText text)
                    {
                        text.Initialize(GameCore.Current.Game, text.GameObject, name);
                    }
                }
            }
        }
    }
}
