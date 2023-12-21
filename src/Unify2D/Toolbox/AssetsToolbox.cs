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
   
        int windowIndex = 0;
        string newName = "";
        string newPath = "";
        string[] folders;
        string selectedFolder;
        string previousFolder = null;
        string selectedFile;
        string previousFile = null;
        string selectedItem;
        
        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
            newPath = _editor.AssetsPath;
            selectedFolder = _editor.AssetsPath;
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

            folders = Directory.GetDirectories(_path);
            var rootFiles = Directory.GetFiles(_path);

        }
        /// <summary>
        ///  Show files in selected folder
        /// </summary>
        /// <param name="folderPath"></param>
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
        public void AssetsWindowMenu()
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
                if (ImGui.Button("Duplicate Asset Windows"))
                {
                    DuplicateWindowsAsset();
                }
                ImGui.EndMenuBar();
            }
        }
        public override void Show()
        {
            // assets menu options
            AssetsWindowMenu();

            // show folder files
            if (ImGui.BeginTable("Tree Table", 2))
            {
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
                        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.Selected | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth;
                        string relativeFolder = folders[i].Replace(_path, string.Empty);
                        
                        string folderName = Path.GetFileName(relativeFolder);
                        if (ImGui.TreeNodeEx(folderName, flags))
                        {
                            selectedFolder = folders[i];
                            
                            if (selectedFolder != previousFolder)
                            {   
                                Console.WriteLine("folder selectionné = " + Path.GetFileName(relativeFolder));
                                previousFolder = selectedFolder;
                            }
                        }

                        if (folders[i] != selectedFolder)
                        {
                            ImGui.SetNextItemOpen(false);
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
            ImGui.SetItemTooltip("Right click to open file menu");
            PopUpItem();
        }
        public void PopUpItem()
        {
            // popup menu when right click on item
            if (ImGui.BeginPopupContextItem())
            {
                ImGui.Text("Popup for " + Path.GetFileNameWithoutExtension(selectedItem));

                if (ImGui.Button("Rename"))
                    RenameItem(newName);
                ImGui.SameLine();
                ImGui.InputText("Write new name then click on rename", ref newName, 128);

                if (ImGui.Button("Move To"))
                    MoveItem(newPath);
                ImGui.SameLine();
                for (int i = 0; i < folders.Length; i++)
                {
                    if (ImGui.Selectable(folders[i]))
                    {
                        Console.WriteLine(newPath);
                        newPath = folders[i];
                        ImGui.Text(Path.GetFileNameWithoutExtension(folders[i]));
                        if (ImGui.IsItemHovered())
                        {


                        }
                    }
                }

                if (ImGui.Button("Delete"))
                    DeleteItem();
                if (ImGui.Button("Close"))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
        }
        /// <summary>
        /// duplicate asset toolbox wip
        /// </summary>
        public void DuplicateWindowsAsset()
        {
            windowIndex++;
            ImGui.Begin("new toolbox" + windowIndex, ImGuiWindowFlags.MenuBar);
            Console.WriteLine("Duplicate");
        }

        /// <summary>
        ///     Create new directory
        /// </summary>
        public void CreateDirectory()
        {
            CheckIfFolderExist(0);
        }

        /// <summary>
        /// Check if new directory doesnt exist, if exist add suffixe number
        /// </summary>
        /// <param name="index"></param>
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

        /// <summary>
        ///     Delete selected item (folder or file)
        /// </summary>
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
        /// <summary>
        /// Move item to newPath
        /// </summary>
        /// <param name="newPath"></param>
        public void MoveItem(string newPath)
        {
            FileInfo file = new FileInfo(selectedItem);
            DirectoryInfo directoryInfo = new DirectoryInfo(selectedItem);
            bool isDirectory;
            FileAttributes attributes = file.Attributes;
            if (attributes.HasFlag(FileAttributes.Directory))
                isDirectory = true;
            else
                isDirectory = false;

            string directory = file.DirectoryName;
            string extension = file.Extension;

            if (!isDirectory)
            {
                if (file.Exists)
                {
                    File.Move(file.ToString(), newPath);
                }
                else
                {
                    Console.Write("Le fichier n'existe pas");
                }
            }
            if (isDirectory)
            {
                if (directoryInfo.Exists)
                {
                    string newFile = Path.Combine(directory, newPath + extension);
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

        /// <summary>
        /// Rename selected item (folder or file)
        /// </summary>
        /// <param name="newName"></param>
        public void RenameItem(string newName)
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
