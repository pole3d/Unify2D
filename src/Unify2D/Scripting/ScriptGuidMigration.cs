using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Unify2D.Scripting
{
    internal static class ScriptGuidMigration
    {
        internal sealed class Result
        {
            public int Version { get; set; } = 1;
            public int ChangedFiles { get; set; }
            public List<string> Errors { get; } = new();
            public bool Success => Errors.Count == 0;
        }

        internal static Result Run(string projectPath, GuidTypeBinder binder)
        {
            projectPath = Path.GetFullPath(projectPath);
            string assets = Path.Combine(projectPath, "Assets");
            string migration = Path.Combine(projectPath, ".unify", "migrations", "script-guids-v1");
            var result = new Result();
            var changes = new List<(string Path, string Original, string Updated)>();
            // Scan even a migrated project: imported legacy prefabs may appear later.
            foreach (var path in Directory.EnumerateFiles(assets, "*", SearchOption.AllDirectories)
                .Where(path => Path.GetExtension(path).Equals(".scene", StringComparison.OrdinalIgnoreCase) ||
                               Path.GetExtension(path).Equals(".prefab", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    string original = File.ReadAllText(path);
                    using var reader = new JsonTextReader(new StringReader(original)) { DateParseHandling = DateParseHandling.None };
                    var root = JToken.Load(reader);
                    bool changed = false;
                    var tokens = root is JContainer container ? container.DescendantsAndSelf() : Enumerable.Empty<JToken>();
                    foreach (var property in tokens.OfType<JProperty>().Where(p => p.Name == "$type"))
                    {
                        string name = property.Value.Value<string>();
                        if (string.IsNullOrEmpty(name)) continue;
                        if (Guid.TryParse(name, out _))
                        {
                            binder.BindToType(null, name); // Missing scripts must not silently disappear.
                            continue;
                        }
                        int comma = name.IndexOf(',');
                        string typeName = comma < 0 ? name : name.Substring(0, comma).Trim();
                        string assembly = comma < 0 ? null : new AssemblyName(name.Substring(comma + 1).Trim()).Name;
                        if (assembly != null && assembly != "GameAssembly") continue;
                        if (binder.TryGetScriptGuid(typeName, out var guid))
                        {
                            property.Value = guid;
                            changed = true;
                        }
                        else if (assembly == "GameAssembly")
                            result.Errors.Add($"{path}: cannot resolve script '{typeName}'. Restore its original class or provide an explicit mapping.");
                    }
                    if (changed) changes.Add((path, original, root.ToString(Formatting.Indented)));
                }
                catch (Exception error) when (error is JsonException || error is IOException ||
                    error is UnauthorizedAccessException || error is ArgumentException)
                {
                    result.Errors.Add($"{path}: {error.Message}");
                }
            }

            try
            {
                Directory.CreateDirectory(migration);
                if (result.Success)
                {
                    // Preflight all references before modifying any scene or prefab.
                    foreach (var change in changes)
                    {
                        if (File.ReadAllText(change.Path) != change.Original)
                            throw new IOException($"File changed during migration: {change.Path}");
                        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(change.Original)));
                        string backup = Path.Combine(migration, Path.GetRelativePath(projectPath, change.Path)) + "." + hash + ".bak";
                        Directory.CreateDirectory(Path.GetDirectoryName(backup));
                        if (!File.Exists(backup)) File.Copy(change.Path, backup);
                        WriteAtomic(change.Path, change.Updated);
                        result.ChangedFiles++;
                    }
                    string marker = Path.Combine(projectPath, ".unify", "script-format.json");
                    if (!File.Exists(marker))
                        WriteAtomic(marker, "{\"Version\":1}");
                }
                WriteAtomic(Path.Combine(migration, "report.json"), JsonConvert.SerializeObject(result, Formatting.Indented));
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                result.Errors.Add(error.Message);
            }
            return result;
        }

        private static void WriteAtomic(string path, string content)
        {
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, content);
                File.Move(temporary, path, overwrite: true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
