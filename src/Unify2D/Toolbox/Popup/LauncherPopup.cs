using ImGuiNET;
using System;
using System.IO;
using Unify2D.Toolbox.Popup;

namespace Unify2D.Toolbox
{
    internal class LauncherPopup : PopupBase
    {
        public override string Name => "Launcher";

        GameEditor _editor;
        string _newProjectPath = String.Empty;
        string _newProjectName = "NEW_PROJECT";

        protected override void DrawInternal(GameEditor editor)
        {
            _editor = editor;

            DrawExistingProject(editor);
            ImGui.NewLine();
            DrawNewProject(editor);

        }

        private void DrawNewProject(GameEditor editor)
        {
            ImGui.SeparatorText("New Project");

            ImGui.InputText("Name", ref _newProjectName, 30);

            if (ImGui.Button("..."))
            {
                FilePickerPopup filePicker = new FilePickerPopup();
                filePicker.OnPathSelected += OnNewProjectPathSelected;

                editor.ShowPopup(filePicker);
            }
            ImGui.SameLine();

            ImGui.InputText("Path", ref _newProjectPath, 70);

            if (ImGui.Button("Create"))
            {
                _newProjectPath = Path.Combine(_newProjectPath, _newProjectName);

                Directory.CreateDirectory(_newProjectPath);

                OnOpenProjectPathSelected(_newProjectPath);
            }


        }

        private void DrawExistingProject(GameEditor editor)
        {
            ImGui.SeparatorText("Recent projects");

            ImGui.BeginChildFrame(12, new System.Numerics.Vector2(140, 80));
            if (string.IsNullOrEmpty(editor.Settings.Data.CurrentProjectPath) == false)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, ToolsUI.ToColor32(225, 70, 50, 255));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ToolsUI.ToColor32(255, 90, 60, 255));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ToolsUI.ToColor32(255, 70, 50, 255));
                if (ImGui.Button(Path.GetFileName(editor.Settings.Data.CurrentProjectPath)))
                {
                    LoadProject();
                }
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
            }
            ImGui.EndChildFrame();


            ImGui.NewLine();


            if (ImGui.Button("Open from disk"))
            {
                FilePickerPopup filePicker = new FilePickerPopup();
                filePicker.OnPathSelected += OnOpenProjectPathSelected;

                editor.ShowPopup(filePicker);
            }

            ImGui.NewLine();

        }



        private void OnOpenProjectPathSelected(string path)
        {
            _editor.Settings.Data.CurrentProjectPath = path;
            LoadProject();
        }

        private void OnNewProjectPathSelected(string path)
        {
            _newProjectPath = path;
        }

        void LoadProject()
        {
            _editor.LoadScene();

            _editor.HidePopup();
        }


    }
}
