namespace StyleChecker.Settings.InvalidConfig
{
    using System.Collections.Immutable;
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

        /// <summary>
        /// The key of the resource string for "invalid root element".
        /// </summary>
        public const string InvalidRootElement = nameof(R.InvalidRootElement);

        /// <summary>
        /// The key of the resource string for "invalid max line length".
        /// </summary>
        public const string InvalidMaxLineLength
            = nameof(R.InvalidMaxLineLength);

        /// <summary>
        /// The key of the resource string for "invalid namespace".
        /// </summary>
        public const string InvalidNamespace = nameof(R.InvalidNamespace);

        private const string Category = Categories.Settings;
        private static readonly DiagnosticDescriptor Rule = NewRule();
        private Config config = new Config();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            Config.Load(context, c => config = c);
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            void EndAction(CompilationAnalysisContext c)
            {
                DiagnosticConfig(c);
            }
            void NoAction(SyntaxTreeAnalysisContext c)
            {
            }
            context.RegisterCompilationStartAction(c =>
            {
                c.RegisterSyntaxTreeAction(NoAction);
                c.RegisterCompilationEndAction(EndAction);
            });
        }

        private static DiagnosticDescriptor NewRule()
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
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

        private void DiagnosticConfig(CompilationAnalysisContext context)
        {
            var message = config.ErrorMessage;
            if (message == null)
            {
                return;
            }
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            var position = config.ErrorPosition;
            var start = (position == null)
                ? new LinePosition(0, 0)
                : new LinePosition(
                    position.LineNumber - 1,
                    position.LinePosition - 1);
            var location = Location.Create(
                config.FilePath,
                new TextSpan(0, 0),
                new LinePositionSpan(start, start));
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                localize(config.ErrorMessage),
                config.ErrorArgument);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
