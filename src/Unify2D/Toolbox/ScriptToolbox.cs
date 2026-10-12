using ImGuiNET;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Unify2D.Assets;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// Script Editor
    /// Allow the user to modify C# script directly inside the editor
    /// </summary>
    internal class ScriptToolbox : Toolbox
    {
        const string ProjectFile = "GameAssembly.csproj";
        const string TemplateProjectDirectory = @"..\..\..\GameAssemblyProject";

        string TemplateProjectPathFull => CoreTools.CombinePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), TemplateProjectDirectory);

        Asset _asset;
        bool _hasUnsavedChanges;

        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            editor.Scripting.SourcesChanged += RefreshSource;
        }

        public void SetObject(Asset asset)
        {
            if (_asset == asset) return;
            _asset = asset;
            _hasUnsavedChanges = false;
            RefreshSource();
        }

        private void RefreshSource()
        {
            if (!_hasUnsavedChanges && _asset?.AssetContent is ScriptAssetContent script && File.Exists(script.Path))
                script.Load();
        }

        public override void Draw()
        {
            if (_asset == null)
                return;

            var scriptAsset = _asset.AssetContent as ScriptAssetContent;

            ImGui.Begin($"Script : {_asset.Name}###script", ImGuiWindowFlags.NoFocusOnAppearing);

            if (ImGui.Button("Open in VS"))
            {
                // Resolve before setting the shell's working directory to avoid repeating a relative project path.
                string projectDirectory = Path.GetFullPath(_editor.ProjectPath);

                foreach (var file in Directory.GetFiles(TemplateProjectPathFull))
                {
                    string fileName = Path.GetFileName(file);
                    string newPath = Path.Combine(projectDirectory, fileName);
                    File.Copy(file, newPath, true);
                }

                ProcessStartInfo process = new ProcessStartInfo(Path.Combine(projectDirectory, ProjectFile));
                process.WorkingDirectory = projectDirectory;
                process.UseShellExecute = true;
                // process.Arguments = $"/edit {_asset.FullPath}"; // Doesn't work

                Process.Start(process);
            }
            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                scriptAsset.Save();
                _hasUnsavedChanges = false;
                _editor.Scripting.RequestReload();
            }

            if (scriptAsset != null)
            {
                if (scriptAsset.IsLoaded == false)
                    scriptAsset.Load();

                 System.Numerics.Vector2 size =  ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
                size.Y -= 40;

                if (ImGui.InputTextMultiline("##source", ref scriptAsset.Content, ushort.MaxValue, size, ImGuiInputTextFlags.AllowTabInput))
                    _hasUnsavedChanges = true;
         
            }

            ImGui.End();
        }

    }
}
