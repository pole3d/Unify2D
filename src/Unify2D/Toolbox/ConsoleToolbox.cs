using ImGuiNET;
using System.Collections.Generic;

using Num = System.Numerics;

namespace Unify2D.Toolbox
{
    /// <summary>
    /// The <see cref="ConsoleToolbox"/> class,
    /// is a specialized toolbox designed to provide a user interface for debugging and inspectiong logs.
    /// It pulls thoses logs from the <see cref="Debug"> class and draws them within the editor environment.
    /// </summary>
    internal class ConsoleToolbox : Toolbox
    {
        private HashSet<string> _logCategories = new HashSet<string>() { "All" };
        private LogTypes _logTypes = LogTypes.Log | LogTypes.Warning | LogTypes.Error;

        internal ConsoleToolbox()
        {
            /*
            Debug.Log("log 1");
            Debug.Log("veeeeeeeeeeeeeerrrrrrrrrrrryyyyyyyyyyyyy\nllllllllloooooooooooooonnnnnnnnnnnnnngggggggggggggg\nlooog");

            Debug.LogWarning("warning 1");

            Debug.LogError("error 1");

            Debug.Assert(true, "assert true");
            Debug.Assert(false, "assert false");

            for(int i = 0; i < 999; i++)
            {
                Debug.Log($"spam {i}", "spam");
            }
            */
        }

        private void ToggleLogType(LogTypes type)
        {
            if(_logTypes.HasFlag(type))
            {
                _logTypes &= ~type;
            }
            else
            {
                _logTypes |= type;
            }
        }
        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
        }
        public override void Draw()
        {
            ImGui.Begin("Console", ImGuiWindowFlags.NoScrollbar);

            Num.Vector2 avail = ImGui.GetContentRegionAvail();
            avail.Y /= 2f;

            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1);


            ImGui.BeginChild("Logs", avail, true);


            ImGui.BeginTabBar("Log Categories", ImGuiTabBarFlags.None);


            if (ImGui.TabItemButton("Clear"))
            {
                Debug.ClearLogs();
            }
            if (ImGui.TabItemButton("Filter"))
            {
                ImGui.SetWindowPos("Log Filter", ImGui.GetMousePos());
                _editor.ShowPopup(new EnumPopup<LogTypes>("Log Filter", _logTypes, ToggleLogType));
            }

            foreach (string category in Debug.GetCategories())
            {
                bool isCat = _logCategories.Contains(category);
                if (isCat)
                {
                    ImGui.PushStyleColor(ImGuiCol.Tab, new Num.Vector4(0.4f, 0.5f, 1, 1));
                }

                if (ImGui.TabItemButton(category))
                {
                    if(_logCategories.Add(category) == false)
                    {
                        _logCategories.Remove(category);
                    }
                }

                if (isCat)
                {
                    ImGui.PopStyleColor();
                }
            }
            ImGui.EndTabBar();


            foreach (var category in _logCategories)
            {
                if(_logCategories.Count > 1)
                {
                    //ImGui.SeparatorText(category);
                }
                foreach (var log in Debug.GetLogs(category))
                {
                    log.Draw(_logTypes);
                }
            }

            ImGui.EndChild();


            ImGui.BeginChild("Selected", avail, true);
            foreach (var log in DebugLog.s_SelectedLogs)
            {
                log.DrawSelected();
            }
            ImGui.EndChild();


            ImGui.PopStyleVar();
            ImGui.End();

        }
    }
}
