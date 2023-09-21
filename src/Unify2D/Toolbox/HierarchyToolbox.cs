using ImGuiNET;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : Toolbox
    {

        bool[] _hierarchy = new bool[100];


        public override void Show()
        {
            ImGui.Begin("Hierarchy");

            GameObject goToDestroy = null;

            int i = 0;
            foreach (var item in GameCore.Current.GameObjects)
            {
                ImGui.PushID(i++);
                if (ImGui.Selectable($"{item.Name}", _hierarchy[i]))
                {      

                    for (int j = 0; j < _hierarchy.Length; j++)
                    {
                        _hierarchy[j] = false;
                    }

                    _hierarchy[i] = true;
                    _editor.SelectObject(item);
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
                _editor.UnSelectObject();
            }

        }
    }
}
