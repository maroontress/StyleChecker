namespace StyleChecker.Naming.Underscore
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "Underscore";

        private const string Category = Categories.Naming;
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
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(
            SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetCompilationUnitRoot(
                context.CancellationToken);
            var all = new List<SyntaxToken>();
            all.AddRange(root.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.LocalDeclarationStatement))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.VariableDeclaration))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.VariableDeclarator))
                .SelectMany(n => n.ChildTokens())
                .Where(t => t.ToString().IndexOf('_') != -1)
                .ToArray());
            /*
                Count out-var (Out Variable Declarations):
                https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/out-var.md
            */
            all.AddRange(root.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.Argument))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.DeclarationExpression))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.SingleVariableDesignation))
                .SelectMany(n => n.ChildTokens())
                .Where(t => t.ToString().IndexOf('_') != -1)
                .ToArray());
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
