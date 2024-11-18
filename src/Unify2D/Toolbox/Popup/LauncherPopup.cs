using ImGuiNET;
using System;
using System.IO;
using Unify2D.Toolbox.Popup;
using Unify2D.Tools;
using UnifyCore.Scripting;

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
                _newProjectPath = ToolsEditor.CombinePath(_newProjectPath, _newProjectName);

                CreateProject();

                OnOpenProjectPathSelected(_newProjectPath);
            }


        }

        private void CreateProject()
        {
            Directory.CreateDirectory(_newProjectPath);
            Directory.CreateDirectory(Path.Combine(_newProjectPath, GameEditor.AssetsFolder));

            using StreamWriter stream = File.CreateText(Path.Combine(_newProjectPath, "CMakeLists.txt"));
            stream.WriteLine("""
                             cmake_minimum_required(VERSION 3.21)
                             
                             set(CMAKE_GENERATOR "Visual Studio 16 2019 Win64")
                             
                             project(GameAssembly CSharp)
                             
                             add_library(GameAssembly SHARED
                             )
                             
                             set_property(TARGET GameAssembly PROPERTY DOTNET_TARGET_FRAMEWORK_VERSION "v4.8")
                             set_property(TARGET GameAssembly PROPERTY WIN32_EXECUTABLE FALSE)
                             set_property(TARGET GameAssembly PROPERTY VS_CONFIGURATION_TYPE DynamicLibrary)
                             set_property(TARGET GameAssembly PROPERTY VS_DOTNET_REFERENCES
                                 "Microsoft.CSharp"
                                 "PresentationCore"
                                 "PresentationFramework"
                                 "System"
                                 "System.Core"
                                 "System.Data"
                                 "System.Data.DataSetExtensions"
                                 "System.Windows.Forms"
                                 "System.Net.Http"
                                 "System.Xaml"
                                 "System.Xml"
                                 "System.Xml.Linq"
                                 "WindowsBase"
                             )
                             
                             """);
            
            string corePath = Path.Combine(Directory.GetCurrentDirectory(), "UnifyCore.dll");
            string fnaPath = Path.Combine(Directory.GetCurrentDirectory(), "FNA.dll");
            stream.WriteLine($"set_target_properties(GameAssembly PROPERTIES VS_DOTNET_REFERENCE_UnifyCore \"{corePath.Replace('\\', '/')}\")");
            stream.WriteLine($"set_target_properties(GameAssembly PROPERTIES VS_DOTNET_REFERENCE_FNA \"{fnaPath.Replace('\\', '/')}\")");
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
            //_editor.SceneEditorManager.LoadScene();
            
            Scripting.Instance.LoadDll($@"{GameEditor.Instance.ProjectPath}\build\Debug\GameAssembly.dll");

            _editor.HidePopup();
        }


    }
}
