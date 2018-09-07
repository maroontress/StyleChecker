namespace StyleChecker.Settings.InvalidConfig
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using R = Resources;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "InvalidConfig";

        public const string InvalidRootElement = nameof(R.InvalidRootElement);

        public const string InvalidMaxLineLength
            = nameof(R.InvalidMaxLineLength);

        public const string InvalidNamespace = nameof(R.InvalidNamespace);

        private const string Category = Categories.Settings;
        private static readonly DiagnosticDescriptor Rule;
        private Config config = new Config();

        static Analyzer()
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            Rule = new DiagnosticDescriptor(
                DiagnosticId,
                localize(nameof(R.Title)),
                localize(nameof(R.MessageFormat)),
                Category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)));
        }

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            Config.Load(context, c => config = c);
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            Action<CompilationAnalysisContext> endAction = c =>
            {
                DiagnosticConfig(c);
            };
            Action<SyntaxTreeAnalysisContext> noAction = c =>
            {
            };
            context.RegisterCompilationStartAction(c =>
            {
                c.RegisterSyntaxTreeAction(noAction);
                c.RegisterCompilationEndAction(endAction);
            });
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
