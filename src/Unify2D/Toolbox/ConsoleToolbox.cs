using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unify2D.Assets;
using Unify2D.Core.Graphics;
using Unify2D.Core;
using Unify2D.Tools;
using Microsoft.Xna.Framework;

using Num = System.Numerics;

namespace Unify2D.Toolbox
{
    internal class ConsoleToolbox : Toolbox
    {
        private string _logCategory = "All";

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
            foreach(string category in Debug.GetCategories())
            {
                if (ImGui.TabItemButton(category))
                {
                    _logCategory = category;
                }
            }
            ImGui.EndTabBar();


            ImGui.BeginTabItem(_logCategory);
            foreach (var log in Debug.GetLogs(_logCategory))
            {
                log.Draw();
            }
            ImGui.EndTabItem();


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
