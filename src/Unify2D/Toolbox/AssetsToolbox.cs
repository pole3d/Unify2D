using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Unify2D.Assets;
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

        internal override void Reset()
        {
            _assets.Clear();
            _path = _editor.AssetsPath;

            if (String.IsNullOrEmpty(_path))
                return;

            if (Directory.Exists(_path) == false)
                Directory.CreateDirectory(_path);

            var files = Directory.GetFiles(_path);

            foreach (var file in files)
            {
                string relativeFile = file.Replace(_path, string.Empty);

                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(relativeFile),
                    Path.GetExtension(relativeFile), Path.GetDirectoryName(relativeFile)));
            }

            _selected = new bool[files.Length];
        }


        public override void Draw()
        {
            ImGui.Begin("Assets");

            if (ImGui.Button("Show Explorer", new System.Numerics.Vector2(-1, 0)))
            {
                string path = $"{GameEditor.Instance.AssetsPath + Path.DirectorySeparatorChar}";

                if ( Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);

                System.Diagnostics.Process.Start("explorer.exe", path );
            }

            for (int n = 0; n < _assets.Count; n++)
            {
                if (ImGui.Selectable(_assets[n].ToString(), _selected[n]))
                {
                    // Clear selection when CTRL is not held
                    if (!ImGui.GetIO().KeyCtrl)
                    {
                        for (int i = 0; i < _assets.Count; i++)
                        {
                            _selected[i] = false;
                        }
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

                    Clipboard.DragContent = _assets[n];

                    ImGui.Text(_assets[n].ToString());

                    ImGui.EndDragDropSource();
                }
            }
            ImGui.End();
        }
    }
}
