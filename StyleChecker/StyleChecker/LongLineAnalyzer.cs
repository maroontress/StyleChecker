using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StyleChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LongLineAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LongLineAnalyzer";

        private static readonly LocalizableString Title
            = new LocalizableResourceString(nameof(Resources.LongLineTitle),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat
            = new LocalizableResourceString(nameof(Resources.LongLineMessageFormat),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description
            = new LocalizableResourceString(nameof(Resources.LongLineDescription),
                Resources.ResourceManager, typeof(Resources));

        private const string Category = "Size";

        private static DiagnosticDescriptor Rule
            = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
            var trivias = root.DescendantTrivia();
            foreach (var t in trivias)
            {
                if (t.IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    var col = t.GetLocation().GetLineSpan().StartLinePosition.Character;
                    if (col >= 80)
                    {
                        var diagnostic = Diagnostic.Create(Rule, t.GetLocation(), t);
                        context.ReportDiagnostic(diagnostic);
                        break;
                    }
                }
            }
        }
    }
}
