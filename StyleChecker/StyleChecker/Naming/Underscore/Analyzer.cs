namespace StyleChecker.Naming.Underscore
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// Underscore analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "Underscore";

        private const string Category = Categories.Naming;
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

        private static void AnalyzeSyntaxTree(
            SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetCompilationUnitRoot(
                context.CancellationToken);

            bool ContainsUndersore(SyntaxToken t)
                => t.Text.IndexOf('_') != -1;
            var all = LocalVariables.DeclarationTokens(root)
                .Concat(LocalVariables.OutVariableTokens(root))
                .Concat(LocalVariables.PatternMatchingTokens(root))
                .Concat(LocalVariables.ParameterTokens(root))
                .Concat(LocalVariables.FunctionTokens(root))
                .Concat(LocalVariables.CatchTokens(root))
                .Concat(LocalVariables.ForEachTokens(root))
                .Where(ContainsUndersore)
                .ToList();
            if (all.Count == 0)
            {
                return;
            }
            foreach (var token in all)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    token.GetLocation(),
                    token);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
