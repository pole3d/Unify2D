using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unify2D.Assets;
using Unify2D.Builder;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.ImGuiRenderer;
using Unify2D.Toolbox;
using Unify2D.Toolbox.Popup;
using Unify2D.Tools;
using Num = System.Numerics;

namespace Unify2D
{
    /// <summary>
    /// 
    /// </summary>
    public class DebugLog
    {
        public static DebugLog s_SelectedLog;

        private DateTime _time;
        private string _text;
        private string _menuText;
        public DebugLog(string text)
        {
            _time = System.DateTime.Now;
            _text = text;

            _menuText = $"{_time.ToLongTimeString()} : {_text.Split('\n')[0]}";
        }

        internal virtual void Draw()
        {
            if (s_SelectedLog == this)
            {
                ImGui.BeginPopup(_menuText);
                DrawSelected();
                ImGui.CloseCurrentPopup();
            }
            else if(ImGui.MenuItem(_menuText))
            {
                s_SelectedLog = this;
            }
        }

        internal void DrawSelected()
        {
            if (ImGui.MenuItem(_time.ToLongTimeString() + " :"))
            {
                s_SelectedLog = null;
            }
            ImGui.Text(_text);
        }
    }

    public class WarningLog : DebugLog
    {
        public WarningLog(string text) : base(text)
        {
        }

        internal override void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Num.Vector4(1, 0.6f, 0, 1));

            base.Draw();

            ImGui.PopStyleColor();
        }
    }
    public class ErrorLog : DebugLog
    {
        public ErrorLog(string text) : base(text)
        {
        }

        internal override void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Num.Vector4(1, 0, 0, 1));

            base.Draw();

            ImGui.PopStyleColor();
        }
    }
}



