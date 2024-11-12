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
        GameObject _goToDestroy = null;

        public void SetCore(GameCoreViewer coreViewer)
        {
            _tag = coreViewer;
        }

        public override void Draw()
        {
            ImGui.Begin("Hierarchy");

            if (ImGui.Button("Add GameObject", new System.Numerics.Vector2(-1, 0)))
            {
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

            ImGui.BeginChild("gameObjectList");

            Selection.TryGameObject(out GameObject selectedGameObject);

            int i = 0;

            // if (_tag is GameCoreViewer { GameCore.GameObjects: not null })
            // {
            //     // Debug.ClearLogs();
            //     // Debug.Log($"-- { (SceneManager.Instance.CurrentScene.GameObjects).Count}");
            //     foreach (var item in (SceneManager.Instance.CurrentScene.GameObjects))
            //     {
            //         bool isPrefabInstance = item.PrefabInstance != null;
            //
            //         ImGui.PushID(i++);
            //         if (isPrefabInstance)
            //         {
            //             ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1.0f, 1.0f, 1.0f));
            //             ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.0f, 1.0f, 1.0f, 0.5f));
            //             ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.0f, 1.0f, 1.0f, 1.0f));
            //         }
            //
            //         //TODO Second way to Display to the hierarchy -> ImGui.Selectable allow to drag and drop but not to have children
            //         // if (ImGui.Selectable($"{item.Name}", selectedGameObject == item))
            //         // {
            //         //     Selection.SelectObject(item);
            //         // }
            //         
            //         if (isPrefabInstance)
            //             ImGui.PopStyleColor(3);
            //
            //         if (ImGui.BeginPopupContextItem())
            //         {
            //             if (ImGui.Button("Destroy"))
            //             {
            //                 ImGui.CloseCurrentPopup();
            //                 _goToDestroy = item;
            //             }
            //             ImGui.EndPopup();
            //         }
            //
            //         if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
            //         {
            //             unsafe
            //             {
            //                 // Set payload to carry the index of our item (could be anything)
            //                 ImGui.SetDragDropPayload("HIERARCHY", (IntPtr)(&i), sizeof(int));
            //             }
            //
            //             Clipboard.Content = item;
            //             Debug.Log($"HierarchyToolbox Dragging {item.Name}");
            //
            //             ImGui.Text(item.Name);
            //             ImGui.EndDragDropSource();
            //         }
            //
            //         ImGui.PopID();
            //     }
            // }
            ImGui.EndChild();
            ImGui.End();

            if (_goToDestroy != null)
            {
                SceneManager.Instance.CurrentScene.DestroyImmediate(_goToDestroy);
                Selection.UnSelectObject();
                _goToDestroy = null;
                Debug.Log($"-- Destroy GO, count : { (SceneManager.Instance.CurrentScene.GameObjects).Count}");
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

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Button("Create Prefab"))
                    {
                        // Logique pour créer un prefab
                        Asset prefabAsset = GameEditor.Instance.AssetManager.CreateAsset<PrefabAssetContent>(go.Name);
                        ((PrefabAssetContent)prefabAsset.AssetContent).Save(go);
                        
                        ImGui.CloseCurrentPopup();
                        Debug.Log($"Create Prefab : {prefabAsset}");
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