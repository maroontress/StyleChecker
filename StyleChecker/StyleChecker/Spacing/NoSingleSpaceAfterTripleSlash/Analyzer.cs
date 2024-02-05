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
        = ImmutableHashSet.Create(' ', '\t');

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => ImmutableArray.Create(Rule);

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
            => t.Kind() is SyntaxKind.SingleLineDocumentationCommentTrivia;

        static bool IsDceTrivia(SyntaxTrivia t)
            => t.Kind() is SyntaxKind.DocumentationCommentExteriorTrivia;

        static bool IsSingleSpace(SyntaxTrivia t)
            => t.Kind() is SyntaxKind.WhitespaceTrivia
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
            if (k == all.Count - 1)
            {
                return null;
            }
            return all[k + 1];
        }

        static bool IsNextTokenNewLine(
            XmlTextSyntax s, SyntaxToken t)
        {
            var all = s.TextTokens;
            var k = all.IndexOf(t);
            if (k is -1)
            {
                throw new ArgumentException("t");
            }
            if (k == all.Count - 1)
            {
                return false;
            }
            return all[k + 1].Kind()
                    is SyntaxKind.XmlTextLiteralNewLineToken;
        }

        static bool DoesTokenHaveGoodSpace(SyntaxToken t)
        {
            var text = t.Text;
            var p = t.Parent;
            if (!(p is XmlTextSyntax child))
            {
                return false;
            }
            if (!WhitespaceCharSet.Contains(text[0]))
            {
                return false;
            }
            if (!(child.Parent is DocumentationCommentTriviaSyntax top)
                || !IsSldcTrivia(top))
            {
                return true;
            }
            var next = GetNextElement(top, child);
            if (next is null)
            {
                return true;
            }
            if (IsNextTokenNewLine(child, t))
            {
                return true;
            }
            return text.Length is 1;
        }

        static bool DoesTokenStartWithWhiteSpace(SyntaxToken t)
        {
            var k = t.Kind();
            return k is SyntaxKind.XmlTextLiteralNewLineToken
                || (k is SyntaxKind.XmlTextLiteralToken
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

        var tree = context.Tree;
        var root = tree.GetCompilationUnitRoot(
            context.CancellationToken);
        var all = root.DescendantNodes(descendIntoTrivia: true)
            .OfType<DocumentationCommentTriviaSyntax>()
            .Where(t => IsSldcTrivia(t))
            .SelectMany(t => t.DescendantTrivia())
            .Where(t => IsDceTrivia(t)
                && !DoesTokenHaveSingleLeadingTrivia(t)
                && !IsNextSiblingTriviaSingleSpace(t));

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
        foreach (var t in all)
        {
            var w = Location.Create(tree, t.Token.Span);
            context.ReportDiagnostic(Diagnostic.Create(Rule, w));
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
