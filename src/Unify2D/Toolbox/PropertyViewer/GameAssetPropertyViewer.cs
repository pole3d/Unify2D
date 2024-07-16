using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;
using Unify2D.Core.Graphics;

namespace Unify2D.Toolbox
{
    internal class GameAssetPropertyViewer : PropertyViewer
    {


        public override void Draw(PropertyInfo property, object instance)
        {
            GameAsset value = property.GetValue(instance) as GameAsset;
            string name = "none";

            if (value == null)
            {
                ImGui.InputText("path", ref name, 50);
            }
            else 
            {
                name = value.Name;
                ImGui.InputText("path", ref name, 50);
            }

            if (name != "none")
            {
                var asset = GameEditor.Instance.AssetsToolBox.GetAssetFromPath(name);

                if (asset != null && (value == null || asset != value.Asset))
                {
                    if (instance is SpriteRenderer spriteRenderer)
                    {
                        spriteRenderer.Initialize(GameCore.Current.Game, spriteRenderer.GameObject, name);
                    }
                }
            }

            if (value != null)
            {
                Texture2D texture = value.Asset as Texture2D;
                InspectorToolbox.TextureBound textureBound = GameEditor.Instance.InspectorToolbox.GetTextureBound(texture);
                if (textureBound == null)
                {
                    IntPtr ptr = GameEditor.Instance.GuiRenderer.BindTexture(texture);

                    textureBound = new InspectorToolbox.TextureBound { IntPtr = ptr, Texture = texture };
                    GameEditor.Instance.InspectorToolbox.AddTextureBound(textureBound);

                }
                else
                {
                    ImGui.Image(textureBound.IntPtr, new System.Numerics.Vector2(40, 40));
                }

            }
        }
    }
}
