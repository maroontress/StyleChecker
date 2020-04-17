namespace StyleChecker.Refactoring.NotDesignedForExtension
{
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using R = Resources;

    /// <summary>
    /// NotDesignedForExtension analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "NotDesignedForExtension";

        private const string Category = Categories.Refactoring;
        private static readonly DiagnosticDescriptor Rule = NewRule();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        private protected override void Register(AnalysisContext context)
        {
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
            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree
                .GetCompilationUnitRoot(cancellationToken);
            var allMembers = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(n => model.GetDeclaredSymbol(n, cancellationToken))
                .OfType<INamedTypeSymbol>()
                .Where(s => !s.IsSealed && !s.IsStatic)
                .SelectMany(s => s.GetMembers());

            var allMethods = allMembers.OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary
                    && ((m.IsVirtual && (!m.ReturnsVoid || !IsEmpty(m)))
                        || (m.IsOverride && !m.IsSealed)))
                .Select(m => (ToToken(m), R.Method));
            var allProperties = allMembers.OfType<IPropertySymbol>()
                .Where(p => p.IsVirtual || (p.IsOverride && !p.IsSealed))
                .Select(p => (ToToken(p), R.Property));
            var all = allMethods.Concat(allProperties);

            foreach (var (token, format) in all)
            {
                if (token == default)
                {
                    continue;
                }
                var location = token.GetLocation();
                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    string.Format(CultureInfo.CurrentCulture, format, token));
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static SyntaxToken ToToken(IMethodSymbol m)
        {
            var node = ToNode<MethodDeclarationSyntax>(m);
            return node is null ? default : node.Identifier;
        }

        private static SyntaxToken ToToken(IPropertySymbol m)
        {
            var node = ToNode<PropertyDeclarationSyntax>(m);
            return node is null ? default : node.Identifier;
        }

        private static T? ToNode<T>(ISymbol m)
            where T : SyntaxNode
        {
            var reference = m.DeclaringSyntaxReferences.FirstOrDefault();
            return (reference is null) ? null : reference.GetSyntax() as T;
        }

        private static bool IsEmpty(IMethodSymbol m)
        {
            static bool HasNoBlock(MethodDeclarationSyntax n)
                => n.Body is null
                    && n.ExpressionBody is null;

            static bool HasAnEmptyBlock(MethodDeclarationSyntax n)
                => n.Body is BlockSyntax block
                    && !block.Statements.Any();

            var node = ToNode<MethodDeclarationSyntax>(m);
            return !(node is null)
                && (HasNoBlock(node) || HasAnEmptyBlock(node));
        }
    }
}
