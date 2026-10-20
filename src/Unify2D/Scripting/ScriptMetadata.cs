using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnifyCore;

namespace Unify2D.Scripting
{
    internal static class ScriptMetadata
    {
        internal static Dictionary<string, string> ReadGuids(IEnumerable<string> paths)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var owners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in paths)
            {
                string guid = AssetMetadata.GetOrCreateGuid(path);
                if (guid == GuidTypeBinder.SpriteRendererGuid)
                    throw new InvalidDataException($"Script '{path}' uses a reserved engine GUID.");
                if (owners.TryGetValue(guid, out var previous))
                    throw new InvalidDataException($"Duplicate script GUID {guid}: '{previous}' and '{path}'. Restore the correct .meta files.");
                owners[guid] = path;
                result[path] = guid;
            }
            return result;
        }

        internal static CSharpCompilation AddIdentities(CSharpCompilation compilation, Dictionary<string, string> guids)
        {
            var component = compilation.GetTypeByMetadataName("Unify2D.Core.Component");
            var attributes = new List<string>();
            foreach (var tree in compilation.SyntaxTrees)
            {
                if (!guids.TryGetValue(tree.FilePath, out var guid)) continue;
                var model = compilation.GetSemanticModel(tree);
                var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    var type = model.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
                    if (type == null || !seen.Add(type)) continue;
                    for (var parent = type.BaseType; parent != null; parent = parent.BaseType)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(parent, component)) continue;
                        string key = SymbolDisplay.FormatLiteral(GuidTypeBinder.ScriptGuidPrefix + guid, true);
                        string name = SymbolDisplay.FormatLiteral(MetadataName(type), true);
                        attributes.Add($"[assembly: global::System.Reflection.AssemblyMetadataAttribute({key}, {name})]");
                        break;
                    }
                }
            }
            return compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(string.Join("\n", attributes),
                path: "Unify2D.ScriptIdentities.g.cs"));
        }

        private static string MetadataName(INamedTypeSymbol type) =>
            type.ContainingType != null ? MetadataName(type.ContainingType) + "+" + type.MetadataName :
            type.ContainingNamespace.IsGlobalNamespace ? type.MetadataName :
            type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;
    }
}
