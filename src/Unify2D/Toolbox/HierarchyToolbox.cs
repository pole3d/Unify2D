using ImGuiNET;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : Toolbox
    {

        bool[] _hierarchy = new bool[100];
        GameEditor _editor;

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
        }


        public override void Show()
        {
            ImGui.Begin("Hierarchy");

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
                    _editor.SelectGameObject(item);
                }
                ImGui.PopID();

            }





            ImGui.End();
        }
    }
}
