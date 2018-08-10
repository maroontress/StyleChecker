using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace StyleChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PostIncrementAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PostIncrementAnalyzer";

        private static readonly LocalizableString Title
            = new LocalizableResourceString(
                nameof(Resources.PostIncrementTitle),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat
            = new LocalizableResourceString(
                nameof(Resources.PostIncrementMessageFormat),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description
            = new LocalizableResourceString(
                nameof(Resources.PostIncrementDescription),
                Resources.ResourceManager, typeof(Resources));

        private const string Category = "Ordering";

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
            Predicate<SyntaxNode> matches = n =>
            {
                return n.IsKind(SyntaxKind.PostIncrementExpression)
                    || n.IsKind(SyntaxKind.PostDecrementExpression);
            };
            Predicate<SyntaxNode> matchesParent = n =>
            {
                var p = n.Parent;
                return p != null
                    && (p.IsKind(SyntaxKind.ExpressionStatement)
                        || p.IsKind(SyntaxKind.ForStatement));
            };
            var all = root.DescendantNodes()
                .Where(n => matches(n) && matchesParent(n))
                .ToArray();
            if (all.Length == 0)
            {
                return;
            }
            foreach (var token in all)
            {
                var diagnostic = Diagnostic.Create(Rule,
                    token.GetLocation(), token);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
