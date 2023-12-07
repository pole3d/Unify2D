using System;
using System.Numerics;
using ImGuiNET;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : Toolbox
    {

        bool[] _hierarchy = new bool[100];
        
        public void SetCore(GameCoreInfo coreInfo)
        {
            _tag = coreInfo;
        }
        
        public override void Draw()
        {
            ImGui.Begin("Hierarchy");

            GameObject goToDestroy = null;

            if (_tag is GameCoreInfo coreInfo && coreInfo.AssetType == GameCoreInfo.Type.Prefab) {
                if (ImGui.Button("Close prefab", new Vector2(ImGui.GetWindowWidth(), 20.0f))) {
                    GameEditor.Instance.CloseGameCore(coreInfo);
                }
                ImGui.Separator();
            }

            ImGui.BeginChild("gameObjectList");

            int i = 0;
            foreach (var item in ((GameCoreInfo)_tag).GameCore.GameObjects)
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
                
                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
                {
                    unsafe
                    {
                        // Set payload to carry the index of our item (could be anything)
                        ImGui.SetDragDropPayload("HIERARCHY", (IntPtr)(&i), sizeof(int));
                    }
                    
                    Clipboard.DragContent = item;
                    ImGui.Text(item.Name);
                        
                    ImGui.EndDragDropSource();
                }
                ImGui.PopID();
            }

            ImGui.EndChild();

            ImGui.End();

            if (goToDestroy != null)
            {
                GameCore.Current.DestroyImmediate(goToDestroy);
                _editor.UnSelectObject();
            }

        }
    }
}
