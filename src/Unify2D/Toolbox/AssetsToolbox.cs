using ImGuiNET;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
   

        int folderCount = 0;
        int treeFilesIndex = 0;
        string newName = "";
        string[] folders;
        string selectedFolder;
        string previousFolder = null;
        string selectedFile;
        string previousFile = null;
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
            folderCount = 0;
            if (String.IsNullOrEmpty(_path))
                return;

            if (Directory.Exists(_path) == false)
                Directory.CreateDirectory(_path);

            folders = Directory.GetDirectories(_path);
            var rootFiles = Directory.GetFiles(_path);

        }
        public void DisplayFilesTree(string folderPath)
        {

            if (folderPath == null)
                return;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.NoTreePushOnOpen;


            string relativePath = Path.Combine(_editor.AssetsPath, folderPath);
            string[] files = Directory.GetFiles(relativePath);

            foreach (var file in files)
            {
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(Path.GetFileNameWithoutExtension(file));
                if (ImGui.IsItemClicked())
                {
                    Console.WriteLine("OO");
                    selectedFile = file;


                    if (selectedFile != previousFile)
                    {
                        Console.WriteLine("file selectionné = " + Path.GetFileNameWithoutExtension(file));
                        previousFile = selectedFile;
                    }
                }
            }
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
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Delete"))
                {
                    if (ImGui.MenuItem("Delete Folder", null))
                    {
                        DeleteFolder(0, null);
                    }
                    if (ImGui.MenuItem("Delete File", null))
                    {
                        DeleteFile(null);
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Rename selected file"))
                {
                    if (ImGui.MenuItem("Rename file", null))
                    {
                        RenameFile(null, newName);
                    }
                }


                ImGui.EndMenuBar();
            }

            if (ImGui.InputTextWithHint("Rename", "Enter new name here", ref newName, 128))
            {

                RenameFile(null, newName);
            }

            if (ImGui.BeginTable("Tree Table", 2))
            {
                ImGui.Unindent(ImGui.GetTreeNodeToLabelSpacing());
                ImGui.TableNextColumn();
                if (ImGui.TreeNode("Root"))
                {

                    for (int i = 0; i < folders.Length; i++)
                    {
                        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.AllowOverlap;
                        string relativeFolder = folders[i].Replace(_path, string.Empty);
                        string folderName = Path.GetFileName(relativeFolder);
                        if (ImGui.TreeNodeEx(folderName, flags))
                        {
                            ImGui.TableNextRow();
                            //ImGui.TreePop();
                            selectedFolder = folders[i];

                            if (selectedFolder != previousFolder)
                            {
                                Console.WriteLine("folder selectionné = " + Path.GetFileName(relativeFolder));
                                previousFolder = selectedFolder;
                            }
                        }
                    }
                    ImGui.TreePop();
                }
                //ImGui.TableNextColumn();
                ImGui.TableSetColumnIndex(1);
                ImGui.SetNextItemOpen(true);
                if (ImGui.TreeNode("Files"))
                {
                    ImGui.TableSetColumnIndex(1);
                    DisplayFilesTree(selectedFolder);
                }
                ImGui.EndTable();
            }
            bool j = false;

            if (ImGui.GetIO().MouseClicked[1])
            {
                j = true;
                
            }
            if(j) 
            {
                Console.WriteLine("SS");

                if (ImGui.Button("sdsd"))
                    ImGui.OpenPopup("File Popup");
                if (ImGui.BeginPopup("File Popup", ImGuiWindowFlags.MenuBar))
                {
                    if (ImGui.BeginMenuBar())
                    {
                        if (ImGui.BeginMenu("Rename"))
                        {
                            ImGui.EndMenu();
                        }
                        if (ImGui.BeginMenu("Delete"))
                        {
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenuBar();
                    }
                    ImGui.EndPopup();
                }
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
                string folderName = "New Folder" + newIndex.ToString();
                //int newNodeIndex = treeFiles.Count-1;

                //treeFiles.Add(new TreeNode() { name = "New Folder " + newIndex, type = "Folder", childType = "file", nodeIndex = newNodeIndex, childCount = 10, folderPath = _editor.AssetsPath, path = Path.Combine(_editor.AssetsPath, folderName) });
            }
        }

        public void DeleteFolder(int index, string folderName)
        {
            //if (treeFiles[index].type != "Folder")
            //{
            //    Console.WriteLine("La node n'est pas un folder");
            //    return;
            //}

            Console.Write("La node est un folder");
            string folderPath = Path.Combine(_editor.AssetsPath, folderName);
            //treeFiles.RemoveAt(index);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                Reset();
            }

        }
        public void RenameFile(string pathFile, string newName)
        {
            FileInfo currentFile = new FileInfo(_editor.AssetsPath + pathFile);
            string newFile = Path.Combine(_editor.AssetsPath + newName);
            if (currentFile.Exists)
            {
                string newPath = Path.Combine(_editor.AssetsPath, newName);
                Console.WriteLine("Rename");
                File.Move(currentFile.ToString(), newPath);
                //treeFiles.RemoveAt(GetSelectedNode().nodeIndex);
                //treeFiles.Add(new TreeNode() { name = newName, type = "Folder", childType = "file", nodeIndex = treeFilesIndex, childCount = 10, folderPath = _editor.AssetsPath, path = Path.Combine(_editor.AssetsPath) });
                treeFilesIndex++;
            }
            else
            {
                Console.WriteLine("Le fichier n'existe pas");
            }

        }
        public void DeleteFile(string pathFile)
        {
            string combinedPath = _editor.AssetsPath + pathFile;
            //treeFiles.RemoveAt(GetSelectedNode().nodeIndex);
            if (File.Exists(combinedPath))
            {
                File.Delete(combinedPath);
            }
            else
            {
                Console.WriteLine(combinedPath + " doesnt exist");
            }

        }

    }
}
