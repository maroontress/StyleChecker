namespace StyleChecker.Refactoring.StinkyBooleanExpression
{
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
                => o.Type.SpecialType is SpecialType.System_Boolean;

            static bool HasWhenTrueOrFalseKindOf(
                ConditionalExpressionSyntax s,
                SyntaxKind kind)
            {
                return s.WhenTrue.Kind() == kind
                    || s.WhenFalse.Kind() == kind;
            }

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
                return !(ToOperation(n) is IConditionalOperation o)
                    ? Enumerable.Empty<(ConditionalExpressionSyntax,
                        IConditionalOperation)>()
                    : ImmutableArray.Create((n, o));
            }

            var boolLiteralExpressionSet = ImmutableHashSet.Create(
                SyntaxKind.TrueLiteralExpression,
                SyntaxKind.FalseLiteralExpression);

            bool AreBothBoolLiterals(ConditionalExpressionSyntax s)
                => boolLiteralExpressionSet.SetEquals(
                    ImmutableArray.Create(
                        s.WhenTrue.Kind(),
                        s.WhenFalse.Kind()));

            var allConditionalPods = root.DescendantNodes()
                .OfType<ConditionalExpressionSyntax>()
                .SelectMany(s => ToConditionalPods(s))
                .Where(p => IsOperationTypeBool(p.Operation))
                .ToArray();
            var targets = allConditionalPods
                .Where(p => !AreBothBoolLiterals(p.Node))
                .ToArray();
            var trueLiteralPods = targets.Where(
                p => HasWhenTrueOrFalseKindOf(
                    p.Node, SyntaxKind.TrueLiteralExpression));
            var falseLiteralPods = targets.Where(
                p => HasWhenTrueOrFalseKindOf(
                    p.Node, SyntaxKind.FalseLiteralExpression));

            var allToUseConditionalLogicalOr = trueLiteralPods
                .Select(p => (p.Node, R.UseConditionalLogicalOr));
            var allToUseConditionalLogicalAnd = falseLiteralPods
                .Select(p => (p.Node, R.UseConditionalLogicalAnd));
            var all = allToUseConditionalLogicalAnd
                .Concat(allToUseConditionalLogicalOr);
            foreach (var (node, message) in all)
            {
                var location = node.GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    message);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
