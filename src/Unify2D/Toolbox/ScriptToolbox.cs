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
        GameObject _gameObject;
        Asset _asset;


        public void SetObject(Asset asset)
        {
            _asset = asset;
        }

        public override void Draw()
        {
            if (_asset == null)
                return;

            ImGui.Begin($"Script : {_asset.Name}##script");

            if (_asset.AssetContent is ScriptAssetContent scriptAsset)
            {
                if (scriptAsset.IsLoaded == false)
                    scriptAsset.Load();

                 System.Numerics.Vector2 size =  ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
                size.Y -= 20;

                ImGui.InputTextMultiline("##source", ref scriptAsset.Content, ushort.MaxValue, size);
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
