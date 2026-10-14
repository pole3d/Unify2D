using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unify2D.Scripting
{
    // Works on text snapshots, never on the editor or collectible runtime assemblies.
    internal static class ScriptCompletion
    {
        internal sealed record Description(string Kind, string Type, string[] Signatures);
        internal sealed record Result(int Start, int Length, string[] Items)
        {
            internal IReadOnlyDictionary<string, Description> Details { get; init; }
                = new Dictionary<string, Description>();
        }

        private static readonly SymbolDisplayFormat SignatureFormat = SymbolDisplayFormat.MinimallyQualifiedFormat
            .WithMemberOptions(SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeContainingType |
                SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeModifiers)
            .WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName |
                SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeDefaultValue |
                SymbolDisplayParameterOptions.IncludeOptionalBrackets);

        private static string Name(ISymbol symbol) =>
            SyntaxFacts.GetKeywordKind(symbol.Name) != SyntaxKind.None ? "@" + symbol.Name : symbol.Name;

        private static Description Describe(ISymbol[] symbols)
        {
            var symbol = symbols[0];
            var type = symbol switch
            {
                IMethodSymbol method => method.ReturnType,
                IPropertySymbol property => property.Type,
                IFieldSymbol field => field.Type,
                ILocalSymbol local => local.Type,
                IParameterSymbol parameter => parameter.Type,
                IEventSymbol ev => ev.Type,
                IAliasSymbol alias => alias.Target as ITypeSymbol,
                ITypeSymbol t => t,
                _ => null
            };
            string kind = symbol switch
            {
                IMethodSymbol => "method",
                IPropertySymbol => "property",
                IFieldSymbol => "field",
                ILocalSymbol => "local",
                IParameterSymbol => "parameter",
                IEventSymbol => "event",
                IAliasSymbol => "alias",
                INamedTypeSymbol named => named.TypeKind.ToString().ToLowerInvariant(),
                INamespaceSymbol => "namespace",
                _ => symbol.Kind.ToString().ToLowerInvariant()
            };
            return new Description(kind, type?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "",
                symbols.Select(s => s.ToDisplayString(SignatureFormat)).Distinct().OrderBy(s => s, StringComparer.Ordinal).ToArray());
        }

        private static readonly Lazy<MetadataReference[]> References = new(() =>
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path)).ToArray());

        internal static Result Get(string source, int cursor, string path,
            KeyValuePair<string, string>[] otherSources, bool force = false)
        {
            cursor = Math.Clamp(cursor, 0, source.Length);
            int start = cursor, end = cursor;
            while (start > 0 && SyntaxFacts.IsIdentifierPartCharacter(source[start - 1])) start--;
            while (end < source.Length && SyntaxFacts.IsIdentifierPartCharacter(source[end])) end++;
            string prefix = source.Substring(start, cursor - start);
            var empty = new Result(start, end - start, Array.Empty<string>());
            bool afterDot = start > 0 && source[start - 1] == '.';
            if (!force && prefix.Length < 2 && !afterDot) return empty;

            // A placeholder makes incomplete member access usable by the semantic model.
            var tree = ScriptCompilationDefaults.Parse(source.Remove(start, end - start).Insert(start, "__completion"), path: path);
            var root = tree.GetRoot();
            var token = root.FindToken(start);
            if (token.SpanStart != start || !token.IsKind(SyntaxKind.IdentifierToken)) return empty;
            var trivia = root.FindTrivia(start, findInsideTrivia: true);
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || trivia.IsKind(SyntaxKind.DisabledTextTrivia))
                return empty;

            var trees = otherSources.Where(pair => !string.Equals(pair.Key, path, StringComparison.OrdinalIgnoreCase))
                .Select(pair => ScriptCompilationDefaults.Parse(pair.Value, path: pair.Key)).Append(tree);
            var compilation = CSharpCompilation.Create("ScriptCompletion", ScriptCompilationDefaults.WithImports(trees), References.Value,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var model = compilation.GetSemanticModel(tree);
            IEnumerable<ISymbol> symbols;
            var member = token.Parent?.Parent as MemberAccessExpressionSyntax;
            if (member != null && member.Name.Span.Contains(start))
            {
                var receiver = model.GetSymbolInfo(member.Expression).Symbol;
                var container = receiver as INamespaceOrTypeSymbol ?? model.GetTypeInfo(member.Expression).Type;
                if (container == null || container is ITypeSymbol { TypeKind: TypeKind.Error }) return empty;
                bool staticOnly = receiver is INamespaceOrTypeSymbol;
                symbols = model.LookupSymbols(start, container, includeReducedExtensionMethods: true)
                    .Where(symbol => staticOnly ? symbol.IsStatic || symbol is INamespaceOrTypeSymbol :
                        !symbol.IsStatic && symbol is not INamespaceOrTypeSymbol);
            }
            else
            {
                if (afterDot) return empty;
                symbols = model.LookupSymbols(start);
            }

            var descriptions = symbols.Where(symbol => !symbol.IsImplicitlyDeclared && symbol.CanBeReferencedByName)
                .Where(symbol => Name(symbol).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .GroupBy(Name).ToDictionary(group => group.Key, group => Describe(group.ToArray()));
            IEnumerable<string> names = descriptions.Keys;
            if (member == null)
            {
                foreach (string keyword in SyntaxFacts.GetKeywordKinds().Select(SyntaxFacts.GetText))
                    descriptions.TryAdd(keyword, new Description("keyword", "", new[] { keyword }));
            }
            return new Result(start, end - start, names
                .Where(name => name != "__completion" && name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && name != prefix)
                .Distinct(StringComparer.Ordinal).OrderBy(name => !name.StartsWith(prefix, StringComparison.Ordinal))
                .ThenBy(name => name, StringComparer.OrdinalIgnoreCase).Take(40).ToArray()) { Details = descriptions };
        }
    }
}
