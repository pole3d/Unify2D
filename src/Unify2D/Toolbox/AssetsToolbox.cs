using ImGuiNET;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Assets;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    internal class AssetsToolbox : Toolbox
    {
        private const int V = 1;
        string _path;
        bool[] _selected;
        List<Asset> _assets = new List<Asset>();

        public static TreeNode selectedNode;

        int treeFilesIndex = 0;
        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            Reset();
        }

        internal override void Reset()
        {
            _assets.Clear();
            _path = _editor.AssetsPath;
            treeFilesIndex = 0;
            if (String.IsNullOrEmpty(_path))
                return;

            if (Directory.Exists(_path) == false)
                Directory.CreateDirectory(_path);

            var folders = Directory.GetDirectories(_path);
            var rootFiles = Directory.GetFiles(_path);

            // root
            treeFiles.Add(new TreeNode() { name = "Root", type = "Root", childType = "null", nodeIndex = treeFilesIndex, childCount = rootFiles.Length + folders.Length, path = _editor.AssetsPath, isSelected = false }) ;
            treeFilesIndex++;

            // files in root
            foreach (var file in rootFiles)
            {
                string relativeFile = file.Replace(_path, string.Empty);

                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeFile), Path.GetExtension(relativeFile), Path.GetDirectoryName(relativeFile)));

                string fileName = Path.GetFileNameWithoutExtension(relativeFile).ToString();

                treeFiles.Add(new TreeNode() { name = fileName, type = "file", childType = "null", nodeIndex = treeFilesIndex, childCount = -1, folderPath = _editor.AssetsPath, path = Path.Combine(_editor.AssetsPath, Path.GetDirectoryName(relativeFile), relativeFile), isSelected = false }); ;
                treeFilesIndex++;

            }

            // folder in root
            foreach (var folder in folders)
            {
                string relativeFolder = folder.Replace(_path, string.Empty);
                string folderName = Path.GetFileName(relativeFolder);

                treeFiles.Add(new TreeNode() { name = folderName, type = "Folder", childType = "file", nodeIndex = treeFilesIndex, childCount = 2, folderPath = _editor.AssetsPath, path = Path.Combine(_editor.AssetsPath, folderName), isSelected = false });
                treeFilesIndex++;

                var files = Directory.GetFiles(Path.Combine(_editor.AssetsPath, folderName));

                // files in each folders
                foreach (var file in files)
                {
                    string relativeFile = file.Replace(Path.Combine(_editor.AssetsPath, folderName), string.Empty);

                    _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeFile), Path.GetExtension(relativeFile), Path.GetDirectoryName(relativeFile)));

                    string fileName = Path.GetFileNameWithoutExtension(relativeFile).ToString();
                    string extension = Path.GetExtension(relativeFile).ToString();

                    treeFiles.Add(new TreeNode() { name = fileName, type = "file", childType = "null", nodeIndex = treeFilesIndex, childCount = -1, folderPath = Path.Combine(_editor.AssetsPath, folderName), path = Path.Combine(_editor.AssetsPath, Path.GetDirectoryName(relativeFile), relativeFile), isSelected = false }); ;

                    treeFilesIndex++;
                }
            }

            //_selected = new bool[files.Length];
        }


        public override void Draw()
        {
            ImGui.Begin("Assets", ImGuiWindowFlags.MenuBar);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Create"))
                {
                    ImGui.MenuItem("Script", null);

                    if (ImGui.MenuItem("New Folder", null))
                    {
                        CreateDirectoryTreeNode();
                    }

                    //Appeared on merge and causes compilation errors, please remove if outdated   -Thomas
                    /*
                    Selection.SelectObject(_assets[n]);
                    _selected[n] = !_selected[n];
                    */
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Delete"))
                {
                    if (ImGui.MenuItem("Delete Folder", null))
                    {
                        DeleteFolder(GetSelectedNode().nodeIndex, GetSelectedNode().path);
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (ImGui.TreeNode("Tree View"))
            {
                ImGuiTableFlags flags = ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.Reorderable;
                if (ImGui.BeginTable("3 ways", 3, flags))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.NoHide);
                    ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 18f);
                    ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed, 18f);
                    ImGui.TableHeadersRow();

                    
                    TreeNode.DisplayNode(treeFiles[0], treeFiles);
                    ImGui.EndTable();
                }
                ImGui.TreePop();
            }



            ImGui.End();
        }

        public static void SetSelectedNode(TreeNode node)
        {
            selectedNode = node;
        }
        public static TreeNode GetSelectedNode() 
        { return selectedNode; }
        public void CreateDirectoryTreeNode()
        {
            Console.WriteLine(_editor.AssetsPath);
            string currentPath = Directory.GetCurrentDirectory();
            Console.WriteLine(currentPath);
            CheckNewFolder(0);
        }

        
        public void CheckNewFolder(int newIndex)
        {
            if (Directory.Exists(Path.Combine(_editor.AssetsPath, "New Folder" + newIndex)))
            {
                CheckNewFolder(newIndex + 1);
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(_editor.AssetsPath, "New Folder" + newIndex));
                string folderName = "New Folder" + newIndex.ToString();
                treeFiles.Add(new TreeNode() { name = "New Folder " + newIndex, type = "Folder", childType = "file", nodeIndex = treeFilesIndex, childCount = 10, folderPath = _editor.AssetsPath, path = Path.Combine(_editor.AssetsPath, folderName) });
            }
        }

        public void DeleteFolder(int index, string folderName)
        {
            if (treeFiles[index].type != "Folder")
            {
                Console.WriteLine("La node n'est pas un folder");
                return;
            }

            Console.Write("La node est un folder");
            string folderPath = Path.Combine(_editor.AssetsPath, folderName);
            treeFiles.RemoveAt(index);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            
        }

        public void DeleteFile(string pathFile)
        {
            if(File.Exists(pathFile))
            {
                File.Delete(pathFile);
            }
           
        }
        

        public struct TreeNode
        {
            public string name;
            public string type;
            public string childType;
            public int nodeIndex;
            public int childCount;
            public string folderPath;
            public string path;
            public bool isSelected;
            

            public static void DisplayNode(TreeNode node, List<TreeNode> allNodes)
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    Console.Write(ImGui.IsItemClicked());
                    TreeNode selectedNode = allNodes[node.nodeIndex -1];
                    selectedNode.isSelected = !selectedNode.isSelected;
                  
                    if (selectedNode.isSelected)
                    {
                        Console.WriteLine(selectedNode.name + " is selected, her index is "+ selectedNode.nodeIndex);
                        SetSelectedNode(selectedNode);
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
               
  
                bool isFolder = false;
                if (node.childCount > 0)
                {
                    isFolder = true;
                }

                if (isFolder)
                {
                    bool open = ImGui.TreeNodeEx(node.name, ImGuiTreeNodeFlags.SpanFullWidth);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(node.type);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(node.nodeIndex.ToString());

                    if (open)
                    {

                        for (int i = 0; i < allNodes.Count; i++)
                        {
                            if (node.type == "Root")
                            {
                                if (node.path == allNodes[i].folderPath)
                                {
                                    DisplayNode(allNodes[i], allNodes);
                                }
                            }
                            if (node.type == "Folder")
                            {
                                if (allNodes[i].folderPath == node.path)
                                {
                                    DisplayNode(allNodes[i], allNodes);
                                }

                            }
                        }
                        ImGui.TreePop();
                    }
                }
                else
                {
                    ImGui.TreeNodeEx(node.name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(node.type);
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(node.nodeIndex.ToString());
                }
            }
        };
        public List<TreeNode> treeFiles = new List<TreeNode>();
    }



}
