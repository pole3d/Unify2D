using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Toolbox.Popup;

namespace Unify2D.Toolbox
{
    internal class LauncherPopup : PopupBase
    {
        public override string Name => "Launcher";

        protected override void DrawInternal(GameEditor editor)
        {
            DrawExistingProject();
            ImGui.NewLine();
            DrawNewProject();

        }

        private static void DrawNewProject()
        {
            ImGui.SeparatorText("New Project");

            string name = "NEW";
            ImGui.InputText("Name", ref name, 50);
            if (ImGui.Button("Choose folder"))
            {

            }
            if (ImGui.Button("Create"))
            {

            }
        }

        private static void DrawExistingProject()
        {
            ImGui.SeparatorText("Existing projects");
            if (ImGui.Button("Open"))
            {

            }

            ImGui.NewLine();

            ImGui.Text("Recent Projects");
        }
    }
}
