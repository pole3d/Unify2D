using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Core;
using static Unify2D.Toolbox.InspectorToolbox;

namespace Unify2D.Toolbox
{
    internal class GameAssetPropertyViewer : PropertyViewer
    {
        public override void Draw(PropertyInfo property, object instance)
        {
            GameAsset value = property.GetValue(instance) as GameAsset;
            string name = "none";

            if (value != null)
            {
                name = value.Name;
                ImGui.InputText("path", ref name, 50);
            }

            if (value != null)
            {
                Texture2D texture = value.Asset as Texture2D;
                TextureBound textureBound = GameEditor.Instance.Inspector.GetTextureBound(texture);
                if (textureBound == null)
                {
                    IntPtr ptr = GameEditor.Instance.GuiRenderer.BindTexture(texture);

                    textureBound = new TextureBound { IntPtr = ptr, Texture = texture };
                    GameEditor.Instance.Inspector.AddTextureBound(textureBound);

                }
                else
                {
                    ImGui.Image(textureBound.IntPtr, new System.Numerics.Vector2(40, 40));
                }

            }
        }



    }
}
