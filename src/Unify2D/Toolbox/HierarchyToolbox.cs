using ImGuiNET;
using System;
using System.Numerics;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.Tools;
using UnifyCore;


namespace Unify2D.Toolbox
{
    internal class HierarchyToolbox : Toolbox
    {
        private int _countGO = 0;
        //  private int _currentIndex = 0;
        private GameObject _goToDestroy = null;
        private bool _isAnyWidgetHovered = false;

        private const string CreatePrefabButtonLabel = "Create Prefab";
        private const string DestroyButtonLabel = "Destroy";
        private const string AddGameObjectButtonLabel = "Add GameObject";
        private const string HierarchyWindowLabel = "Hierarchy";
        private const string GameObjectNamePrefix = "GameObject_";
        private const string GameObjectListChildLabel = "gameObjectList";


        public void SetCore(GameCoreViewer coreViewer)
        {
            _tag = coreViewer;
        }

        public override void Draw()
        {


            Vector2 size = ImGui.GetContentRegionAvail();

            ImGui.Begin(HierarchyWindowLabel);

            _isAnyWidgetHovered = false;

            if (ImGui.Button(AddGameObjectButtonLabel, new System.Numerics.Vector2(-1, 0)))
            {
                if (ImGui.IsItemHovered())
                    _isAnyWidgetHovered = true;

                if (Selection.Selected != null)
                {
                    GameObject parent = Selection.Selected as GameObject;
                    GameObject go = GameObject.CreateChild(parent);
                    go.Name = $"{GameObjectNamePrefix}{_countGO++}";
                }
                else
                {
                    GameObject go = GameObject.Create();
                    go.Name = $"{GameObjectNamePrefix}{_countGO++}";
                }
            }


            // Commented because waiting resolve operation 
            if (_tag is GameCoreViewer coreViewer && coreViewer.AssetType == GameCoreViewer.Type.Prefab)
            {
                if (ImGui.Button("Close prefab", new Vector2(ImGui.GetWindowWidth(), 20.0f)))
                {
                    GameEditor.Instance.CloseGameCore(coreViewer);
                }

                ImGui.Separator();
            }



            ImGui.BeginGroup();



            // First way to Display GameObjects to the hierarchy -> Don't allow to D&D
            if (SceneManager.Instance.CurrentScene != null)
            {
                foreach (GameObject gameObject in SceneManager.Instance.CurrentScene.GameObjects)
                {
                    if (gameObject.Parent != null)
                        continue;

                    DrawNode(gameObject);
                }
            }

            // Empty node to deselect when you click on it
            var base_flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Leaf |
                             ImGuiTreeNodeFlags.NoTreePushOnOpen;
            ImGui.TreeNodeEx($"     ", base_flags);

            ImGui.EndGroup();

            Vector2 bb_min = ImGui.GetItemRectMin();
            Vector2 bb_max = ImGui.GetItemRectMax();
            Vector2 saved = ImGui.GetCursorScreenPos();

            ImGui.SetCursorScreenPos(bb_min);
            ImGui.InvisibleButton("##drop_zone_node", bb_max - bb_min);
            GameToolbox.TryDragAndDrop(_editor);
            ImGui.SetCursorScreenPos(saved);

            if (_isAnyWidgetHovered == false && ImGui.IsMouseReleased(ImGuiMouseButton.Left)
                                             && ImGui.IsWindowFocused())
            {
                Selection.UnSelectObject();
            }

            ImGui.BeginChild(GameObjectListChildLabel);

            Selection.TryGameObject(out GameObject selectedGameObject);

            ImGui.End();

            if (_goToDestroy != null && SceneManager.Instance.CurrentScene != null)
            {
                SceneManager.Instance.CurrentScene.DestroyImmediate(_goToDestroy);
                Selection.UnSelectObject();
                _goToDestroy = null;
            }
        }


        private void DrawNode(GameObject go)
        {
            ImGui.PushID((int)go.UID);

            ImGuiTreeNodeFlags base_flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                            ImGuiTreeNodeFlags.SpanAvailWidth;

            if (go.Children == null || go.Children.Count == 0)
            {
                base_flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Leaf |
                             ImGuiTreeNodeFlags.NoTreePushOnOpen;

                if (Selection.Selected == go)
                {
                    base_flags |= ImGuiTreeNodeFlags.Selected;
                }

                // Add cyan color if prefab
                if (go.Tag is PrefabAssetContent)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 1.0f, 1.0f)); // Cyan color for prefabs
                    ImGui.TreeNodeEx($"{go.Name}##{go.GetHashCode()}", base_flags);
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.TreeNodeEx($"{go.Name}##{go.GetHashCode()}", base_flags);

                }

                if (ImGui.IsItemFocused())
                {
                    if (ImGui.IsKeyPressed(ImGuiKey.Space))
                    {
                        Console.WriteLine("test");
                    }
                }
                if (ImGui.IsItemClicked())
                {
                    Selection.SelectObject(go);
                }

                if (ImGui.IsItemHovered())
                    _isAnyWidgetHovered = true;

                ShowContextMenu(go);
            }
            else
            {
                if (Selection.Selected == go)
                {
                    base_flags |= ImGuiTreeNodeFlags.Selected;
                }

                bool open;
                if (go.Tag is PrefabAssetContent)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 1.0f, 1.0f)); // Cyan color for prefabs
                    open = ImGui.TreeNodeEx($"{go.Name}##{go.GetHashCode()}", base_flags);
                    ImGui.PopStyleColor();
                }
                else
                {
                    open = ImGui.TreeNodeEx($"{go.Name}##{go.GetHashCode()}", base_flags);
                }

                if (ImGui.IsItemClicked())
                {
                    Selection.SelectObject(go);
                }
                if (ImGui.IsItemFocused())
                {
                    if (ImGui.IsKeyPressed(ImGuiKey.Space))
                    {
                        Console.WriteLine("test");
                    }
                }

                if (ImGui.IsItemHovered())
                    _isAnyWidgetHovered = true;

                ShowContextMenu(go);

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

        private void ShowContextMenu(GameObject go)
        {
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Button(CreatePrefabButtonLabel))
                {
                    // Prefab creation Logic
                    Asset prefabAsset = GameEditor.Instance.EditorAssetManager.CreateAsset<PrefabAssetContent>(go.Name);
                    ((PrefabAssetContent)prefabAsset.AssetContent).Save(go);

                    ImGui.CloseCurrentPopup();
                }

                if (ImGui.Button(DestroyButtonLabel))
                {
                    ImGui.CloseCurrentPopup();
                    _goToDestroy = go;
                }

                ImGui.EndPopup();
            }
        }


    }
}