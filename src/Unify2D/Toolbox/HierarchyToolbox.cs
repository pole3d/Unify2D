using ImGuiNET;
using System;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : Toolbox
    {
        public override void Draw()
        {
            ImGui.Begin("Hierarchy");

            GameObject goToDestroy = null;

            int i = 0;

            Selection.TryGameObject(out GameObject selectedGameObject);

            foreach (var item in GameCore.Current.GameObjects)
            {
                
                ImGui.PushID(i++);
                if (ImGui.Selectable($"{item.Name}", selectedGameObject == item))
                {      
                    Selection.SelectObject(item);
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Button("Destroy"))
                    {
                        ImGui.CloseCurrentPopup();

                        goToDestroy = item;
                    }

                    ImGui.EndPopup();
                }
                ImGui.PopID();

            }


            ImGui.End();

            if (goToDestroy != null)
            {
                GameCore.Current.DestroyImmediate(goToDestroy);
                Selection.UnSelectObject();
            }

        }
    }
}
