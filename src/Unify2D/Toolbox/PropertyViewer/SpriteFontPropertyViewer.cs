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
            
            GameAsset value = property.GetValue(instance) as GameAsset; //get the current value of the property in component
            string name = "";
            
            name = value == null ? name : value.Name;
            ImGui.InputText("path", ref name, 50);
            
            if (text.Font != null)
            {
                return;
            }

            if (value != null && name == value.Name)
            {
                return;
            }
            
            if (name != "")
            {
                Asset asset = GameEditor.Instance.AssetsToolBox.GetAssetFromPath(name);
                
                if (asset != null && (value == null || asset != value.Asset))
                {
                    if (text.Initialize(GameCore.Current.Game, text.GameObject, name))
                    {
                        property.SetValue(instance, name);
                    }
                }
            }
        }
    }
}
