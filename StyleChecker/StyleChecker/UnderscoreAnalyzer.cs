using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StyleChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnderscoreAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "UnderscoreAnalyzer";

        private static readonly LocalizableString Title
            = new LocalizableResourceString(nameof(Resources.UnderscoreTitle),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat
            = new LocalizableResourceString(nameof(Resources.UnderscoreMessageFormat),
                Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description
            = new LocalizableResourceString(nameof(Resources.UnderscoreDescription),
                Resources.ResourceManager, typeof(Resources));

        private const string Category = "Naming";

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
            var tokens = root.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.LocalDeclarationStatement))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.VariableDeclaration))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.VariableDeclarator))
                .SelectMany(n => n.ChildTokens())
                .Where(t => t.ToString().IndexOf('_') != -1)
                .Select(t => t)
                .ToArray();
            if (tokens.Length == 0)
            {
                return;
            }
            foreach (var token in tokens)
            {
                var diagnostic = Diagnostic.Create(Rule, token.GetLocation(), token);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

