namespace StyleChecker.Refactoring.StinkyBooleanExpression;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using R = Resources;

/// <summary>
/// StinkyBooleanExpression analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "StinkyBooleanExpression";

    private const string Category = Categories.Refactoring;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => ImmutableArray.Create(Rule);

    private static ImmutableHashSet<SyntaxKind>
            BoolLiteralExpressionSet { get; } = ImmutableHashSet.Create(
        SyntaxKind.TrueLiteralExpression,
        SyntaxKind.FalseLiteralExpression);

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(AnalyzeModel);
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

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context)
    {
        static bool IsOperationTypeBool(IOperation o)
            => o.Type is {} type
                && type.SpecialType is SpecialType.System_Boolean;

        static bool AreBothBoolLiterals(ConditionalExpressionSyntax s)
            => BoolLiteralExpressionSet.SetEquals(
                [s.WhenTrue.Kind(), s.WhenFalse.Kind()]);

        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        var root = model.SyntaxTree
            .GetCompilationUnitRoot(cancellationToken);

        IOperation? ToOperation(SyntaxNode n)
            => model.GetOperation(n, cancellationToken);

        IEnumerable<(ConditionalExpressionSyntax Node,
                IConditionalOperation Operation)>
            ToConditionalPods(ConditionalExpressionSyntax n)
        {
            return ToOperation(n) is not IConditionalOperation o
                ? [] : [(n, o)];
        }

        var targets = root.DescendantNodes()
            .OfType<ConditionalExpressionSyntax>()
            .SelectMany(ToConditionalPods)
            .Where(p => IsOperationTypeBool(p.Operation)
                && !AreBothBoolLiterals(p.Node))
            .ToList();
        var allToUseConditionalLogicalAnd = targets.Where(
                p => p.Node.BothIsKind(SyntaxKind.TrueLiteralExpression))
            .Select(p => (p.Node, R.UseConditionalLogicalOr));
        var allToUseConditionalLogicalOr = targets.Where(
                p => p.Node.BothIsKind(SyntaxKind.FalseLiteralExpression))
            .Select(p => (p.Node, R.UseConditionalLogicalAnd));
        var all = allToUseConditionalLogicalAnd
            .Concat(allToUseConditionalLogicalOr)
            .ToList();
        foreach (var (node, message) in all)
        {
            var location = node.GetLocation();
            var diagnostic = Diagnostic.Create(
                Rule, location, message);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
