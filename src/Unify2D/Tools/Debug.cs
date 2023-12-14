using Genbox.VelcroPhysics.Collision.Filtering;
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
        private static Dictionary<string, List<DebugLog>> _logs = new();

        internal static List<DebugLog> GetLogs(string category)
        {
            return _logs[category];
        }
        internal static IEnumerable<string> GetCategories()
        {
            return _logs.Keys;
        }


        public static void Log(string text)
        {
            Debug.Log(text, "All");
        }
        public static void Log(string text, string category)
        {
            if(! _logs.TryGetValue(category, out List<DebugLog> logs))
            {
                logs = new List<DebugLog>();
                _logs.Add(category, logs);
            }
            logs.Add(new DebugLog(text));
        }
        public static void LogWarning(string text)
        {
            Debug.LogWarning(text, "All");
        }
        public static void LogWarning(string text, string category)
        {
            if (!_logs.TryGetValue(category, out List<DebugLog> logs))
            {
                logs = new List<DebugLog>();
                _logs.Add(category, logs);
            }
            logs.Add(new WarningLog(text));
        }
        public static void LogError(string text)
        {
            Debug.LogError(text, "All");
        }
        public static void LogError(string text, string category)
        {
            if (!_logs.TryGetValue(category, out List<DebugLog> logs))
            {
                logs = new List<DebugLog>();
                _logs.Add(category, logs);
            }
            logs.Add(new ErrorLog(text));
        }
        public static void Assert(bool condition, string text)
        {
            if(condition) Debug.LogError(text, "All");
        }
        public static void Assert(bool condition, string text, string category)
        {
            if (condition)
            {
                if (!_logs.TryGetValue(category, out List<DebugLog> logs))
                {
                    logs = new List<DebugLog>();
                    _logs.Add(category, logs);
                }
                logs.Add(new ErrorLog(text));
            }
        }
    }
}



