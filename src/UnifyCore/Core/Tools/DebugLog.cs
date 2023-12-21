using ImGuiNET;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.ImGuiRenderer;
using Num = System.Numerics;

namespace Unify2D
{
    [Flags]
    public enum LogTypes
    {
        Log = 1 << 1,
        Warning = 1 << 2,
        Error = 1 << 3,
    }

    /// <summary>
    /// The <see cref="DebugLog"/> class serves as a base class for log entries,
    /// providing common functionality such as storing log text and a timestamp.
    /// This class is intended to be inherited by specialized log types that may include additional properties or behaviors.
    /// </summary>
    public class DebugLog
    {
        public static HashSet<DebugLog> s_SelectedLogs = new();

        private DateTime _time;
        private string _text;
        private string _menuText;
        public DebugLog(string text)
        {
            _time = System.DateTime.Now;
            _text = text;

            _menuText = $"{_time.ToLongTimeString()} : {_text.Split('\n')[0]}";
        }

        public virtual void Draw(LogTypes filter)
        {
            if(filter.HasFlag(LogTypes.Log)) Draw();
        }
        public virtual void Draw()
        {
            if (s_SelectedLogs.Contains(this))
            {
                ImGui.SeparatorText(_menuText);
            }
            else if (ImGui.MenuItem(_menuText))
            {
                if(! Keyboard.GetState().IsKeyDown(Keys.LeftShift)) 
                {
                    s_SelectedLogs.Clear();
                }
                s_SelectedLogs.Add(this);
            }
        }

        public virtual void DrawSelected()
        {
            ImGui.SeparatorText(_time.ToLongTimeString() + " :");
            ImGui.Text(_text);
        }
    }

    /// <summary>
    /// The <see cref="WarningLog"/> class inherits the <see cref="DebugLog"/>,
    /// it provides filtering of non-Warning logs and is colored in orange.
    /// </summary>
    public class WarningLog : DebugLog
    {
        public WarningLog(string text) : base(text)
        {
        }

        public override void Draw(LogTypes filter)
        {
            if (filter.HasFlag(LogTypes.Warning)) Draw();
        }
        public override void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Num.Vector4(1, 0.6f, 0, 1));

            base.Draw();

            ImGui.PopStyleColor();
        }
        public override void DrawSelected()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Num.Vector4(1, 0.6f, 0, 1));

            base.DrawSelected();

            ImGui.PopStyleColor();
        }
    }
    /// <summary>
    /// The <see cref="ErrorLog"/> class inherits the <see cref="DebugLog"/>,
    /// it provides filtering of non-Error logs and is colored in red.
    /// </summary>
    public class ErrorLog : DebugLog
    {
        public ErrorLog(string text) : base(text)
        {
        }

        public override void Draw(LogTypes filter)
        {
            if (filter.HasFlag(LogTypes.Error)) Draw();
        }
        public override void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Num.Vector4(1, 0, 0, 1));

            base.Draw();

            ImGui.PopStyleColor();
        }
        public override void DrawSelected()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Num.Vector4(1, 0, 0, 1));

            base.DrawSelected();

            ImGui.PopStyleColor();
        }
    }
    /// <summary>
    /// The <see cref="DebugColorLog"/> class inherits the <see cref="DebugLog"/>,
    /// it provides a field for text color.
    /// </summary>
    public class DebugColorLog : DebugLog
    {
        protected Num.Vector4 _color;

        public DebugColorLog(string text, Num.Vector4 color) : base(text)
        {
            _color = color;
        }

        public override void Draw()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, _color);

            base.Draw();

            ImGui.PopStyleColor();
        }
        public override void DrawSelected()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, _color);

            base.DrawSelected();

            ImGui.PopStyleColor();
        }
    }
}



