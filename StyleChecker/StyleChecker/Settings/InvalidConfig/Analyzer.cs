namespace StyleChecker.Settings.InvalidConfig
{
    using System.Collections.Immutable;
    using System.Xml;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using R = Resources;

    /// <summary>
    /// InvalidConfig analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "InvalidConfig";

        private const string Category = Categories.Settings;
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
            context.RegisterCompilationStartAction(StartAction);
        }

        private static void StartAction(
            CompilationStartAnalysisContext context)
        {
            var pod = ConfigBank.LoadRootConfig(context);
            context.RegisterCompilationEndAction(c =>
            {
                DiagnosticConfig(c, pod);
            });
        }

        private static DiagnosticDescriptor NewRule()
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            return new DiagnosticDescriptor(
                DiagnosticId,
                localize(nameof(R.Title)),
                localize(nameof(R.MessageFormat)),
                Category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)),
                helpLinkUri: HelpLink.ToUri(DiagnosticId));
        }

        private static void DiagnosticConfig(
            CompilationAnalysisContext context, ConfigPod pod)
        {
            var localize = Localizers.Of<R>(R.ResourceManager);

            Location NewLocation(int row, int col)
            {
                var start = new LinePosition(row, col);
                return Location.Create(
                    pod.Path,
                    new TextSpan(0, 0),
                    new LinePositionSpan(start, start));
            }

            if (pod.Exception is XmlException xmlException)
            {
                var row = xmlException.LineNumber - 1;
                var col = xmlException.LinePosition - 1;
                var diagnostic = Diagnostic.Create(
                    Rule,
                    NewLocation(row, col),
                    ConfigBank.Filename,
                    xmlException.Message);
                context.ReportDiagnostic(diagnostic);
                return;
            }
            if (pod.Exception != null)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    NewLocation(0, 0),
                    ConfigBank.Filename,
                    pod.Exception.Message);
                context.ReportDiagnostic(diagnostic);
                return;
            }
            var errors = pod.RootConfig.Validate();
            foreach (var m in errors)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    NewLocation(0, 0),
                    ConfigBank.Filename,
                    m);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
