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
        internal ConsoleToolbox()
        {
            Debug.Log("log 1");
            Debug.Log("veeeeeeeeeeeeeerrrrrrrrrrrryyyyyyyyyyyyy\nllllllllloooooooooooooonnnnnnnnnnnnnngggggggggggggg\nlooog");

            Debug.LogWarning("warning 1");

            Debug.LogError("error 1");

            Debug.Assert(true, "assert true");
            Debug.Assert(false, "assert false");

            for(int i = 2; i < 999; i++)
            {
                Debug.Log($"log {i}");
            }
        }

        public override void Initialize(GameEditor editor)
        {
            base.Initialize(editor);
        }
        public override void Draw()
        {
            ImGui.Begin("Console", ImGuiWindowFlags.None);

            List<DebugLog> logs = Debug.GetLogs();

            foreach (var log in logs)
            {
                log.Draw();
            }

            ImGui.PopStyleVar();
            ImGui.End();

        }
    }
}
