using ImGuiNET;
using System.IO;
using Unify2D.Toolbox.Popup;

namespace Unify2D.Toolbox
{
    internal class LauncherPopup : PopupBase
    {
        public override string Name => "Launcher";

        protected override void DrawInternal(GameEditor editor)
        {
            DrawExistingProject(editor);
            ImGui.NewLine();
           DrawNewProject(editor);

        }

        private static void DrawNewProject(GameEditor editor)
        {
            ImGui.SeparatorText("New Project");

            ImGui.BeginDisabled();
            string name = "NEW";
            ImGui.InputText("Name", ref name, 50);

            if (ImGui.Button("Choose folder"))
            {

            }
            if (ImGui.Button("Create"))
            {

            }

            ImGui.EndDisabled();

        }

        private static void DrawExistingProject(GameEditor editor)
        {
            ImGui.SeparatorText("Recent projects");

            ImGui.BeginChildFrame( 12,new System.Numerics.Vector2(140,80));
            if (string.IsNullOrEmpty(editor.Settings.Data.CurrentProjectPath) == false)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, ToolsUI.ToColor32(225, 70, 50, 255));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ToolsUI.ToColor32(255, 90, 60, 255));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ToolsUI.ToColor32(255, 70, 50, 255));
                if (ImGui.Button(  Path.GetFileName(editor.Settings.Data.CurrentProjectPath)))
                {
                    editor.LoadScene();
                    editor.HidePopup();
                }
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
                ImGui.PopStyleColor();
            }
            ImGui.EndChildFrame();


            ImGui.NewLine();
  


            if (ImGui.Button("Open from disk"))
            {
                editor.ShowPopup(new FilePickerPopup());
            }

            ImGui.NewLine();

        }
    }
}
