using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace StyleChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnderscoreAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UnderscoreAnalyzer";

        private static readonly LocalizableString Title
            = new LocalizableResourceString(
                nameof(Resources.UnderscoreTitle),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat
            = new LocalizableResourceString(
                nameof(Resources.UnderscoreMessageFormat),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description
            = new LocalizableResourceString(
                nameof(Resources.UnderscoreDescription),
                Resources.ResourceManager, typeof(Resources));

        private const string Category = "Naming";

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
            var all = root.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.LocalDeclarationStatement))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.VariableDeclaration))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.VariableDeclarator))
                .SelectMany(n => n.ChildTokens())
                .Where(t => t.ToString().IndexOf('_') != -1)
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
