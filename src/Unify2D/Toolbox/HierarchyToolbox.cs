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
                GameObject go = new GameObject();
                go.Name = "GameObject";

                if (Selection.Selected != null)
                {
                    GameObject parent = Selection.Selected as GameObject;
                    parent.AddChild(go);
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
            }
        }

        void DrawNode(GameObject go)
        {
            ImGui.PushID(_currentIndex++);

            ImGuiTreeNodeFlags base_flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick |
                ImGuiTreeNodeFlags.SpanAvailWidth;

            if (Selection.Selected == go)
            {
                base_flags |= ImGuiTreeNodeFlags.Selected;
            }

            if (go.Children == null || go.Children.Count == 0)
            {
                base_flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen; // ImGuiTreeNodeFlags_Bullet

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


//bool test_drag_and_drop = true;
//base_flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;
//selection_mask = (1 << 2);
//int node_clicked = -1;
//for (int i = 0; i < 6; i++)
//{
//    // Disable the default "open on single-click behavior" + set Selected flag according to our selection.
//    // To alter selection we use IsItemClicked() && !IsItemToggledOpen(), so clicking on an arrow doesn't alter selection.
//    ImGuiTreeNodeFlags node_flags = base_flags;
//    bool is_selected = nodeSelected == i ; // (selection_mask & (1 << i)) != 0;
//    if (is_selected)
//        node_flags |= ImGuiTreeNodeFlags.Selected;

//    if (i == 3)
//    {
//        // Items 3..5 are Tree Leaves
//        // The only reason we use TreeNode at all is to allow selection of the leaf. Otherwise we can
//        // use BulletText() or advance the cursor by GetTreeNodeToLabelSpacing() and call Text().
//        node_flags |= ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_NoTreePushOnOpen; // ImGuiTreeNodeFlags_Bullet
//        ImGui::TreeNodeEx((void*)(intptr_t)i, node_flags, "Selectable Leaf %d", i);
//        if (ImGui::IsItemClicked() && !ImGui::IsItemToggledOpen())
//            node_clicked = i;
//        if (test_drag_and_drop && ImGui::BeginDragDropSource())
//        {
//            ImGui::SetDragDropPayload("_TREENODE", NULL, 0);
//            ImGui::Text("This is a drag and drop source");
//            ImGui::EndDragDropSource();
//        }
//    }
//    else
//    {
//        // Items 0..2 are Tree Node
//        bool node_open = ImGui.TreeNodeEx((IntPtr)i, node_flags, $"Selectable Node {i}");
//        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
//            nodeSelected = i;
//        if (test_drag_and_drop && ImGui.BeginDragDropSource())
//        {
//            ImGui.SetDragDropPayload("_TREENODE", (IntPtr)0, 0);
//            ImGui.Text("This is a drag and drop source");
//            ImGui.EndDragDropSource();
//        }
//        if (node_open)
//        {
//            ImGui.BulletText("Blah blah\nBlah Blah");
//            ImGui.TreePop();
//        }
//    }

//    //else
//    //{
//    //    // Items 3..5 are Tree Leaves
//    //    // The only reason we use TreeNode at all is to allow selection of the leaf. Otherwise we can
//    //    // use BulletText() or advance the cursor by GetTreeNodeToLabelSpacing() and call Text().
//    //    node_flags |= ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_NoTreePushOnOpen; // ImGuiTreeNodeFlags_Bullet
//    //    ImGui.TreeNodeEx((void*)(intptr_t)i, node_flags, "Selectable Leaf %d", i);
//    //    if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
//    //        node_clicked = i;
//    //    if (test_drag_and_drop && ImGui.BeginDragDropSource())
//    //    {
//    //        ImGui.SetDragDropPayload("_TREENODE", NULL, 0);
//    //        ImGui.Text("This is a drag and drop source");
//    //        ImGui.EndDragDropSource();
//    //    }
//    //}
//}
//if (node_clicked != -1)
//{
//    // Update selection state
//    // (process outside of tree loop to avoid visual inconsistencies during the clicking frame)
//    if (ImGui.GetIO().KeyCtrl)
//        selection_mask ^= (1 << node_clicked);          // CTRL+click to toggle
//    else //if (!(selection_mask & (1 << node_clicked))) // Depending on selection behavior you want, may want to preserve selection when clicking on item that is part of the selection
//        selection_mask = (1 << node_clicked);           // Click to single-select
//}