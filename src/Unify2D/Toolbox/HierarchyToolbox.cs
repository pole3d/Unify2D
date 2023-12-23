using System;
using System.Numerics;
using ImGuiNET;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : ToolboxBase
    {
        public void SetCore(GameCoreViewer coreViewer)
        {
            _tag = coreViewer;
        }
        
        public override void Draw()
        {
            ImGui.Begin("Hierarchy");

            if (ImGui.Button("Add GameObject", new System.Numerics.Vector2(-  1,0)))
            {
                GameObject go = new GameObject();
                GameCore.Current.AddGameObjectImmediate(go);
                go.Name = "GameObject";
            }

            GameObject goToDestroy = null;

            if (_tag is GameCoreViewer coreViewer && coreViewer.AssetType == GameCoreViewer.Type.Prefab) {
                if (ImGui.Button("Close prefab", new Vector2(ImGui.GetWindowWidth(), 20.0f))) {
                    GameEditor.Instance.CloseGameCore(coreViewer);
                }
                ImGui.Separator();
            }

            ImGui.BeginChild("gameObjectList");

            Selection.TryGameObject(out GameObject selectedGameObject);
            
            int i = 0;
            foreach (var item in ((GameCoreViewer)_tag).GameCore.GameObjects)
            {
                bool isPrefabInstance = item.PrefabInstance != null;
                
                ImGui.PushID(i++);
                if (isPrefabInstance)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 1.0f, 1.0f));
                    ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.0f, 1.0f, 1.0f, 0.5f));
                    ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.0f, 1.0f, 1.0f, 1.0f));
                }
                if (ImGui.Selectable($"{item.Name}", selectedGameObject == item))
                {
                    Selection.SelectObject(item);
                }
                if (isPrefabInstance)
                    ImGui.PopStyleColor(3);

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
                Selection.UnSelectObject();
            }

        }
    }
}
