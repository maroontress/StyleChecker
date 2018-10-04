namespace StyleChecker.Naming.ThoughtlessName
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// ThoughtlessName analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "ThoughtlessName";

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
            context.RegisterSemanticModelAction(AnalyzeModel);
        }

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var model = context.SemanticModel;
            var root = model.SyntaxTree.GetCompilationUnitRoot(
                context.CancellationToken);
            var all = new List<SyntaxToken>();
            /*
                Local variable declarations:
            */
            all.AddRange(root.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.LocalDeclarationStatement))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.VariableDeclaration))
                .SelectMany(n => n.ChildNodes())
                .Where(n => n.IsKind(SyntaxKind.VariableDeclarator))
                .SelectMany(n => n.ChildTokens())
                .Where(t => t.IsKind(SyntaxKind.IdentifierToken)));
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
                .Where(t => t.IsKind(SyntaxKind.IdentifierToken)));
            /*
                Parameters:
            */
            all.AddRange(root.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.Parameter))
                .SelectMany(n => n.ChildTokens())
                .Where(t => t.IsKind(SyntaxKind.IdentifierToken)));

            if (all.Count == 0)
            {
                return;
            }
            foreach (var token in all)
            {
                var name = token.Text;
                if (name.Length == 1)
                {
                    continue;
                }
                var span = token.Span;
                var symbol = model.LookupSymbols(span.Start, null, name)
                    .FirstOrDefault() as ILocalSymbol;
                if (symbol == null)
                {
                    continue;
                }
                var typeSymbol = symbol.Type;
                var typeName = typeSymbol.Name;
                var typeArconym = new string(typeName.ToCharArray()
                    .Where(c => char.IsUpper(c))
                    .Select(c => char.ToLower(c))
                    .ToArray());
                if (!typeArconym.Equals(name))
                {
                    continue;
                }
                var reason = string.Format(R.Acronym, name, typeName);
                var diagnostic = Diagnostic.Create(
                    Rule,
                    token.GetLocation(),
                    token,
                    reason);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
