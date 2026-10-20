using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Unify2D.Scripting
{
    internal static class ScriptComponentRules
    {
        private static readonly DiagnosticDescriptor MultipleComponents = new(
            "U2D001", "One component class per file",
            "Le fichier '{0}' contient plusieurs classes de composants : {1}. Placez chaque composant dans son propre fichier .cs.",
            "Scripts", DiagnosticSeverity.Error, isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor SplitComponent = new(
            "U2D002", "One file per component class",
            "Le composant '{0}' est déclaré dans plusieurs fichiers. Regroupez ses déclarations dans un seul fichier .cs pour conserver une identité .meta unique.",
            "Scripts", DiagnosticSeverity.Error, isEnabledByDefault: true);

        internal static List<Diagnostic> Validate(Compilation compilation)
        {
            var errors = new List<Diagnostic>();
            var component = compilation.GetTypeByMetadataName("Unify2D.Core.Component");
            if (component == null) return errors; // Normal compiler diagnostics handle missing references.
            var declarations = new Dictionary<INamedTypeSymbol, List<Location>>(SymbolEqualityComparer.Default);
            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);
                var components = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    var type = model.GetDeclaredSymbol(declaration) as INamedTypeSymbol;
                    if (type == null) continue;
                    for (var parent = type.BaseType; parent != null; parent = parent.BaseType)
                    {
                        if (!SymbolEqualityComparer.Default.Equals(parent, component)) continue;
                        if (components.Add(type))
                        {
                            if (!declarations.TryGetValue(type, out var locations))
                                declarations[type] = locations = new List<Location>();
                            locations.Add(declaration.Identifier.GetLocation());
                        }
                        break;
                    }
                }
                if (components.Count > 1)
                    errors.Add(Diagnostic.Create(MultipleComponents,
                        declarations[components.First()].Last(), tree.FilePath,
                        string.Join(", ", components.Select(type => type.ToDisplayString()).OrderBy(name => name))));
            }
            foreach (var pair in declarations.Where(pair => pair.Value.Count > 1))
                errors.Add(Diagnostic.Create(SplitComponent, pair.Value[0], pair.Key.ToDisplayString()));
            return errors;
        }
    }
}
