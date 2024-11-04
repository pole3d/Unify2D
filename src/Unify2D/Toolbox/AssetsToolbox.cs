using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// The <see cref="AssetsToolbox"/> class,
    /// is a specialized toolbox designed to provide a user interface to visualise and select <see cref="Asset">s.
    /// </summary>
    internal class AssetsToolbox : Toolbox
    {
        string _path;
        bool[] _selected;
        List<Asset> _assets = new List<Asset>();
        HashSet<string> _extensionsToIgnore;

        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);

            _extensionsToIgnore = new HashSet<string> { ".csproj", ".dll", ".sln" };

            Reset();
        }

        public Asset GetAssetFromPath(string path)
        {
            foreach (var asset in _assets)
            {
                if (path == asset.FullPath)
                    return asset;
            }

            return null;
        }

        internal override void Reset()
        {
            _assets.Clear();
            _path = _editor.AssetsPath;

            if (String.IsNullOrEmpty(_path))
                return;

            if (Directory.Exists(_path) == false)
                Directory.CreateDirectory(_path);

            string[] files = Directory.GetFiles(_path);

            foreach (string file in files)
            {
                string relativeFile = file.Replace(_path, string.Empty);
                string extension = Path.GetExtension(relativeFile);

                if (_extensionsToIgnore.Contains(extension))
                    continue;

                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeFile),
                    Path.GetExtension(relativeFile), Path.GetDirectoryName(relativeFile)));
            }

            string[] directories = Directory.GetDirectories(_path);

            foreach (string directory in directories)
            {
                string relativeDirectory = directory.Replace(_path, string.Empty);
                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeDirectory),
                    Path.GetDirectoryName(relativeDirectory)));
            }

            _selected = new bool[files.Length + directories.Length];
        }

        public override void Draw()
        {
            ImGui.Begin("Assets");

            if (ImGui.Button("Show Explorer", new System.Numerics.Vector2(-1, 0)))
            {
                ShowExplorer(string.Empty);
            }

            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.Button("Create New Script"))
                {
                    ImGui.CloseCurrentPopup();
                    CreateScript();
                }

                if (ImGui.Button("Create New Folder"))
                {
                    ImGui.CloseCurrentPopup();
                    CreateFolder();
                }

                ImGui.EndPopup();
            }

            for (int n = 0; n < _assets.Count; n++)
            {
                if (ImGui.Selectable(_assets[n].ToString(), _selected[n]))
                {
                    // Clear selection when CTRL is not held
                    if (!ImGui.GetIO().KeyCtrl)
                    {
                        for (int i = 0; i < _assets.Count; i++)
                            _selected[i] = false;
                    }

                    Selection.SelectObject(_assets[n]);
                    _selected[n] = !_selected[n];
                }

                HandBeginDragDropSource(n);
                HandleBeginDragDropTarget(n);
                HandleBeginPopupContext(n);
            }

            ImGui.End();
        }

        private void HandleBeginPopupContext(int assetIndex)
        {
            if (!ImGui.BeginPopupContextItem()) 
                return;
            
            if (ImGui.Button("Delete"))
            {
                DeleteAsset($"{_path}{_assets[assetIndex].FullPath}");
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Button("Show in explorer"))
            {
                ShowExplorer(string.Empty);
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        private unsafe void HandBeginDragDropSource(int assetIndex)
        {
            if (!ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) 
                return;
            
            // Set payload to carry the index of our item (could be anything)
            ImGui.SetDragDropPayload("ASSET", (IntPtr)(&assetIndex), sizeof(int));

            Clipboard.Content = _assets[assetIndex];

            ImGui.Text(_assets[assetIndex].ToString());
            ImGui.EndDragDropSource();
        }

        private unsafe void HandleBeginDragDropTarget(int assetIndex)
        {
            if (!ImGui.BeginDragDropTarget()) 
                return;
            
            if (String.IsNullOrEmpty(_assets[assetIndex].Extension))
            {
                ImGuiDragDropFlags dropTargetFlags = ImGuiDragDropFlags.AcceptBeforeDelivery |
                                                     ImGuiDragDropFlags.AcceptNoPreviewTooltip;
                ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("ASSET", dropTargetFlags);

                if (payload.NativePtr != (void*)IntPtr.Zero)
                {
                    if (payload.Delivery)
                    {
                        int sourceIndex = *(int*)payload.Data;
                        string oldPath = $"{_path}{_assets[sourceIndex].FullPath}";
                        string newPath = $"{_path}{_assets[assetIndex].FullPath}{_assets[sourceIndex].FullPath}";

                        if (Path.Exists(newPath))
                            return; 
                        
                        if(String.IsNullOrEmpty(_assets[sourceIndex].Extension))
                            Directory.Move(oldPath, newPath);
                        else
                            File.Move(oldPath, newPath);
                        
                        _assets[sourceIndex].SetPath(_path + _assets[assetIndex].FullPath);

                        Reset();
                    }
                }

                ImGui.EndDragDropTarget();
            }
        }

        private void CreateScript()
        {
            string newFile = "NewScript.cs";
            string newScriptPath = Path.Combine(_path, newFile);
            int counter = 1;

            while (File.Exists(newScriptPath))
            {
                newFile = $"NewScript{counter}.cs";
                newScriptPath = Path.Combine(_path, newFile);
                counter++;
            }
            
            string className = newFile.Replace(".cs", "");
            using (StreamWriter sw = File.CreateText(newScriptPath))
            {
                string defaultScript = $"using Unify2D.Core;\r\nusing Input = Microsoft.Xna.Framework.Input;\r\n\r\nnamespace Game\r\n{{\r\n    class {className} : Component\r\n    {{\r\n        public override void Update(GameCore game)\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
                sw.WriteLine(defaultScript);
            }

            Reset();
        }

        private void CreateFolder()
        {
            string folderName = "New Folder";
            string newFolderPath = Path.Combine(_path, folderName);
            int counter = 1;

            while (Directory.Exists(newFolderPath))
            {
                newFolderPath = Path.Combine(_path, $"New Folder ({counter})");
                counter++;
            }

            Directory.CreateDirectory(newFolderPath);

            Reset();
        }

        private void DeleteAsset(string path)
        {
            if (Path.Exists(path))
            {
                if (string.IsNullOrEmpty(Path.GetExtension(path)))
                    Directory.Delete(path);
                else
                    File.Delete(path);
            }

            Reset();
        }

        private static void ShowExplorer(string path)
        {
            string fullPath = GameEditor.Instance.AssetsPath + Path.DirectorySeparatorChar + path;

            if (Directory.Exists(fullPath) == false)
                Directory.CreateDirectory(fullPath);

            System.Diagnostics.Process.Start("explorer.exe", fullPath);
        }
    }
}