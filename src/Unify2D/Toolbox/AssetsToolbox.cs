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
    internal class AssetsToolbox : Toolbox
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
            _assets.Clear();
            _path = _editor.AssetsPath;

            if (String.IsNullOrEmpty(_path))
                return;

            if (Directory.Exists(_path) == false)
                Directory.CreateDirectory(_path);

            var files = Directory.GetFiles(_path);

            foreach (var file in files)
            {
                string relativeFile = file.Replace(_path, string.Empty);

                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeFile),
                    Path.GetExtension(relativeFile),
                    Path.GetDirectoryName(relativeFile)));
            }

            _selected = new bool[files.Length];
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

                    _editor.SelectObject(_assets[n]);
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
                    StringBuilder nameSb = new StringBuilder(draggedGO.Name);
                    int safeguard = 0;
                    // Append number in filename if file already exists
                    while (File.Exists(Path.Combine(_editor.AssetsPath, nameSb + ".prefab")))
                    {
                        if (++safeguard > 99999)
                            throw new Exception("Too many files with the same name, or potentially stuck in an infinite loop. Prefab save failed.");
                        
                        char lastChar = nameSb[nameSb.Length - 1];
                        if (char.IsDigit(lastChar))
                        {
                            nameSb.Length--;
                            if (lastChar == '9')
                                nameSb.Append("10");
                            else
                                nameSb.Append((char)(lastChar + 1));
                        }
                        else
                            nameSb.Append('1');
                    }
                    nameSb.Append(".prefab");
                    // Make so type name should be written in serialized data
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.TypeNameHandling = TypeNameHandling.Auto;
                    // Write serialized data to file
                    File.WriteAllText(Path.Combine(_editor.AssetsPath, nameSb.ToString()), JsonConvert.SerializeObject(draggedGO, settings));
                    // Refresh toolbox
                    Reset();
                }
                ImGui.EndDragDropTarget();
            }
            
            ImGui.End();
        }
    }
}
