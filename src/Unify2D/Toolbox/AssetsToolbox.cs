using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using ImGuiNET;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    internal class AssetsToolbox : ToolboxBase
    {
        string _path;
        bool[] _selected;
        List<Asset> _assets = new List<Asset>();

        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            Reset();
        }

        internal override void Reset()
        {
            _editor.AssetManager.RefreshDatabase();
            _assets = _editor.AssetManager.Assets;
            _selected = new bool[_editor.AssetManager.NbOfFiles];
        }

        public override void Draw()
        {
            ImGui.Begin("Assets");
            ImGui.BeginChild("assetList");

            for (int n = 0; n < _assets.Count; n++)
            {
                if (ImGui.Selectable(_assets[n].ToString(), _selected[n]))
                {
                    // Clear selection when CTRL is not held
                    if (!ImGui.GetIO().KeyCtrl)
                    {
                        for (int i = 0; i < _assets.Count; i++)
                        {
                            _selected[i] = false;
                        }
                    }

                    Selection.SelectObject(_assets[n]);
                    _selected[n] = !_selected[n];
                }

                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
                {
                    unsafe
                    {
                        // Set payload to carry the index of our item (could be anything)
                        ImGui.SetDragDropPayload("ASSET", (IntPtr)(&n), sizeof(int));
                    }

                    Clipboard.DragContent = _assets[n];

                    ImGui.Text(_assets[n].ToString());

                    ImGui.EndDragDropSource();
                }
                
                if (ImGui.BeginPopupContextItem())
                {
                    if (_assets[n].AssetContent is PrefabAssetContent prefabContent)
                    {
                        if (ImGui.Button("Open Prefab"))
                        {
                            GameEditor.Instance.OpenPrefab(prefabContent);
                        }
                    }
                    if (ImGui.Button("Delete"))
                    {
                        DeleteSelectedAssets();
                    }
                    ImGui.EndPopup();
                }
            }
            
            ImGui.EndChild();
            
            if (ImGui.BeginDragDropTarget())
            {
                GameObject draggedGO = null;
                
                unsafe
                {
                    var ptr = ImGui.AcceptDragDropPayload("HIERARCHY");
                    if (ptr.NativePtr != null)
                        draggedGO = Clipboard.DragContent as GameObject;
                }

                if (draggedGO != null)
                {
                    // Write serialized data to file
                    Asset prefabAsset = _editor.AssetManager.CreateAsset<PrefabAssetContent>(draggedGO.Name);
                    ((PrefabAssetContent)prefabAsset.AssetContent).Save(draggedGO);
                    // Refresh toolbox
                    Reset();
                }
                ImGui.EndDragDropTarget();
            }
            
            ImGui.End();
        }
        
        private void DeleteSelectedAssets()
        {
            for (int n = 0; n < _assets.Count; n++)
            {
                if (_selected[n])
                {
                    File.Delete(GameEditor.Instance.AssetsPath + _assets[n].FullPath);
                }
            }
            Reset();
        }
    }
}
