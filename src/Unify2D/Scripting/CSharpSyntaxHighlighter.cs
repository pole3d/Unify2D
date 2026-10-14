using System;
using ImGuiColorTextEditNet;
using ImGuiColorTextEditNet.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Unify2D.Scripting
{
    internal sealed class CSharpSyntaxHighlighter : ISyntaxHighlighter
    {
        private SourceText _text;
        private PaletteIndex[] _colors = Array.Empty<PaletteIndex>();
        public bool AutoIndentation => true;
        public int MaxLinesPerFrame => 2048;
        public string GetTooltip(string id) => null;

        public void Update(string source)
        {
            var tree = CSharpSyntaxTree.ParseText(source);
            _text = tree.GetText();
            _colors = new PaletteIndex[source.Length];
            var root = tree.GetRoot();
            foreach (var token in root.DescendantTokens())
            {
                var kind = token.Kind();
                PaletteIndex color = PaletteIndex.Punctuation;
                if (SyntaxFacts.IsKeywordKind(kind) || SyntaxFacts.IsContextualKeyword(kind)) color = PaletteIndex.Keyword;
                else if (kind == SyntaxKind.NumericLiteralToken) color = PaletteIndex.Number;
                else if (kind == SyntaxKind.CharacterLiteralToken) color = PaletteIndex.CharLiteral;
                else if (kind.ToString().Contains("String")) color = PaletteIndex.String;
                else if (kind == SyntaxKind.IdentifierToken)
                    color = token.Parent is BaseTypeDeclarationSyntax or TypeParameterSyntax ? PaletteIndex.KnownIdentifier : PaletteIndex.Identifier;
                Paint(token.Span, color);
            }
            foreach (var trivia in root.DescendantTrivia())
            {
                var kind = trivia.Kind();
                if (kind is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia or
                    SyntaxKind.SingleLineDocumentationCommentTrivia or SyntaxKind.MultiLineDocumentationCommentTrivia)
                    Paint(trivia.FullSpan, PaletteIndex.Comment);
                else if (trivia.IsDirective) Paint(trivia.FullSpan, PaletteIndex.Preprocessor);
                else if (kind == SyntaxKind.DisabledTextTrivia) Paint(trivia.FullSpan, PaletteIndex.Comment);
            }
        }

        private void Paint(TextSpan span, PaletteIndex color)
        {
            for (int i = span.Start; i < span.End && i < _colors.Length; i++) _colors[i] = color;
        }

        public object Colorize(Span<Glyph> line, object state)
        {
            int index = state is int next ? next : 0;
            int start = _text != null && index < _text.Lines.Count ? _text.Lines[index].Start : _colors.Length;
            for (int i = 0; i < line.Length; i++)
                line[i] = new Glyph(line[i].Char, start + i < _colors.Length ? _colors[start + i] : PaletteIndex.Default);
            return index + 1;
        }
    }
}
