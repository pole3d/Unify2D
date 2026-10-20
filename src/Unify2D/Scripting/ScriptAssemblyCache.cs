using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Unify2D.Scripting
{
    internal static class ScriptAssemblyCache
    {
        internal sealed class Entry
        {
            public string Fingerprint { get; set; }
            public byte[] Image { get; set; }
            public string ImageHash { get; set; }
            public Dictionary<string, string[]> TypesBySource { get; set; }
        }

        private static readonly string[] ReferencePaths =
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
        private static readonly Lazy<MetadataReference[]> CachedReferences = new(() =>
            ReferencePaths.Select(path => MetadataReference.CreateFromFile(path)).ToArray());
        internal static IEnumerable<MetadataReference> References => CachedReferences.Value;

        internal static string GetPath(string assetsPath)
        {
            string projectKey = Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(Path.GetFullPath(assetsPath).ToUpperInvariant())));
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Unify2D", "ScriptCache", projectKey + ".json");
        }

        internal static string Fingerprint(Dictionary<string, string> sources)
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            void Add(string value)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                hash.AppendData(BitConverter.GetBytes(bytes.Length));
                hash.AppendData(bytes);
            }
            Add("Unify2D-script-cache-v1");
            // Includes compiler options, implicit imports and editor code changes.
            Add(typeof(ScriptAssemblyCache).Module.ModuleVersionId.ToString());
            Add(typeof(CSharpCompilation).Module.ModuleVersionId.ToString());
            Add(Environment.Version.ToString());
            foreach (string path in ReferencePaths.OrderBy(path => path, StringComparer.Ordinal))
            {
                var file = new FileInfo(path);
                Add(path);
                Add(file.Length.ToString());
                Add(file.LastWriteTimeUtc.Ticks.ToString());
            }
            foreach (var source in sources.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                Add(source.Key);
                Add(source.Value);
            }
            return Convert.ToHexString(hash.GetHashAndReset());
        }

        internal static Entry Read(string path, string fingerprint, Dictionary<string, string> sources)
        {
            try
            {
                var entry = JsonSerializer.Deserialize<Entry>(File.ReadAllText(path));
                if (entry?.Fingerprint != fingerprint || entry.Image == null || entry.TypesBySource == null ||
                    entry.TypesBySource.Count != sources.Count ||
                    entry.TypesBySource.Any(pair => !sources.ContainsKey(pair.Key) || pair.Value == null) ||
                    entry.ImageHash != Convert.ToHexString(SHA256.HashData(entry.Image)))
                    return null;
                return entry;
            }
            catch (IOException) { return null; }
            catch (UnauthorizedAccessException) { return null; }
            catch (JsonException) { return null; }
        }

        internal static void Write(string path, Entry entry)
        {
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                entry.ImageHash = Convert.ToHexString(SHA256.HashData(entry.Image));
                File.WriteAllText(temporary, JsonSerializer.Serialize(entry));
                File.Move(temporary, path, overwrite: true);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            finally
            {
                try { File.Delete(temporary); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }
    }
}
