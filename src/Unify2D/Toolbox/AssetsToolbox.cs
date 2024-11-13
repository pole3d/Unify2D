using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                CreateAssetFromFile(file);

            string[] directories = Directory.GetDirectories(_path);

            foreach (string directory in directories)
                CreateAssetFromDirectory(directory);

            _selected = new bool[_assets.Count];
        }

        private Asset CreateAssetFromDirectory(string directory, bool isChild = false)
        {
            string relativeDirectory = directory.Replace(_path, string.Empty);
            Asset newAsset = new Asset(Path.GetFileNameWithoutExtension(relativeDirectory), Path.GetDirectoryName(relativeDirectory), true);
                
                _assets.Add(newAsset);

            string[] filesInDirectory = Directory.GetFiles($"{_path}{newAsset.FullPath}");
            string[] directoriesInDirectory = Directory.GetDirectories($"{_path}{newAsset.FullPath}");
                
            foreach (string file in filesInDirectory)
            {
                Asset child = CreateAssetFromFile(file, true);
                newAsset.AddChild(child);
            }
            
            foreach (string dir in directoriesInDirectory)
            {
                Asset child = CreateAssetFromDirectory(dir, true);
                newAsset.AddChild(child);
            }

            return newAsset;
        }

        private Asset CreateAssetFromFile(string file, bool isChild = false)
        {
            string relativeFile = file.Replace(_path, string.Empty);
            string extension = Path.GetExtension(relativeFile);

            if (_extensionsToIgnore.Contains(extension))
                return null;

            Asset newAsset = new Asset(Path.GetFileNameWithoutExtension(relativeFile),
                Path.GetExtension(relativeFile), Path.GetDirectoryName(relativeFile));
            
            _assets.Add(newAsset);

            return newAsset;
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
            
            DrawAssetTree();

            ImGui.End();
        }
        
        private void DrawAssetTree()
        {
            IEnumerable<Asset> rootAssets = _assets.Where(asset => asset.Parent == null).ToList();

            foreach (Asset rootAsset in rootAssets)
                DrawNode(rootAsset);
        }

        private void DrawNode(Asset node)
        {
            ImGuiTreeNodeFlags base_flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick |
                                            ImGuiTreeNodeFlags.SpanAvailWidth;

            if (node.Children.Count == 0)
            {
                base_flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

                if (Selection.Selected == node)
                    base_flags |= ImGuiTreeNodeFlags.Selected;

                ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}", base_flags);

                if (ImGui.IsItemClicked())
                    Selection.SelectObject(node);
            }
            else
            {
                bool open = ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}", base_flags);

                if (open)
                {
                    foreach (Asset child in node.Children)
                        DrawNode(child); 
                    
                    ImGui.TreePop();
                }
            }
            
            HandBeginDragDropSource(node);
            HandleBeginDragDropTarget(node);
            HandleBeginPopupContext(node);
        }

        private void HandleBeginPopupContext(Asset asset)
        {
            if (!ImGui.BeginPopupContextItem()) 
                return;
            
            if (ImGui.Button("Delete"))
            {
                DeleteAsset(asset);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Button("Show in explorer"))
            {
                ShowExplorer(string.Empty);
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        private unsafe void HandBeginDragDropSource(Asset asset)
        {
            if (!ImGui.BeginDragDropSource(ImGuiDragDropFlags.None)) 
                return;
            
            int index = _assets.FindIndex(a => a == asset);
            ImGui.SetDragDropPayload("ASSET", (IntPtr)(&index), sizeof(int));

            Clipboard.Content = asset;

            ImGui.Text(asset.ToString());
            ImGui.EndDragDropSource();
        }

        private unsafe void HandleBeginDragDropTarget(Asset asset)
        {
            if (!ImGui.BeginDragDropTarget()) 
                return;

            if (!asset.IsDirectory) 
                return;
            
            ImGuiDragDropFlags dropTargetFlags = ImGuiDragDropFlags.AcceptBeforeDelivery |
                                                 ImGuiDragDropFlags.AcceptNoPreviewTooltip;
            ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("ASSET", dropTargetFlags);

            if (payload.NativePtr != (void*)IntPtr.Zero)
            {
                if (payload.Delivery)
                {
                    int sourceIndex = *(int*)payload.Data;
                    string oldPath = $"{_path}{_assets[sourceIndex].FullPath}";
                    string newPath = $"{_path}{asset.FullPath}{_assets[sourceIndex].FullPath}";

                    if (Path.Exists(newPath))
                        return; 
                        
                    if(_assets[sourceIndex].IsDirectory)
                        Directory.Move(oldPath, newPath);
                    else
                        File.Move(oldPath, newPath);
                        
                    _assets[sourceIndex].SetPath(_path + asset.FullPath);
  
                    Reset();
                }
            }

            ImGui.EndDragDropTarget();
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

        private void DeleteAsset(Asset asset)
        {
            string path = $"{_path}{asset.FullPath}";
            
            if (Path.Exists(path))
            {
                if (asset.IsDirectory)
                {
                    string[] files = Directory.GetFiles(path);
                    
                    if (files.Length > 0)
                    {
                        foreach (string file in files)
                            File.Delete(file);
                    }
                    
                    Directory.Delete(path);
                }
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