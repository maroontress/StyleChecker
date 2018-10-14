namespace StyleChecker.Refactoring.StaticGenericClass
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
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
            var all = new List<SyntaxNode>();
            all.AddRange(root.DescendantNodes()
                .Where(n => IsStaticGenericClass(context, n))
                .AsEnumerable());
            if (all.Count == 0)
            {
                return;
            }

            var cancellationToken = context.CancellationToken;
            foreach (var node in all)
            {
                var classSymbol = model.GetDeclaredSymbol(node, cancellationToken);
                bool IsClassTypeParameter(ISymbol s)
                {
                    return s.Kind == SymbolKind.TypeParameter
                       && s.ContainingSymbol == classSymbol;
                }
                bool IsTargetMethod(SyntaxNode m)
                {
                    return m.DescendantNodes()
                        .Where(n => n.IsKind(SyntaxKind.IdentifierName))
                        .Select(n => model.GetSymbolInfo(n, cancellationToken))
                        .Select(i => i.Symbol)
                        .FirstOrDefault(IsClassTypeParameter) != null;
                }
                var methodList = node.ChildNodes()
                    .FirstOrDefault(n => n.IsKind(SyntaxKind.MethodDeclaration)
                        && IsTargetMethod(n));
                if (methodList == null)
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

        private static bool IsStaticGenericClass(
            SemanticModelAnalysisContext context,
            SyntaxNode node)
        {
            if (!node.IsKind(SyntaxKind.ClassDeclaration))
            {
                return false;
            }
            var childTokens = node.ChildTokens();
            var isStatic = childTokens.FirstOrDefault(
               t => t.IsKind(SyntaxKind.StaticKeyword));
            if (isStatic == null)
            {
                return false;
            }
            var childNodes = node.ChildNodes();
            var firstTypeParameter = childNodes.FirstOrDefault(
                n => n.IsKind(SyntaxKind.TypeParameterList));
            if (firstTypeParameter == null)
            {
                return false;
            }
            var firstMethod = childNodes.FirstOrDefault(
                n => n.IsKind(SyntaxKind.MethodDeclaration));
            return firstMethod != null;
        }
    }
}
