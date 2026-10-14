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
        internal sealed record Description(string Kind, string Type, string[] Signatures)
        {
            internal string Documentation { get; init; }
        }
        internal sealed record Edit(int Start, int Length, string Text, int? Cursor);
        internal sealed record Result(int Start, int Length, string[] Items)
        {
            internal IReadOnlyDictionary<string, Edit> Changes { get; init; } = new Dictionary<string, Edit>();
            internal IReadOnlyDictionary<string, Description> Details { get; init; }
                = new Dictionary<string, Description>();
        }

        private static readonly SymbolDisplayFormat SignatureFormat = SymbolDisplayFormat.MinimallyQualifiedFormat
            .WithMemberOptions(SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeContainingType |
                SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeModifiers)
            .WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName |
                SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeDefaultValue |
                SymbolDisplayParameterOptions.IncludeOptionalBrackets);

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

        private static readonly Lazy<Microsoft.CodeAnalysis.Host.Mef.MefHostServices> Host = new(() =>
            Microsoft.CodeAnalysis.Host.Mef.MefHostServices.Create(
                Microsoft.CodeAnalysis.Host.Mef.MefHostServices.DefaultAssemblies.Concat(new[] {
                    System.Reflection.Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
                    System.Reflection.Assembly.Load("Microsoft.CodeAnalysis.Features"),
                    System.Reflection.Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")
                }).Distinct()));

        internal static Result Get(string source, int cursor, string path,
            KeyValuePair<string, string>[] otherSources, bool force = false)
            => GetAsync(source, cursor, path, otherSources, force).GetAwaiter().GetResult();

        internal static async System.Threading.Tasks.Task<Result> GetAsync(string source, int cursor, string path,
            KeyValuePair<string, string>[] otherSources, bool force = false,
            System.Threading.CancellationToken cancellationToken = default)
        {
            cursor = Math.Clamp(cursor, 0, source.Length);
            int start = cursor, end = cursor;
            while (start > 0 && SyntaxFacts.IsIdentifierPartCharacter(source[start - 1])) start--;
            while (end < source.Length && SyntaxFacts.IsIdentifierPartCharacter(source[end])) end++;
            string prefix = source.Substring(start, cursor - start);
            var empty = new Result(start, end - start, Array.Empty<string>());
            if (!force && prefix.Length < 2 && !(start > 0 && source[start - 1] == '.')) return empty;

            using var workspace = new AdhocWorkspace(Host.Value);
            var project = workspace.AddProject(ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(),
                "ScriptCompletion", "ScriptCompletion", LanguageNames.CSharp,
                parseOptions: CSharpParseOptions.Default,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: References.Value));
            var sources = otherSources.Where(pair => !string.Equals(pair.Key, path, StringComparison.OrdinalIgnoreCase))
                .Select(pair => ScriptCompilationDefaults.Parse(pair.Value, pair.Key));
            foreach (var tree in ScriptCompilationDefaults.WithImports(sources))
                workspace.AddDocument(project.Id, tree.FilePath, tree.GetText(cancellationToken));
            var document = workspace.AddDocument(project.Id, path,
                ScriptCompilationDefaults.Parse(source, path).GetText(cancellationToken));
            var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(document);
            if (service == null) throw new InvalidOperationException("C# completion service is unavailable.");
            var list = await service.GetCompletionsAsync(document, cursor,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (list == null) return empty;
            var items = list.Items
                .Where(item => item.FilterText.TrimStart('@').StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.Rules.MatchPriority)
                .ThenBy(item => !item.FilterText.StartsWith(prefix, StringComparison.Ordinal))
                .ThenBy(item => item.SortText, StringComparer.OrdinalIgnoreCase)
                .GroupBy(item => item.DisplayText).Select(group => group.First()).Take(40).ToArray();

            var details = new Dictionary<string, Description>();
            var changes = new Dictionary<string, Edit>();
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var member = root.FindToken(Math.Max(0, cursor - 1)).Parent?.AncestorsAndSelf()
                .OfType<MemberAccessExpressionSyntax>().FirstOrDefault(m => m.Name.FullSpan.Start <= cursor && m.FullSpan.End >= cursor);
            INamespaceOrTypeSymbol container = null;
            if (member != null)
                container = model.GetSymbolInfo(member.Expression, cancellationToken).Symbol as INamespaceOrTypeSymbol
                    ?? model.GetTypeInfo(member.Expression, cancellationToken).Type;
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var description = await service.GetDescriptionAsync(document, item, cancellationToken).ConfigureAwait(false);
                var change = await service.GetChangeAsync(document, item, cancellationToken: cancellationToken).ConfigureAwait(false);
                // Roslyn may insert escaped identifiers or replace a span different from the typed prefix.
                changes[item.DisplayText] = new Edit(change.TextChange.Span.Start, change.TextChange.Span.Length,
                    change.TextChange.NewText, change.NewPosition);
                var symbols = model.LookupSymbols(cursor, container, name: item.DisplayText.TrimStart('@'),
                    includeReducedExtensionMethods: true).Where(s => s.CanBeReferencedByName).ToArray();
                var detail = symbols.Length > 0 ? Describe(symbols)
                    : new Description(item.Tags.FirstOrDefault()?.ToLowerInvariant() ?? "", "", Array.Empty<string>());
                details[item.DisplayText] = detail with { Documentation = description.Text };
            }
            return new Result(start, end - start, items.Select(item => item.DisplayText).ToArray())
                { Details = details, Changes = changes };
        }
    }
}
