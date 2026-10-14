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
        readonly ScriptCompletionInput _completion = new();

        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
            editor.Scripting.SourcesChanged += RefreshSource;
        }

        public void SetObject(Asset asset)
        {
            if (_asset == asset) return;
            _asset = asset;
            _completion.Reset();
            _hasUnsavedChanges = false;
            RefreshSource();
        }

        private void RefreshSource()
        {
            if (!_hasUnsavedChanges && _asset?.AssetContent is ScriptAssetContent script && File.Exists(script.Path))
            {
                string previous = script.Content;
                script.Load();
                if (script.Content != previous) _completion.Reset();
            }
        }

        public override void Draw()
        {
            if (_asset == null)
                return;

            var scriptAsset = _asset.AssetContent as ScriptAssetContent;

            if (!ImGui.Begin($"Script : {_asset.Name}###script", ImGuiWindowFlags.NoFocusOnAppearing))
            {
                ImGui.End();
                return;
            }

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
                Save(scriptAsset);
            }

            if (scriptAsset != null)
            {
                if (scriptAsset.IsLoaded == false)
                    scriptAsset.Load();

                var size = ImGui.GetContentRegionAvail();
                if (_completion.Draw(ref scriptAsset.Content, size, Path.GetFullPath(scriptAsset.Path),
                    _editor.Scripting.GetSourceSnapshot))
                    _hasUnsavedChanges = true;
                if (_completion.SaveRequested) Save(scriptAsset);
         
            }

            ImGui.End();
        }

        private void Save(ScriptAssetContent script)
        {
            script.Save();
            _hasUnsavedChanges = false;
            _editor.Scripting.RequestReload();
        }

    }
}
