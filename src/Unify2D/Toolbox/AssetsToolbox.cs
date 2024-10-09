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

            _extensionsToIgnore = new HashSet<string>{ ".csproj", ".dll" , ".sln" };

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

                if ( _extensionsToIgnore.Contains(extension) )
                    continue;

                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeFile),
                    Path.GetExtension(relativeFile), Path.GetDirectoryName(relativeFile)));
            }
            
            string[] directories = Directory.GetDirectories(_path);

            foreach (string directory in directories)
            {
                string relativeDirectory = directory.Replace(_path, string.Empty);
                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeDirectory), Path.GetDirectoryName(relativeDirectory)));
            }

            _selected = new bool[files.Length + directories.Length];
        }


        public override void Draw()
        {
            ImGui.Begin("Assets");

            if (ImGui.Button("Show Explorer", new System.Numerics.Vector2(-1, 0)))
            {
                ShowExplorer();
            }

            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.Button("Create New Script"))
                {
                    CreateScript();
                }

                if (ImGui.Button("Create New Folder"))
                {
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
                
                if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
                {
                    unsafe
                    {
                        // Set payload to carry the index of our item (could be anything)
                        ImGui.SetDragDropPayload("ASSET", (IntPtr)(&n), sizeof(int));
                    }

                    Clipboard.Content = _assets[n];

                    ImGui.Text(_assets[n].ToString());
                    ImGui.EndDragDropSource();
                }
            }

            ImGui.End();
        }

        private void CreateScript()
        {
            string newFile = "newScript.cs";

            using (StreamWriter sw = File.CreateText(Path.Combine(_path, newFile)))
            {
                string defaultScript = "using Unify2D.Core;\r\nusing Input = Microsoft.Xna.Framework.Input;\r\n\r\nnamespace Game\r\n{\r\n    class NewScript : Component\r\n    {\r\n        public override void Update(GameCore game)\r\n        {\r\n\r\n        }\r\n    }\r\n}";
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

        private static void ShowExplorer()
        {
            string path = GameEditor.Instance.AssetsPath + Path.DirectorySeparatorChar;

            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    }
}
