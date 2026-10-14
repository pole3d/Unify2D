using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unify2D.Scripting
{
    internal static class ScriptCompilationDefaults
    {
        // A separate tree preserves source locations and completion offsets in user files.
        private static readonly SyntaxTree Imports = CSharpSyntaxTree.ParseText(
            "global using Input = global::Microsoft.Xna.Framework.Input;\n" +
            "global using Keys = global::Microsoft.Xna.Framework.Input.Keys;\n" +
            "global using Vector2 = global::Microsoft.Xna.Framework.Vector2;\n",
            path: "Unify2D.ImplicitUsings.g.cs");

        internal static IEnumerable<SyntaxTree> WithImports(IEnumerable<SyntaxTree> sources)
            => sources.Append(Imports);

        internal static SyntaxTree Parse(string source, string path = "")
        {
            var tree = CSharpSyntaxTree.ParseText(source, path: path);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            char[] normalized = null;
            foreach (var directive in root.Usings)
            {
                string name = directive.Alias?.Name.Identifier.ValueText;
                string target = directive.Name?.ToString().Replace("global::", "");
                if (!((name == "Input" && target == "Microsoft.Xna.Framework.Input") ||
                      (name == "Keys" && target == "Microsoft.Xna.Framework.Input.Keys") ||
                      (name == "Vector2" && target == "Microsoft.Xna.Framework.Vector2")))
                    continue;
                // Ignore redundant explicit aliases only in the compilation snapshot.
                // Preserve every offset and newline for diagnostics and completion.
                normalized ??= source.ToCharArray();
                for (int i = directive.Span.Start; i < directive.Span.End; i++)
                    if (normalized[i] != '\r' && normalized[i] != '\n') normalized[i] = ' ';
            }
            return normalized == null ? tree : CSharpSyntaxTree.ParseText(new string(normalized), path: path);
        }
    }
}
