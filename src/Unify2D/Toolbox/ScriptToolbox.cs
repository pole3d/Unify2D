using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    internal class ScriptToolbox : Toolbox
    {
        GameEditor _editor;
        GameObject _gameObject;
        Asset _asset;

        public override void Initialize(GameEditor editor)
        {
            _editor = editor;
        }

        public void SetObject(Asset asset)
        {
            _asset = asset;
        }

        public override void Show()
        {
            if (_asset == null)
                return;

            ImGui.Begin("Script");

            if (_asset.AssetContent is ScriptAssetContent scriptAsset)
            {
                if (scriptAsset.IsLoaded == false)
                    scriptAsset.Load();

                ImGui.InputTextMultiline("##source", ref scriptAsset.Content, ushort.MaxValue, new System.Numerics.Vector2(800, 550));
                if (ImGui.Button("Save"))
                {
                    scriptAsset.Save();
                    _editor.Scripting.Reload();
                }
            }

            ImGui.End();
        }

    }
}
