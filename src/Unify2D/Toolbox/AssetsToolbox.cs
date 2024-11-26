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
        public List<Asset> Assets => _assets;
        
        private string _path;
        private bool[] _selected;
        private List<Asset> _assets = new List<Asset>();
        private HashSet<string> _extensionsToIgnore = new HashSet<string> { ".csproj", ".dll", ".sln" };
        
        private const string OpenPrefabButtonLabel = "Open Prefab";
        private const string InstantiateAsGameObjectButtonLabel = "Instantiate as GameObject";
        private const string DeleteButtonLabel = "Delete";
        private const string ShowInExplorerButtonLabel = "Show in explorer";
        private const string AssetDragDropPayloadType = "ASSET";


        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
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

        /// <summary>
        /// Resets the assets list by reloading all assets from the assets directory.
        /// </summary>
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

        private Asset CreateAssetFromDirectory(string directory)
        {
            string relativeDirectory = directory.Replace(_path, string.Empty);
            Asset newAsset = new Asset(Path.GetFileNameWithoutExtension(relativeDirectory),
                Path.GetDirectoryName(relativeDirectory), true);

            _assets.Add(newAsset);

            string[] filesInDirectory = Directory.GetFiles($"{_path}{newAsset.FullPath}");
            string[] directoriesInDirectory = Directory.GetDirectories($"{_path}{newAsset.FullPath}");

            foreach (string file in filesInDirectory)
            {
                Asset child = CreateAssetFromFile(file);
                newAsset.AddChild(child);
            }

            foreach (string dir in directoriesInDirectory)
            {
                Asset child = CreateAssetFromDirectory(dir);
                newAsset.AddChild(child);
            }

            return newAsset;
        }

        private Asset CreateAssetFromFile(string file)
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
                base_flags = ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.Leaf |
                             ImGuiTreeNodeFlags.NoTreePushOnOpen; // ImGuiTreeNodeFlags_Bullet

                if (Selection.Selected == node)
                    base_flags |= ImGuiTreeNodeFlags.Selected;

                ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}", base_flags);

                SetNode(node);
            }
            else
            {
                if (Selection.Selected == node)
                    base_flags |= ImGuiTreeNodeFlags.Selected;

                bool open = ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}", base_flags);

                SetNode(node);

                if (open)
                {
                    foreach (Asset child in node.Children)
                        DrawNode(child);

                    ImGui.TreePop();
                }
            }
        }

        private void SetNode(Asset node)
        {
            if (ImGui.IsItemClicked())
                Selection.SelectObject(node);

            HandBeginDragDropSource(node);
            HandleBeginDragDropTarget(node);
            HandleBeginPopupContext(node);
        }

        private string _newFileName = "";
        private bool _canRefreshName = true;

        private void HandleBeginPopupContext(Asset asset)
        {
            if (!ImGui.BeginPopupContextItem())
                return;

            if (ImGui.Button("Delete"))
            {
                DeleteAsset(asset);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Button(ShowInExplorerButtonLabel))
            {
                ShowExplorer(asset.Path);
                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Button("Rename"))
            {
                ImGui.OpenPopup("RenamePopup");
            }

            if (ImGui.BeginPopup("RenamePopup"))
            {
                ImGui.Text("Edit name:");

                if (_canRefreshName)
                {
                    _newFileName = asset.Name;
                    _canRefreshName = false;
                }
                    
                ImGui.InputText("##edit", ref _newFileName, 40);

                if (ImGui.Button("Apply"))
                {
                    string oldPath = asset.FullPath;
                    asset.SetName(_newFileName);
                    
                    if(asset.IsDirectory)
                        Directory.Move($"{_path}{oldPath}", $"{_path}{asset.FullPath}");
                    else
                        File.Move($"{_path}{oldPath}", $"{_path}{asset.FullPath}");
                    
                    _canRefreshName = true;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
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
            ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload(AssetDragDropPayloadType, dropTargetFlags);

            if (payload.NativePtr != (void*)IntPtr.Zero)
            {
                if (payload.Delivery)
                {
                    int sourceIndex = *(int*)payload.Data;
                    string oldPath = $"{_path}{_assets[sourceIndex].FullPath}";
                    string newPath = $"{_path}{asset.FullPath}{_assets[sourceIndex].FullPath}";

                    if (Path.Exists(newPath))
                        return;

                    if (_assets[sourceIndex].IsDirectory)
                        Directory.Move(oldPath, newPath);
                    else
                        File.Move(oldPath, newPath);

                    _assets[sourceIndex].SetPath(asset.Path);

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
                string defaultScript =
                    $"using Unify2D.Core;\r\nusing Input = Microsoft.Xna.Framework.Input;\r\n\r\nnamespace Game\r\n{{\r\n    class {className} : Component\r\n    {{\r\n        public override void Update(GameCore game)\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
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

        private void SelectAsset(Asset asset)
        {
            Selection.SelectObject(asset);
        }

        private void DeleteSelectedAssets()
        {
            for (int n = 0; n < _assets.Count; n++)
            {
                if (_selected[n])
                {
                    DeleteAsset(_assets[n]);
                }
            }
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
            string fullPath = GameEditor.Instance.AssetsPath + path;

            if (Directory.Exists(fullPath) == false)
                Directory.CreateDirectory(fullPath);

            System.Diagnostics.Process.Start("explorer.exe", fullPath);
        }
    }
}