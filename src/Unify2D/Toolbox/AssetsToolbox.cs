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
        int windowIndex = 0;
        string newName = "";
        string[] folders;
        string selectedFolder;
        string previousFolder = null;
        string selectedFile;
        string previousFile = null;
        string selectedItem;
        
        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
            selectedFolder = _editor.AssetsPath;
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
            if (!Directory.Exists(Path.Combine(_editor.AssetsPath, folderPath)))
                return;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.NoTreePushOnOpen;


            string relativePath = Path.Combine(_editor.AssetsPath, folderPath);
            string[] files = Directory.GetFiles(relativePath);

            foreach (var file in files)
            {
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(Path.GetFileNameWithoutExtension(file));
                if (ImGui.IsItemClicked() || ImGui.IsItemHovered())
                {
                    selectedFile = file;
                    selectedItem = file;
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
                        CreateDirectory();
                    }

                    ImGui.EndMenu();
                }
                if(ImGui.Button("Duplicate Asset Windows"))
                {
                    DuplicateWindowsAsset();
                }
                ImGui.EndMenuBar();
            }

            if (ImGui.BeginTable("Tree Table", 2))
            {
                ImGui.Unindent(ImGui.GetTreeNodeToLabelSpacing());
                ImGui.TableNextColumn();
                if (ImGui.TreeNode("Root"))
                {
                    if (ImGui.IsItemHovered())
                    {
                        if(selectedItem != _editor.AssetsPath)
                        {
                            Console.WriteLine("Root is selected");
                            selectedItem = _editor.AssetsPath;
                            selectedFolder = _editor.AssetsPath;
                        }
                        
                    }
                        

                    for (int i = 0; i < folders.Length; i++)
                    {
                        if (ImGui.IsItemHovered())
                            selectedItem = folders[i];
                        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.Selected;
                        string relativeFolder = folders[i].Replace(_path, string.Empty);
                        
                        string folderName = Path.GetFileName(relativeFolder);
                        if (ImGui.TreeNodeEx(folderName, flags))
                        {
                            ImGui.TableNextRow();
                           

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

                ImGui.TableSetColumnIndex(1);
                ImGui.SetNextItemOpen(true);
                if (ImGui.TreeNode("Files"))
                {
                    ImGui.TableSetColumnIndex(1);
                    DisplayFilesTree(selectedFolder);
                }
                ImGui.EndTable();
            }
          
            if(ImGui.BeginPopupContextItem())
            {
                ImGui.Text("Popup for " + Path.GetFileNameWithoutExtension(selectedItem));
                
                if (ImGui.Button("Rename"))
                    RenameFile(newName);
                ImGui.SameLine();
                ImGui.InputText("Write new name then click on rename", ref newName, 128);


                if (ImGui.Button("Delete"))
                    DeleteItem();
                if(ImGui.Button("Close"))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
            ImGui.SetItemTooltip("Right click to open file menu");
        }

        public void DuplicateWindowsAsset()
        {
            windowIndex++;
            ImGui.Begin("new toolbox" + windowIndex, ImGuiWindowFlags.MenuBar);
            Console.WriteLine("Duplicate");

            //if (ImGui.BeginMenuBar())
            //{
            //    if (ImGui.BeginMenu("Create"))
            //    {
            //        ImGui.MenuItem("Script", null);

            //        if (ImGui.MenuItem("New Folder", null))
            //        {
            //            CreateDirectory();
            //        }

            //        ImGui.EndMenu();
            //    }
            //    if (ImGui.Button("Duplicate Asset Windows"))
            //    {

            //    }
            //    ImGui.EndMenuBar();
            //}

            //if (ImGui.BeginTable("Tree Table", 2))
            //{
            //    ImGui.Unindent(ImGui.GetTreeNodeToLabelSpacing());
            //    ImGui.TableNextColumn();
            //    if (ImGui.TreeNode("Root"))
            //    {

            //        for (int i = 0; i < folders.Length; i++)
            //        {
            //            if (ImGui.IsItemHovered())
            //                selectedItem = folders[i];
            //            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.Selected;
            //            string relativeFolder = folders[i].Replace(_path, string.Empty);

            //            string folderName = Path.GetFileName(relativeFolder);
            //            if (ImGui.TreeNodeEx(folderName, flags))
            //            {
            //                ImGui.TableNextRow();


            //                selectedFolder = folders[i];

            //                if (selectedFolder != previousFolder)
            //                {
            //                    Console.WriteLine("folder selectionné = " + Path.GetFileName(relativeFolder));
            //                    previousFolder = selectedFolder;
            //                }
            //            }
            //        }
            //        ImGui.TreePop();
            //    }

            //    ImGui.TableSetColumnIndex(1);
            //    ImGui.SetNextItemOpen(true);
            //    if (ImGui.TreeNode("Files"))
            //    {
            //        ImGui.TableSetColumnIndex(1);
            //        DisplayFilesTree(selectedFolder);
            //    }
            //    ImGui.EndTable();
            //}

            //if (ImGui.BeginPopupContextItem())
            //{
            //    ImGui.Text("Popup for " + Path.GetFileNameWithoutExtension(selectedItem));

            //    if (ImGui.Button("Rename"))
            //        RenameFile(newName);
            //    ImGui.SameLine();
            //    ImGui.InputText("Write new name then click on rename", ref newName, 128);


            //    if (ImGui.Button("Delete"))
            //        DeleteItem();
            //    if (ImGui.Button("Close"))
            //        ImGui.CloseCurrentPopup();
            //    ImGui.EndPopup();
            //}
            //ImGui.SetItemTooltip("Right click to open file menu");
        }
        public void CreateDirectory()
        {
            CheckIfFolderExist(0);
        }
        public void CheckIfFolderExist(int index)
        {
            if(Directory.Exists(Path.Combine(selectedFolder, ("New Folder" + index))))
            {
                CheckIfFolderExist(index + 1);
                Console.WriteLine("LE DOSSIER EXISTE");
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(selectedFolder, ("New Folder" + index)));
                Reset();
                Console.WriteLine("LE DOSSIER à " + index + "d'index");
            }
            
        }
        public void DeleteItem()
        {
            FileInfo item = new FileInfo(selectedItem);
            FileAttributes attr = item.Attributes;
            bool isDirectory;

            if (attr.HasFlag(FileAttributes.Directory))
            {
                isDirectory = true;
            } 
            else
            {
                isDirectory = false;
            }

            if(isDirectory) 
            {
                Console.WriteLine("Is Folder");
                if(Directory.Exists(selectedItem))
                {
                    Directory.Delete(selectedItem, true);
                    Reset();
                }
                else
                {
                    Console.WriteLine("le folder n'existe pas");
                }
            }
            if(!isDirectory)
            {
                Console.WriteLine("Is File");
                if(File.Exists(selectedItem))
                {
                    File.Delete(selectedItem);
                    Reset();
                }
                else
                {
                    Console.WriteLine("le file n'existe pas");
                }
            }
        }
        public void RenameFile(string newName)
        {
            FileInfo file = new FileInfo(selectedItem);
            DirectoryInfo directoryInfo = new DirectoryInfo(selectedItem);
            bool isDirectory;
            FileAttributes attributes = file.Attributes;
            if(attributes.HasFlag(FileAttributes.Directory))
                isDirectory = true;
            else
                isDirectory = false;

            string directory = file.DirectoryName;
            string extension = file.Extension;

            if(!isDirectory)
            {
                if (file.Exists)
                {
                    string newFile = Path.Combine(directory, newName + extension);
                    File.Move(file.ToString(), newFile);
                }
                else
                {
                    Console.Write("Le fichier n'existe pas");
                }
            }
            if(isDirectory)
            {
                if(directoryInfo.Exists)
                {
                    string newFile = Path.Combine(directory, newName + extension);
                    Directory.Move(directoryInfo.ToString(), newFile);
                    selectedFolder = newFile;
                    DisplayFilesTree(selectedFolder);
                    Reset();
                }
                else
                {
                    Console.WriteLine("Le dossier n'existe pas");
                }
            }
        }
    }
}
