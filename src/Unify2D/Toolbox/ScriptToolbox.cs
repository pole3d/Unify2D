using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// Script Editor
    /// Allow the user to modify C# script directly inside the editor
    /// </summary>
    internal class ScriptToolbox : Toolbox
    {
        const string ProjectFile = "GameAssembly.csproj";

        Asset _asset;


        public void SetObject(Asset asset)
        {
            _asset = asset;
        }

        public override void Draw()
        {
            if (_asset == null)
                return;

            var scriptAsset = _asset.AssetContent as ScriptAssetContent;

            ImGui.Begin($"Script : {_asset.Name}##script");

            if (ImGui.Button("Open in VS"))
            {
                ProcessStartInfo process = new ProcessStartInfo(Tools.ToolsEditor.CombinePath(_editor.AssetsPath, ProjectFile));
                process.WorkingDirectory = _editor.AssetsPath;
                process.UseShellExecute = true;
                // process.Arguments = $"/edit {_asset.FullPath}"; // Doesn't work

                Process.Start(process );
            }
            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                scriptAsset.Save();
                _editor.Scripting.Reload();
            }

            if (scriptAsset != null)
            {
                if (scriptAsset.IsLoaded == false)
                    scriptAsset.Load();

                 System.Numerics.Vector2 size =  ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
                size.Y -= 40;

                ImGui.InputTextMultiline("##source", ref scriptAsset.Content, ushort.MaxValue, size);
         
            }

            ImGui.End();
        }

    }
}
