using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Unify2D.Assets;
using Unify2D.Core;
using Unify2D.Tools;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// Script Editor
    /// Allow the user to modify C# script directly inside the editor
    /// </summary>
    internal class ScriptToolbox : Toolbox
    {
        const string ProjectFile = "GameAssembly.csproj";
        const string TemplateProjectDirectory = "GameAssemblyProject";

        string TemplateProjectPathFull => ToolsEditor.CombinePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), TemplateProjectDirectory);

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
                foreach (var file in Directory.GetFiles(TemplateProjectPathFull))
                {
                    string fileName = Path.GetFileName(file);
                    string newPath = ToolsEditor.CombinePath(_editor.AssetsPath, fileName);
                    File.Copy(file, newPath, true);
                }


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
