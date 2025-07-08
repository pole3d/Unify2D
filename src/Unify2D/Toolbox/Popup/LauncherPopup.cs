using ImGuiNET;
using System;
using System.IO;
using Unify2D.Toolbox.Popup;
using Unify2D.Tools;
using UnifyCore;

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
            //ImGui.SeparatorText("New Project");

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
                _newProjectPath = CoreTools.CombinePath(_newProjectPath, _newProjectName);

                CreateProject();

                OnOpenProjectPathSelected(_newProjectPath);
            }


        }

        private void CreateProject()
        {
            Directory.CreateDirectory(_newProjectPath);
            Directory.CreateDirectory(Path.Combine(_newProjectPath, GameEditor.AssetsFolder));
            Directory.CreateDirectory(_newProjectPath + "\\" + GameEditor.ScenesFolder);
        }

        private void DrawExistingProject(GameEditor editor)
        {
            //ImGui.SeparatorText("Recent projects");

            ImGui.BeginChildFrame(12, new System.Numerics.Vector2(140, 80));
            if (string.IsNullOrEmpty(editor.Settings.Data.CurrentProjectPath) == false)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, ToolsUI.ToColor32(225, 70, 50, 255));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ToolsUI.ToColor32(255, 90, 60, 255));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ToolsUI.ToColor32(255, 70, 50, 255));
                if (ImGui.Button(Path.GetFileName(editor.Settings.Data.CurrentProjectPath)))
                {
                    string pathProject = Path.Combine(Directory.GetCurrentDirectory(), editor.Settings.Data.CurrentProjectPath);
                    SceneManager.Instance.CreateOrOpenSceneAtStart(pathProject, GameEditor.ScenesFolder);
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

            SceneManager.Instance.CreateOrOpenSceneAtStart(path, GameEditor.ScenesFolder);

            LoadProject();
        }

        private void OnNewProjectPathSelected(string path)
        {
            _newProjectPath = path;
            SceneManager.Instance.CreateOrOpenSceneAtStart(path, GameEditor.ScenesFolder);
        }

        void LoadProject()
        {
            _editor.ProjectLoaded();
        }


    }
}
