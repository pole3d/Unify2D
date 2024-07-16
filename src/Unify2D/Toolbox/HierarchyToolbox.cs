using ImGuiNET;
using System;
using System.Diagnostics;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : Toolbox
    {

        int _currentIndex = 0;
        GameObject _goToDestroy = null;

        public override void Draw()
        {
            ImGui.Begin("Hierarchy");


            if (ImGui.Button("Add GameObject", new System.Numerics.Vector2(-1, 0)))
            {
                if (Selection.Selected != null)
                {
                    GameObject parent = Selection.Selected as GameObject;
                    GameObject go = GameObject.CreateChild(parent);
                    go.Name = "GameObject";
                }
                else
                {
                    GameObject go = GameObject.Create();
                    go.Name = "GameObject";
                }
            }

            _currentIndex = 0;

            foreach (var item in GameCore.Current.GameObjects)
            {
                if (item.Parent != null)
                    continue;

                DrawNode(item);
            }



            ImGui.End();

            if (_goToDestroy != null)
            {
                GameCore.Current.DestroyImmediate(_goToDestroy);
                Selection.UnSelectObject();
                _goToDestroy = null;
            }
        }

        void DrawNode(GameObject go)
        {
            ImGui.PushID((int)go.UID);

            ImGuiTreeNodeFlags base_flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick |
                ImGuiTreeNodeFlags.SpanAvailWidth;


            if (go.Children == null || go.Children.Count == 0)
            {
                base_flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen; // ImGuiTreeNodeFlags_Bullet

                if (Selection.Selected == go)
                {
                    base_flags |= ImGuiTreeNodeFlags.Selected;
                }

                ImGui.TreeNodeEx($"{go.Name}##{go.GetHashCode()}", base_flags);
                if (ImGui.IsItemClicked())
                {
                    Selection.SelectObject(go);
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Button("Destroy"))
                    {
                        ImGui.CloseCurrentPopup();

                        _goToDestroy = go;
                    }

                    ImGui.EndPopup();
                }
            }
            else
            {
                if (Selection.Selected == go)
                {
                    base_flags |= ImGuiTreeNodeFlags.Selected;
                }

                bool open = (ImGui.TreeNodeEx($"{go.Name}##{go.GetHashCode()}", base_flags));
                if (ImGui.IsItemClicked())
                {
                    Selection.SelectObject(go);
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Button("Destroy"))
                    {
                        ImGui.CloseCurrentPopup();

                        _goToDestroy = go;
                    }

                    ImGui.EndPopup();
                }

                if (open)
                {
                    if (go.Children != null)
                    {
                        foreach (var item in go.Children)
                        {
                            DrawNode(item);
                        }
                    }
                    ImGui.TreePop();
                }
            }

            ImGui.PopID();
        }
    }
}


