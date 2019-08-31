namespace StyleChecker.Refactoring.EmptyArrayCreation
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
    using StyleChecker.Refactoring;
    using R = Resources;

    /// <summary>
    /// EmptyArrayCreation analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "EmptyArrayCreation";

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
            static bool IsZeroSize(IArrayCreationOperation o)
            {
                var size = o.DimensionSizes[0].ConstantValue;
                return o.Initializer is null
                    && size.HasValue
                    && size.Value is int intValue
                    && intValue == 0;
            }

            static bool NoInitializer(IArrayCreationOperation o)
            {
                var initializer = o.Initializer;
                return !(initializer is null)
                    && !initializer.ElementValues.Any();
            }

            var root = context.GetCompilationUnitRoot();
            var model = context.SemanticModel;
            var all = root.DescendantNodes()
                .OfType<ArrayCreationExpressionSyntax>()
                .Select(n => model.GetOperation(n))
                .OfType<IArrayCreationOperation>()
                .Where(o => o.DimensionSizes.Length == 1
                    && (IsZeroSize(o) || NoInitializer(o)));

            foreach (var o in all)
            {
                if (!(o.Syntax is ArrayCreationExpressionSyntax node))
                {
                    continue;
                }
                var type = node.Type.ElementType;
                var diagnostic = Diagnostic.Create(
                    Rule,
                    node.GetLocation(),
                    type);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
