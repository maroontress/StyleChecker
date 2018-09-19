namespace StyleChecker.Cleaning.UnusedUsing
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// UnusedUsing analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "UnusedUsing";

        private const string Category = Categories.Cleaning;
        private static readonly DiagnosticDescriptor Rule;

        static Analyzer()
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            Rule = new DiagnosticDescriptor(
                DiagnosticId,
                localize(nameof(R.Title)),
                localize(nameof(R.MessageFormat)),
                Category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)));
        }

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

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var model = context.SemanticModel;
            var all = model.GetDiagnostics();
            var first = all.Where(d => d.Id == "CS8019")
                .FirstOrDefault();
            if (first == null)
            {
                return;
            }
            var diagnostic = Diagnostic.Create(
                Rule,
                first.Location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
