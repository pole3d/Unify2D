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
        const string TemplateProjectDirectory = "GameAssemblyProject";

        string TemplateProjectPathFull => CoreTools.CombinePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), TemplateProjectDirectory);

        Asset _asset;
        FileSystemWatcher _watcher;

        public void SetObject(Asset asset)
        {
            _asset = asset;
            var scriptAsset = _asset.AssetContent as ScriptAssetContent;

            string path = Path.GetDirectoryName(scriptAsset.Path);

            if (_watcher != null)
            {
                _watcher.Changed -= OnChanged;
                _watcher.Created -= OnCreated;
                _watcher.Renamed -= OnRenamed;
            }

            _watcher = new FileSystemWatcher(path);
            _watcher.Path = path;
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            _watcher.Filter = "*.cs";
            _watcher.Changed += new FileSystemEventHandler(OnChanged);
            _watcher.Created += new FileSystemEventHandler(OnCreated);
            _watcher.Renamed += new RenamedEventHandler(OnRenamed);
            _watcher.EnableRaisingEvents = true;
        }

        private void OnRenamed(object sender, FileSystemEventArgs e)
        {
            var scriptAsset = _asset.AssetContent as ScriptAssetContent;
            scriptAsset.Load();

        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {

        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
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
                    string newPath = CoreTools.CombinePath(_editor.ProjectPath, fileName);
                    File.Copy(file, newPath, true);
                }

                ProcessStartInfo process = new ProcessStartInfo(CoreTools.CombinePath(_editor.ProjectPath, ProjectFile));
                process.WorkingDirectory = _editor.ProjectPath;
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

                ImGui.InputTextMultiline("##source", ref scriptAsset.Content, ushort.MaxValue, size, ImGuiInputTextFlags.AllowTabInput);
         
            }

            ImGui.End();
        }

    }
}
