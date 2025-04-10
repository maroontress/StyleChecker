namespace Analyzers.Ordering.PostIncrement;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using R = Resources;

/// <summary>
/// PostIncrement analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "PostIncrement";

    private const string Category = Categories.Ordering;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
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

    private static IEnumerable<SyntaxNode> FindTargetNodes(
        CompilationUnitSyntax root)
    {
        static bool Matches(SyntaxNode n)
        {
            return n.IsKindOneOf(
                SyntaxKind.PostIncrementExpression,
                SyntaxKind.PostDecrementExpression);
        }

        static bool MatchesParent(SyntaxNode n)
        {
            return n.Parent is {} p
                && p.IsKindOneOf(
                    SyntaxKind.ExpressionStatement,
                    SyntaxKind.ForStatement);
        }

        return root.DescendantNodes()
            .Where(n => Matches(n) && MatchesParent(n));
    }

    private static void AnalyzeSyntaxTree(
        SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetCompilationUnitRoot(
            context.CancellationToken);
        var all = FindTargetNodes(root)
            .ToList();
        foreach (var token in all)
        {
            var diagnostic = Diagnostic.Create(
                Rule, token.GetLocation(), token);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
