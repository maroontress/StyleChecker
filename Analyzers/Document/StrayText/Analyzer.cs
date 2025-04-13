namespace StyleChecker.Analyzers.Document.StrayText;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Analyzers;
using R = Resources;

/// <summary>
/// StrayText analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = nameof(StrayText);

    private const string Category = Categories.Document;
    private static readonly DiagnosticDescriptor Rule = NewRule();

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
        static IEnumerable<SyntaxToken> ToStrayText(XmlTextSyntax n)
        {
            return n.Parent is not DocumentationCommentTriviaSyntax
                ? []
                : n.TextTokens
                    .Where(t => t.IsKind(SyntaxKind.XmlTextLiteralToken)
                        && t.Text.Trim().Length > 0)
                    .Take(1);
        }

        var tree = context.Tree;
        var root = tree.GetCompilationUnitRoot(
            context.CancellationToken);
        var all = root.DescendantNodes(descendIntoTrivia: true)
            .OfType<XmlTextSyntax>()
            .SelectMany(ToStrayText);
        foreach (var t in all)
        {
            var w = Location.Create(tree, t.Span);
            context.ReportDiagnostic(
                Diagnostic.Create(Rule, w, t.ToString().Trim()));
        }
    }
}
