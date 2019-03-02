namespace StyleChecker.Refactoring.StaticGenericClass
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// StaticGenericClass analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "StaticGenericClass";

        private const string Category = Categories.Refactoring;
        private static readonly DiagnosticDescriptor Rule = NewRule();

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

        private static DiagnosticDescriptor NewRule()
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            return new DiagnosticDescriptor(
                DiagnosticId,
                localize(nameof(R.Title)),
                localize(nameof(R.MessageFormat)),
                Category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)),
                helpLinkUri: HelpLink.ToUri(DiagnosticId));
        }

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var model = context.SemanticModel;
            var root = model.SyntaxTree.GetCompilationUnitRoot(
                context.CancellationToken);
            var all = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>();
            if (!all.Any())
            {
                return;
            }

            var cancellationToken = context.CancellationToken;
            foreach (var node in all)
            {
                var classSymbol
                    = model.GetDeclaredSymbol(node, cancellationToken);
                var typeParameterList = node.TypeParameterList;
                if (!classSymbol.IsStatic
                    || typeParameterList == null
                    || !typeParameterList.Parameters.Any())
                {
                    continue;
                }

                bool IsClassTypeParameter(ISymbol s)
                    => s.Kind == SymbolKind.TypeParameter
                        && s.ContainingSymbol == classSymbol;
                bool IsTargetMethod(MethodDeclarationSyntax m)
                    => m.DescendantNodes()
                        .Where(n => n.IsKind(SyntaxKind.IdentifierName))
                        .Select(n => model.GetSymbolInfo(n, cancellationToken))
                        .Select(i => i.Symbol)
                        .Any(IsClassTypeParameter);
                var firstMethod = node.Members
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(m => IsTargetMethod(m));
                if (firstMethod == null)
                {
                    continue;
                }
                var location = node.ChildTokens()
                    .First(t => t.IsKind(SyntaxKind.IdentifierToken))
                    .GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    classSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
