namespace StyleChecker.Cleaning.UnusedVariable
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Syntax;
    using StyleChecker.Naming;
    using R = Resources;

    /// <summary>
    /// UnusedUsing analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "UnusedVariable";

        private const string Category = Categories.Cleaning;
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
            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree.GetCompilationUnitRoot(
                cancellationToken);
            CheckLocal(context, model, root);
            CheckParameter(context, model, root);
        }

        private static void CheckLocal(
            SemanticModelAnalysisContext context,
            SemanticModel model,
            CompilationUnitSyntax root)
        {
            var all = LocalVariables.DeclarationTokens(root)
                .Concat(LocalVariables.DesignationTokens(root))
                .ToList();
            if (all.Count == 0)
            {
                return;
            }
            ILocalSymbol[] FindLocalSymbols(SyntaxToken token)
            {
                var first = model.LookupSymbols(
                        token.Span.Start, null, token.Text)
                    .OfType<ILocalSymbol>()
                    .FirstOrDefault();
                return (first != null)
                    ? new[] { first } : Array.Empty<ILocalSymbol>();
            }
            foreach (var token in all)
            {
                var symbolsArray = FindLocalSymbols(token);
                if (!symbolsArray.Any())
                {
                    continue;
                }
                var symbol = symbolsArray[0];
                var containingSymbol = symbol.ContainingSymbol;
                var containingLocation = containingSymbol.Locations[0];
                var node = root.FindNode(containingLocation.SourceSpan);
                if (node.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(n => FindLocalSymbols(n.Identifier)
                        .Where(s => s.Equals(symbol))
                        .Any())
                    .Any())
                {
                    continue;
                }
                var diagnostic = Diagnostic.Create(
                    Rule,
                    token.GetLocation(),
                    token);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void CheckParameter(
            SemanticModelAnalysisContext context,
            SemanticModel model,
            CompilationUnitSyntax root)
        {
            var cancellationToken = context.CancellationToken;
            var methods = root.DescendantNodes()
                .OfType<BaseMethodDeclarationSyntax>()
                .ToList();
            if (methods.Count == 0)
            {
                return;
            }
            foreach (var node in methods)
            {
                var identifierNames = node.DescendantNodes()
                    .OfType<IdentifierNameSyntax>();
                var m = model.GetDeclaredSymbol(node, cancellationToken);
                var parameters = m.Parameters;
                foreach (var p in parameters)
                {
                    if (identifierNames
                        .Where(n => FindSymbols(model, n.Identifier)
                            .Where(s => s.Equals(p))
                            .Any())
                        .Any())
                    {
                        continue;
                    }
                    var location = p.Locations[0];
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        location,
                        p.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static IEnumerable<ISymbol> FindSymbols(
            SemanticModel model, SyntaxToken token)
        {
            var first = model.LookupSymbols(
                    token.Span.Start, null, token.Text)
                .OfType<ISymbol>()
                .FirstOrDefault();
            return (first != null)
                ? new[] { first } : Array.Empty<ISymbol>();
        }
    }
}
