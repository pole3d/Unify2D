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
    public static class Debug
    {
        private static List<DebugLog> _newLogs = new List<DebugLog>();
        private static List<DebugLog> _logs = new List<DebugLog>();

        internal static List<DebugLog> GetLogs() 
        {
            _logs.AddRange(_newLogs);
            _newLogs.Clear();
            return _logs; 
        }


        public static void Log(string text)
        {
            _newLogs.Add(new DebugLog(text));
        }
        public static void LogWarning(string text)
        {
            _newLogs.Add(new WarningLog(text));
        }
        public static void LogError(string text)
        {
            _newLogs.Add(new ErrorLog(text));
        }
        public static void Assert(bool condition, string text)
        {
            if (condition) _newLogs.Add(new ErrorLog(text));
        }
    }
}



