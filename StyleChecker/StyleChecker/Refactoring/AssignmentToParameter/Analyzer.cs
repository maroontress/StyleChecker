namespace StyleChecker.Refactoring.AssignmentToParameter
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
    using R = Resources;

    /// <summary>
    /// AssignmentToParameter analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "AssignmentToParameter";

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
            static bool IsRefOrOut(RefKind kind)
                => kind == RefKind.Out || kind == RefKind.Ref;

            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree
                .GetCompilationUnitRoot(cancellationToken);
            var allNodes = root.DescendantNodes();
            var assignPart = allNodes
                .OfType<AssignmentExpressionSyntax>()
                .Select(n => model.GetOperation(n, cancellationToken))
                .OfType<IAssignmentOperation>()
                .Select(o => o.Target);
            var unaryPart = allNodes
                .Where(n => n.IsKindOneOf(
                    SyntaxKind.PreIncrementExpression,
                    SyntaxKind.PreDecrementExpression,
                    SyntaxKind.PostIncrementExpression,
                    SyntaxKind.PostDecrementExpression))
                .Select(n => model.GetOperation(n, cancellationToken)
                    .Children.FirstOrDefault())
                .Where(o => !(o is null));
            var argumentPart = allNodes
                .OfType<ArgumentSyntax>()
                .Select(n => model.GetOperation(n, cancellationToken))
                .OfType<IArgumentOperation>()
                .Where(o => IsRefOrOut(o.Parameter.RefKind))
                .Select(o => o.Value);
            var all = assignPart
                .Concat(unaryPart)
                .Concat(argumentPart)
                .OfType<IParameterReferenceOperation>()
                .Where(o => o.Parameter.RefKind == RefKind.None)
                .ToList();

            foreach (var o in all)
            {
                var location = o.Syntax.GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    o.Parameter.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
