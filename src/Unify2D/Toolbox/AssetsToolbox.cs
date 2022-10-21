using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Assets;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    internal class AssetsToolbox : Toolbox
    {
        const string PathAssets = "./Assets";


        string _path;
        bool[] _selected;
        List<Asset> _assets = new List<Asset>();
        GameEditor _editor;

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
            Reset();
        }

        internal override void Reset()
        {
            _assets.Clear();
            _path = Path.Combine(_editor.ProjectPath, PathAssets);

            if (Directory.Exists(_path) == false)
                Directory.CreateDirectory(_path);

            var files = Directory.GetFiles(_path);

            foreach (var file in files)
            {
                _assets.Add(new Asset(Path.GetFileNameWithoutExtension(file),
                    Path.GetExtension(file), Path.GetDirectoryName(file)));
            }

            _selected = new bool[files.Length];
        }


        public override void Show()
        {
            ImGui.Begin("Assets");

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
    }
}
