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
using Unify2D.Core;
using Unify2D.Core.Graphics;
using Unify2D.ImGuiRenderer;
using Num = System.Numerics;

namespace Unify2D
{
    /// <summary>
    /// The <see cref="Debug"/> class provides a collection of static methods
    /// for debugging purposes, including logging and other utilities.
    /// </summary>
    public static class Debug
    {
        private static Dictionary<string, List<DebugLog>> _logs = new();

        public static List<DebugLog> GetLogs(string category)
        {
            if (_logs.TryGetValue(category, out List<DebugLog> values))
            {
                return values;
            }
            else
            {
                return new List<DebugLog>(0);
            }
        }
        public static IEnumerable<string> GetCategories()
        {
            return _logs.Keys;
        }

        public static void Log(string text, string category = "All")
        {
            if(! _logs.TryGetValue(category, out List<DebugLog> logs))
            {
                logs = new List<DebugLog>();
                _logs.Add(category, logs);
            }
            logs.Add(new DebugLog(text));
        }
        public static void Log(string text, Num.Vector4 color, string category = "All")
        {
            if (!_logs.TryGetValue(category, out List<DebugLog> logs))
            {
                logs = new List<DebugLog>();
                _logs.Add(category, logs);
            }
            logs.Add(new DebugColorLog(text, color));
        }
        public static void LogWarning(string text, string category = "All")
        {
            if (!_logs.TryGetValue(category, out List<DebugLog> logs))
            {
                logs = new List<DebugLog>();
                _logs.Add(category, logs);
            }
            logs.Add(new WarningLog(text));
        }
        public static void LogError(string text, string category = "All")
        {
            if (!_logs.TryGetValue(category, out List<DebugLog> logs))
            {
                logs = new List<DebugLog>();
                _logs.Add(category, logs);
            }
            logs.Add(new ErrorLog(text));
        }
        public static void Assert(bool condition, string text, string category = "All")
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

        public static void ClearLogs()
        {
            _logs.Clear();
        }
        
        public static void Log(bool boolean)
        {
            Log(boolean.ToString());
        }
    }
}



