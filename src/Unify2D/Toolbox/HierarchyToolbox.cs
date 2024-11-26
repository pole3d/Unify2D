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
        private int _countGO = 0;
        private int _currentIndex = 0;
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
            ImGui.BeginChild(GameObjectListChildLabel);

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

                ShowContextMenu(go);
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
        
        void ShowContextMenu(GameObject go)
        {
            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Button(CreatePrefabButtonLabel))
                {
                    // Prefab creation Logic
                    Asset prefabAsset = GameEditor.Instance.AssetManager.CreateAsset<PrefabAssetContent>(go.Name);
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