using ImGuiNET;
using System;
using System.Numerics;
using System.Diagnostics;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Tools;


namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : Toolbox
    {
        int _countGO = 0;
        int _currentIndex = 0;
        GameObject _goToDestroy = null;
        bool _isAnyWidgetHovered = false;

        public void SetCore(GameCoreViewer coreViewer)
        {
            _tag = coreViewer;
        }

        public override void Draw()
        {
            ImGui.Begin("Hierarchy");

            _isAnyWidgetHovered = false;

            if (ImGui.Button("Add GameObject", new System.Numerics.Vector2(-1, 0)))
            {
                if (ImGui.IsItemHovered())
                    _isAnyWidgetHovered = true;

                if (Selection.Selected != null)
                {
                    GameObject parent = Selection.Selected as GameObject;
                    GameObject go = GameObject.CreateChild(parent);
                    go.Name = $"GameObject_{_countGO++}";
                    Debug.Log("Add GO - parent selected");
                }
                else
                {
                    GameObject go = GameObject.Create();
                    go.Name = $"GameObject_{_countGO++}";
                    Debug.Log("Add GO - No parent selected");
                }
            }

            // if (_tag is GameCoreViewer coreViewer && coreViewer.AssetType == GameCoreViewer.Type.Prefab)
            // {
            //     if (ImGui.Button("Close prefab", new Vector2(ImGui.GetWindowWidth(), 20.0f)))
            //     {
            //         GameEditor.Instance.CloseGameCore(coreViewer);
            //     }
            //
            //     ImGui.Separator();
            // }

            // First way to Display GameObjects to the hierarchy -> Don't allow to D&D
            foreach (GameObject gameObject in SceneManager.Instance.CurrentScene.GameObjects)
            {
                if (gameObject.Parent != null)
                    continue;
            
                DrawNode(gameObject);
            }

            if (_isAnyWidgetHovered == false && ImGui.IsMouseReleased(ImGuiMouseButton.Left) 
                && ImGui.IsWindowFocused())
            {
                Selection.UnSelectObject();
            }
            ImGui.BeginChild("gameObjectList");

            Selection.TryGameObject(out GameObject selectedGameObject);

            ImGui.End();

            if (_goToDestroy != null)
            {
                SceneManager.Instance.CurrentScene.DestroyImmediate(_goToDestroy);
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
                base_flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Leaf |
                             ImGuiTreeNodeFlags.NoTreePushOnOpen; // ImGuiTreeNodeFlags_Bullet

                if (Selection.Selected == go)
                {
                    base_flags |= ImGuiTreeNodeFlags.Selected;
                }

                ImGui.TreeNodeEx($"{go.Name}##{go.GetHashCode()}", base_flags);
                if (ImGui.IsItemClicked())
                {
                    Selection.SelectObject(go);
                }

                if (ImGui.IsItemHovered())
                    _isAnyWidgetHovered = true;

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Button("Create Prefab"))
                    {
                        // Logique pour créer un prefab
                        Asset prefabAsset = GameEditor.Instance.AssetManager.CreateAsset<PrefabAssetContent>(go.Name);
                        ((PrefabAssetContent)prefabAsset.AssetContent).Save(go);
                        
                        ImGui.CloseCurrentPopup();
                        Debug.Log($"Create Prefab: {prefabAsset}");
                    }
                    
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

                if (ImGui.IsItemHovered())
                    _isAnyWidgetHovered = true;

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