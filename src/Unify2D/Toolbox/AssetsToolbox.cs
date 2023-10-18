using ImGuiNET;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        GameEditor _editor;

        int treeFilesIndex = 0;

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
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

            var files = Directory.GetFiles(_path);
            var folders = Directory.GetDirectories(_path);
           

            treeFiles.Add(new TreeNode() { name = "Root", type = "Root", childType = "null", nodeIndex = treeFilesIndex, childCount = files.Length + folders.Length });
            treeFilesIndex++;
            foreach (var folder in folders)
            {
                string relativeFolder = folder.Replace(_path, string.Empty);
                string folderName = Path.GetFileName(relativeFolder);

                treeFiles.Add(new TreeNode() { name = folderName, type = "Folder", childType = "file", nodeIndex = treeFilesIndex, childCount = 2 });
                treeFilesIndex++;
            }
            foreach (var file in files)
            {
                string relativeFile = file.Replace(_path, string.Empty);

                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeFile), Path.GetExtension(relativeFile), Path.GetDirectoryName(relativeFile)));

                string fileName = Path.GetFileNameWithoutExtension(relativeFile).ToString();
                //string extension = Path.GetExtension(relativeFile).ToString();

                treeFiles.Add(new TreeNode() { name = fileName, type = "file", childType = "null", nodeIndex = treeFilesIndex, childCount = -1 });
                //treeFiles[treeFilesIndex] = new TreeNode() { name = fileName, type = extension, childType = "null", nodeIndex = treeFilesIndex, childCount = -1 };
                treeFilesIndex++;

            }

            _selected = new bool[files.Length];
        }


        public override void Show()
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

                        //TreeNode.DisplayNode(treeFiles[0], treeFiles);


                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Test"))
                {
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            if (ImGui.TreeNode("Tree View"))
            {
                ImGuiTableFlags flags = ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.NoBordersInBody;
                if (ImGui.BeginTable("3 ways", 2, flags))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.NoHide);
                    ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 18f);
                    ImGui.TableHeadersRow();

                    
                    TreeNode.DisplayNode(treeFiles[0], treeFiles);
                    ImGui.EndTable();
                }
                ImGui.TreePop();
            }



            ImGui.End();
        }
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
                treeFiles.Add(new TreeNode() { name = "New Folder " + newIndex, type = "Folder", childType = "file", nodeIndex = treeFilesIndex, childCount = 10 });
            }
        }

        public void DeleteFolder(string pathDirectory)
        {
            if (Directory.Exists(pathDirectory))
            {
                Directory.Delete(pathDirectory, true);
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

            public static void DisplayNode(TreeNode node, List<TreeNode> allNodes)
            {
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

                    if (open)
                    {
                        for (int i = 0; i < allNodes.Count; i++)
                        {
                            // display toute les node sauf le root
                            if(node.type == "Root")
                            {
                                if (allNodes[i].type != "Root")
                                {
                                    DisplayNode(allNodes[i], allNodes);
                                }
                            }


                            if (allNodes[i].type == node.childType)
                            {
                                DisplayNode(allNodes[i], allNodes);
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
                }
            }
        };
        public List<TreeNode> treeFiles = new List<TreeNode>();
    }



}
