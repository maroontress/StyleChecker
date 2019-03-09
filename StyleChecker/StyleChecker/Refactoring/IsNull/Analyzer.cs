namespace StyleChecker.Refactoring.IsNull
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
    /// IsNull analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "IsNull";

        private const string Category = Categories.Cleaning;
        private static readonly DiagnosticDescriptor Rule = NewRule();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
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
                DiagnosticSeverity.Info,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)),
                helpLinkUri: HelpLink.ToUri(DiagnosticId));
        }

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            bool IsNullConstant(Optional<object> v)
                => v.HasValue && v.Value == null;

            bool IsNullLiteral(IOperation o)
                => (o.IsImplicit && o is IConversionOperation conversion)
                    ? IsNullLiteral(conversion.Operand)
                    : (o is ILiteralOperation literal
                       && IsNullConstant(literal.ConstantValue));

            bool IsNull(IPatternOperation o)
                => o is IConstantPatternOperation constantPattern
                    && IsNullLiteral(constantPattern.Value);

            bool Matches(IIsPatternOperation o)
                => IsNull(o.Pattern);

            var root = context.GetCompilationUnitRoot();
            var model = context.SemanticModel;
            var all = root.DescendantNodes()
                .OfType<IsPatternExpressionSyntax>()
                .Select(n => model.GetOperation(n))
                .OfType<IIsPatternOperation>()
                .Where(Matches);

            foreach (var o in all)
            {
                if (!(o.Syntax is IsPatternExpressionSyntax node))
                {
                    continue;
                }

                Diagnostic Of(SyntaxNode n, SyntaxKind k)
                    => Diagnostic.Create(
                        Rule,
                        n.GetLocation(),
                        SyntaxFactory.Token(k));

                var diagnostic
                    = (node.Parent is ParenthesizedExpressionSyntax paren
                        && paren.Parent is PrefixUnaryExpressionSyntax prefix
                        && prefix.OperatorToken.Kind()
                            == SyntaxKind.ExclamationToken)
                        ? Of(prefix, SyntaxKind.ExclamationEqualsToken)
                        : Of(node, SyntaxKind.EqualsEqualsToken);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
