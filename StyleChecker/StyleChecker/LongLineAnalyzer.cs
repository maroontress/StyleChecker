using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace StyleChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LongLineAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LongLineAnalyzer";

        private static readonly LocalizableString Title
            = new LocalizableResourceString(
                nameof(Resources.LongLineTitle),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat
            = new LocalizableResourceString(
                nameof(Resources.LongLineMessageFormat),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description
            = new LocalizableResourceString(
                nameof(Resources.LongLineDescription),
                Resources.ResourceManager, typeof(Resources));

        private const string Category = "Size";

        private static readonly DiagnosticDescriptor Rule
            = new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                MessageFormat,
                Category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(
            SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetCompilationUnitRoot(
                context.CancellationToken);
            var first = root.DescendantTrivia()
                .FirstOrDefault(t => t.IsKind(SyntaxKind.EndOfLineTrivia)
                    && t.GetLocation()
                        .GetLineSpan()
                        .StartLinePosition.Character >= 80);
            if (first == null || first.SyntaxTree == null)
            {
                return;
            }
            var diagnostic = Diagnostic.Create(Rule,
                first.GetLocation(), first);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
