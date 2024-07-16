using ImGuiNET;
using System;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// The <see cref="HierarchyToolbox"/> class,
    /// is a specialized toolbox designed to provide a user interface to visualize and interact with game objects
    /// within a scene.
    /// </summary>
    internal class HierarchyToolbox : Toolbox
    {
        public override void Draw()
        {
            ImGui.Begin("Hierarchy");
 

            if (ImGui.Button("Add GameObject", new System.Numerics.Vector2(-  1,0)))
            {
                GameObject go = new GameObject();
                go.Name = "GameObject";
            }

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
                _goToDestroy = null;
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
