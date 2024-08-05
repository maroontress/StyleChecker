namespace StyleChecker.Spacing.NoSingleSpaceAfterTripleSlash;

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using R = Resources;

/// <summary>
/// NoSingleSpaceAfterTripleSlash analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId
        = nameof(NoSingleSpaceAfterTripleSlash);

    private const string Category = Categories.Spacing;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    private static readonly ImmutableHashSet<char> WhitespaceCharSet
        = [' ', '\t'];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(SyntaxTreeAction);
    }

    private static DiagnosticDescriptor NewRule()
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        return new DiagnosticDescriptor(
            DiagnosticId,
            localize(nameof(R.Title)),
            localize(nameof(R.MessageFormat)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static void SyntaxTreeAction(SyntaxTreeAnalysisContext context)
    {
        static bool IsSldcTrivia(SyntaxNode t)
            => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia);

        static bool IsDceTrivia(SyntaxTrivia t)
            => t.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia);

        static bool IsSingleSpace(SyntaxTrivia t)
            => t.IsKind(SyntaxKind.WhitespaceTrivia)
                && t.Span.Length is 1;

        static XmlNodeSyntax? GetNextElement(
            DocumentationCommentTriviaSyntax s,
            XmlTextSyntax t)
        {
            var all = s.Content;
            var k = all.IndexOf(t);
            if (k is -1)
            {
                throw new ArgumentException("t");
            }
            return (k == all.Count - 1)
                ? null
                : all[k + 1];
        }

        static bool IsNextTokenNewLine(XmlTextSyntax s, SyntaxToken t)
        {
            var all = s.TextTokens;
            var k = all.IndexOf(t);
            if (k is -1)
            {
                throw new ArgumentException("t");
            }
            return k != all.Count - 1
                && all[k + 1].IsKind(SyntaxKind.XmlTextLiteralNewLineToken);
        }

        static bool DoesTokenHaveGoodSpace(SyntaxToken t)
        {
            var text = t.Text;
            return t.Parent is XmlTextSyntax child
                && WhitespaceCharSet.Contains(text[0])
                && (child.Parent is not DocumentationCommentTriviaSyntax top
                    || !IsSldcTrivia(top)
                    || GetNextElement(top, child) is null
                    || IsNextTokenNewLine(child, t)
                    || text.Length is 1);
        }

        static bool DoesTokenStartWithWhiteSpace(SyntaxToken t)
        {
            return t.IsKind(SyntaxKind.XmlTextLiteralNewLineToken)
                || (t.IsKind(SyntaxKind.XmlTextLiteralToken)
                    && DoesTokenHaveGoodSpace(t));
        }

        static bool IsNextSiblingTriviaSingleSpace(SyntaxTrivia t)
        {
            var a = t.Token.LeadingTrivia;
            var n = a.IndexOf(t);
            return a.Count >= n + 2
                && IsSingleSpace(a[n + 1]);
        }

        static bool DoesTokenHaveSingleLeadingTrivia(SyntaxTrivia t)
        {
            var p = t.Token;
            var a = p.LeadingTrivia;
            return DoesTokenStartWithWhiteSpace(p)
                && a.Last() == t;
        }

        static bool DoesTokenHaveTextStartingWithSingleSpace(SyntaxTrivia t)
        {
            var text = t.Token.Text;
            return text.Length > 1
                && WhitespaceCharSet.Contains(text[0])
                && !WhitespaceCharSet.Contains(text[1]);
        }

        static Func<SyntaxTrivia, Location> LocationSupplier(SyntaxTree tree)
            => t => Location.Create(tree, t.Token.Span);

        var tree = context.Tree;
        var toLocation = LocationSupplier(tree);
        var root = tree.GetCompilationUnitRoot(context.CancellationToken);
        var all = root.DescendantNodes(descendIntoTrivia: true)
            .OfType<DocumentationCommentTriviaSyntax>()
            .Where(IsSldcTrivia)
            .SelectMany(t => t.DescendantTrivia())
            .Where(t => IsDceTrivia(t)
                && !DoesTokenHaveSingleLeadingTrivia(t)
                && !IsNextSiblingTriviaSingleSpace(t)
                && !DoesTokenHaveTextStartingWithSingleSpace(t))
            .Select(t => Diagnostic.Create(Rule, toLocation(t)))
            .ToList();

/*
    Case 1a:

    XmlTextLiteralNewLineToken
    + Lead: DocumentationCommentExteriorTrivia

    Case 1b:

    SingleLineDocumentationCommentTrivia
    + ...
    + XmlText
    | + ...
    | + XmlTextLiteralToken (equals " ")
    |   + Lead: ...
    |   + Lead: DocumentationCommentExteriorTrivia
    + ...
    + [...]
      + XmlElement
        + ...
        + XmlText
          + ...
          + XmlTextLiteralToken (starts with " "...)
             + Lead: ...
             + Lead: DocumentationCommentExteriorTrivia

    Case 2:

    Token
    + Lead: ...
    + Lead: DocumentationCommentExteriorTrivia
    + Lead: WhiteSpaceTrivia
    + Lead: ...
*/
        foreach (var d in all)
        {
            context.ReportDiagnostic(d);
        }
    }

/*
    StyleCop.Analyzers (1.1.118) emits SA1004 to the following code:

        /// <seealso cref="LineBreakInsideAttribute
        /// (string, string)"/>
        /// <seealso cref="LineBreakInsideAttribute(
        /// string, string)"/>
        /// <seealso cref="LineBreakInsideAttribute(string,
        /// string)"/>
        /// <seealso cref="LineBreakInsideAttribute(string, string
        /// )"/>
        private void LineBreakInsideAttribute(string a, string b)
        {
        }
*/
}
