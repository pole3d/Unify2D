using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private List<Asset> _selectedAssets = new List<Asset>();
        private List<Asset> _assets = new List<Asset>();
        private HashSet<string> _extensionsToIgnore = new HashSet<string> { ".csproj", ".dll", ".sln" };
        
        private const string OpenPrefabButtonLabel = "Open Prefab";
        private const string InstantiateAsGameObjectButtonLabel = "Instantiate as GameObject";
        private const string DeleteButtonLabel = "Delete";
        private const string ShowInExplorerButtonLabel = "Show in explorer";
        private const string ShowExplorerButtonLabel = "Show explorer";
        private const string RenameButtonLabel = "Rename";
        private const string ApplyRenameButtonLabel = "Apply";
        private const string CreateNewScriptButtonLabel = "Create New Script";
        private const string CreateNewFolderButtonLabel = "Create New Folder";
        private const string AssetDragDropPayloadType = "ASSET";

        private FileSystemWatcher _watcher;
        
        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            SetWatcher();
            Reset();
        }


        /// <summary>
        /// Set the watcher to the assets directory.
        /// </summary>
        private void SetWatcher()
        {
            if (_watcher != null)
            {
                _watcher.Renamed -= OnRenamed;
                _watcher.Deleted -= OnChanged;
            }
            
            string path = Path.GetFullPath(_editor.AssetsPath);
            _watcher = new FileSystemWatcher(path);

            _watcher.NotifyFilter = NotifyFilters.Attributes
                                    | NotifyFilters.CreationTime
                                    | NotifyFilters.DirectoryName
                                    | NotifyFilters.FileName
                                    | NotifyFilters.LastAccess
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.Security
                                    | NotifyFilters.Size;
            
            _watcher.Renamed += OnRenamed;
            _watcher.Deleted += OnChanged;
            _watcher.Created += OnChanged;

            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Get the asset from a given path.
        /// </summary>
        /// <param name="path"> The asset's full path. </param>
        /// <param name="assetFromPath"> Out the asset if found.</param>
        /// <returns></returns>
        public bool TryGetAssetFromPath(string path, out Asset assetFromPath)
        {
            assetFromPath = null;
            
            foreach (Asset asset in _assets)
            {
                if (path == asset.FullPath)
                {
                    assetFromPath = asset;
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Resets the assets list by reloading all assets from the assets directory.
        /// </summary>
        internal override void Reset()
        {
            _assets.Clear();
            _selectedAssets.Clear();
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
        }

        /// <summary>
        /// Create an asset from a directory and add it to the assets list.
        /// </summary>
        /// <param name="directory"> The directory path.</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Create an asset from a file and add it to the assets list.
        /// </summary>
        /// <param name="file"> The file path.</param>
        /// <returns></returns>
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
        
        
        /// <summary>
        /// Called when a file or directory as been renamed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"> The file that is being renamed.</param>
        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (TryGetAssetFromPath($"\\{e.OldName}", out Asset asset))
            {
                string lastFragment = Path.GetFileNameWithoutExtension(e.FullPath);
                asset.SetName(lastFragment);
            }
        }
        
        /// <summary>
        /// Called when a file or directory as been created or deleted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The file that is being changed.</param>
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Reset();
        }

        public override void Draw()
        {
            ImGui.Begin("Assets");

            if (ImGui.Button(ShowExplorerButtonLabel, new System.Numerics.Vector2(-1, 0)))
            {
                ShowExplorer(string.Empty);
            }

            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.Button(CreateNewScriptButtonLabel))
                {
                    ImGui.CloseCurrentPopup();
                    CreateScript();
                }

                if (ImGui.Button(CreateNewFolderButtonLabel))
                {
                    ImGui.CloseCurrentPopup();
                    CreateFolder();
                }

                ImGui.EndPopup();
            }

            DrawAssetTree();

            ImGui.End();
        }

        /// <summary>
        /// Draw only the file with no parent, so we can draw the first layer of the tree
        /// </summary>
        private void DrawAssetTree()
        {
            IEnumerable<Asset> rootAssets = _assets.Where(asset => asset.Parent == null).ToList();

            foreach (Asset rootAsset in rootAssets)
                DrawNode(rootAsset);
        }

        /// <summary>
        /// Draw the asset node and its children recursively.
        /// </summary>
        /// <param name="node"> Node asset.</param>
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
            
            // Draw visual indicator for selected nodes
            if (_selectedAssets.Contains(node))
            {
                DrawSelectionIndicator();
            }
        }
        
        private void DrawSelectionIndicator()
        {
            var drawList = ImGui.GetWindowDrawList();
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            drawList.AddRect(min, max, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(1, 1, 0, 1)), 0, ImDrawFlags.None, 2.0f);
        }

        /// <summary>
        /// Set the node's behavior.
        /// </summary>
        /// <param name="node"></param>
        private void SetNode(Asset node)
        {
            if (ImGui.IsItemClicked())
            {
                // Clear selection when CTRL is not held
                if (!ImGui.GetIO().KeyCtrl)
                {
                    _selectedAssets.Clear();
                }
                SelectAsset(node);
            }

            HandleBeginDragDropSource(node);
            HandleBeginDragDropTarget(node);
            HandleBeginPopupContext(node);
        }

        private string _newFileName = "";
        private bool _canRefreshName = true;

        /// <summary>
        /// Handle right click on an asset.
        /// </summary>
        /// <param name="asset"></param>
        private void HandleBeginPopupContext(Asset asset)
        {
            if (!ImGui.BeginPopupContextItem())
                return;
            
            // Select the asset if it's not already selected
            if (!_selectedAssets.Contains(asset))
            {
                if (!ImGui.GetIO().KeyCtrl)
                {
                    _selectedAssets.Clear();
                }
                SelectAsset(asset);
            }
            
            // Check if all selected assets are prefabs
            bool allArePrefabs = _selectedAssets.All(a => a.AssetContent is PrefabAssetContent);

            if (allArePrefabs)
            {
                if (ImGui.Button(InstantiateAsGameObjectButtonLabel))
                {
                    foreach (var selectedAsset in _selectedAssets)
                    {
                        var prefabContent = selectedAsset.AssetContent as PrefabAssetContent;
                        prefabContent.Load();
                        SceneManager.Instance.CurrentScene.AddRootGameObject(prefabContent.InstantiatedGameObject);
                    }
                    ImGui.CloseCurrentPopup();
                    
                    Reset();
                }
            }

            if (_selectedAssets.Count == 1)
            {
                if (ImGui.Button(OpenPrefabButtonLabel))
                {
                    var prefabContent = asset.AssetContent as PrefabAssetContent;
                    GameEditor.Instance.OpenPrefab(prefabContent);

                    if (prefabContent.IsLoaded == false)
                        prefabContent.Load();

                    SceneManager.Instance.CurrentScene.AddRootGameObject(prefabContent.InstantiatedGameObject);

                    ImGui.CloseCurrentPopup();
                }
                
                string renamePopup = "RenamePopup"; 
            
                if (ImGui.Button(RenameButtonLabel))
                {
                    ImGui.OpenPopup(renamePopup);
                }
                if (ImGui.BeginPopup(renamePopup))
                {
                    ImGui.Text("Edit name:");

                    if (_canRefreshName)
                    {
                        _newFileName = asset.Name;
                        _canRefreshName = false;
                    }
                        
                    ImGui.InputText("##edit", ref _newFileName, 40);

                    if (ImGui.Button(ApplyRenameButtonLabel))
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
            }
            
            if (ImGui.Button(ShowInExplorerButtonLabel))
            {
                ShowExplorer(asset.Path);
                ImGui.CloseCurrentPopup();
            }
            
            if (ImGui.Button(DeleteButtonLabel))
            {
                DeleteSelectedAssets();
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup(); 
        }

        /// <summary>
        /// Asset being dragged.
        /// </summary>
        /// <param name="asset"></param>
        private unsafe void HandleBeginDragDropSource(Asset asset)
        {
            if (!ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
                return;

            int index = _assets.FindIndex(a => a == asset);
            ImGui.SetDragDropPayload(AssetDragDropPayloadType, (IntPtr)(&index), sizeof(int));

            Clipboard.Content = asset;

            ImGui.Text(asset.ToString());
            ImGui.EndDragDropSource();
        }

        /// <summary>
        /// Asset receiving a dragged asset.
        /// </summary>
        /// <param name="asset"></param>
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
                    Asset movingAsset = _assets[sourceIndex];
                    string oldPath = ToolsEditor.CombinePath(_path, movingAsset.FullPath);
                    string combinePath = ToolsEditor.CombinePath(_path, asset.FullPath);
                    string newPath = ToolsEditor.CombinePath(combinePath, movingAsset.Name) + movingAsset.Extension;

                    if (Path.Exists(newPath))
                        return;

                    if (movingAsset.IsDirectory)
                        Directory.Move(oldPath, newPath);
                    else
                        File.Move(oldPath, newPath);

                    movingAsset.SetPath(asset.FullPath);
                }
            }

            ImGui.EndDragDropTarget();
        }

        /// <summary>
        /// Create a script with .cs extension.
        /// </summary>
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

            // CreateAssetFromFile(newFile);
        }
        
        /// <summary>
        /// Create a folder.
        /// </summary>
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

            // CreateAssetFromDirectory(newFolderPath);
        }

        private void SelectAsset(Asset asset)
        {
            Selection.SelectObject(asset);
            _selectedAssets.Add(asset);
        }

        private void DeleteSelectedAssets()
        {
            for (int n = 0; n < _selectedAssets.Count; n++)
                DeleteAsset(_selectedAssets[n].FullPath);
        }
        
        /// <summary>
        /// Delete an asset from the assets directory.
        /// </summary>
        /// <param name="fullPath"></param>
        private void DeleteAsset(string fullPath)
        {
            string path = ToolsEditor.CombinePath(_path, fullPath);
            
            if (TryGetAssetFromPath(fullPath, out Asset asset))
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
        }

        /// <summary>
        /// Open a path in the explorer.
        /// </summary>
        /// <param name="path"></param>
        private static void ShowExplorer(string path)
        {
            string fullPath = GameEditor.Instance.AssetsPath + path;

            if (Directory.Exists(fullPath) == false)
                Directory.CreateDirectory(fullPath);

            System.Diagnostics.Process.Start("explorer.exe", fullPath);
        }
    }
}