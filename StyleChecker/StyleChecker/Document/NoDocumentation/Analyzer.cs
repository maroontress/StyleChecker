namespace StyleChecker.Document.NoDocumentation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleChecker.Settings;
    using R = Resources;

    /// <summary>
    /// NoDocumentation analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "NoDocumentation";

        private const string Category = Categories.Document;
        private static readonly DiagnosticDescriptor Rule = NewRule();
        private static readonly ImmutableHashSet<Accessibility> VisibleSet
            = new HashSet<Accessibility>()
            {
                Accessibility.Public,
                Accessibility.Protected,
                Accessibility.ProtectedOrInternal,
            }.ToImmutableHashSet();

        private ConfigPod pod;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            void StartAction(CompilationStartAnalysisContext c, ConfigPod p)
            {
                pod = p;
                c.RegisterSemanticModelAction(AnalyzeModel);
            }

            ConfigBank.LoadRootConfig(context, StartAction);
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
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

        private static SyntaxToken ToToken(SyntaxNode node)
        {
            /*
                CSharpSyntaxNode
                + [X] VariableDeclaratorSyntax (0)
                | + <= FieldDeclarationSyntax
                | + <= EventFieldDeclarationSyntax
                + MemberDeclarationSyntax
                  + BaseMethodDeclarationSyntax
                  | + [X] ConstructorDeclarationSyntax (1)
                  | + [X] ConversionOperatorDeclarationSyntax (2)
                  | + [X] DestructorDeclarationSyntax (3)
                  | + [X] MethodDeclarationSyntax (4)
                  | + [X] OperatorDeclarationSyntax (5)
                  + BasePropertyDeclarationSyntax
                  | + [X] EventDeclarationSyntax (6)
                  | + [X] IndexerDeclarationSyntax (7)
                  | + [X] PropertyDeclarationSyntax (8)
                  + [X] BaseTypeDeclarationSyntax (9)
                  | + [-] EnumDeclarationSyntax
                  | + TypeDeclarationSyntax
                  |   + [-] ClassDeclarationSyntax
                  |   + [-] InterfaceDeclarationSyntax
                  |   + [-] StructDeclarationSyntax
                  + [X] DelegateDeclarationSyntax (10)
                  + [X] EnumMemberDeclarationSyntax (11)
            */
            return node switch
            {
                BaseTypeDeclarationSyntax s => s.Identifier,
                DelegateDeclarationSyntax s => s.Identifier,
                ConstructorDeclarationSyntax s => s.Identifier,
                DestructorDeclarationSyntax s => s.Identifier,
                VariableDeclaratorSyntax s => s.Identifier,
                PropertyDeclarationSyntax s => s.Identifier,
                IndexerDeclarationSyntax s => s.ThisKeyword,
                MethodDeclarationSyntax s => s.Identifier,
                OperatorDeclarationSyntax s => s.OperatorToken,
                ConversionOperatorDeclarationSyntax s
                    => s.ImplicitOrExplicitKeyword,
                EnumMemberDeclarationSyntax s => s.Identifier,
                EventDeclarationSyntax s => s.Identifier,
                _ => default
            };
        }

        private static IEnumerable<ISymbol> AllContainingSymbol(ISymbol top)
        {
            var s = top;
            while (!(s is null))
            {
                yield return s;
                s = s.ContainingType;
            }
        }

        private static bool IsDocumentVisible(ISymbol symbol)
        {
            return AllContainingSymbol(symbol)
                .Select(s => s.DeclaredAccessibility)
                .All(a => VisibleSet.Contains(a));
        }

        private static bool IsAccessor(ISymbol symbol)
            => symbol is IMethodSymbol methodSymbol
                && !(methodSymbol.AssociatedSymbol is null);

        private static bool IsMissingDocument(ISymbol symbol)
            => !symbol.IsImplicitlyDeclared
                && !IsAccessor(symbol)
                && IsDocumentVisible(symbol)
                && symbol.DeclaringSyntaxReferences
                    .Select(r => r.SyntaxTree.Options.DocumentationMode)
                    .Max() >= DocumentationMode.Diagnose
                && IsNullOrEmpty(symbol.GetDocumentationCommentXml());

        private static bool IsNullOrEmpty(string s)
            => string.IsNullOrEmpty(s);

        private static bool Contains(ICollection<string> set, AttributeData d)
            => set.Contains(d.AttributeClass.ToString());

        private void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var config = pod.RootConfig.NoDocumentation;
            var ignoringSet = config.GetAttributes()
                .ToImmutableHashSet();
            var inclusivelyIgnoringSet = config.GetInclusiveAttributes()
                .ToImmutableHashSet();

            var cancellationToken = context.CancellationToken;
            var model = context.SemanticModel;
            var root = model.SyntaxTree
                .GetCompilationUnitRoot(cancellationToken);
            var all = root.DescendantNodes();

            INamedTypeSymbol BaseTypeSymbol(BaseTypeDeclarationSyntax s)
                => model.GetDeclaredSymbol(s);

            INamedTypeSymbol DelegateSymbol(DelegateDeclarationSyntax s)
                => model.GetDeclaredSymbol(s);

            IEnumerable<INamedTypeSymbol> ToSymbol<T>(
                IEnumerable<SyntaxNode> a,
                Func<T, INamedTypeSymbol> f)
            {
                return a.OfType<T>().Select(f);
            }

            var declaraions = ToSymbol<BaseTypeDeclarationSyntax>(
                    all, BaseTypeSymbol)
                .Concat(ToSymbol<DelegateDeclarationSyntax>(
                    all, DelegateSymbol));

            bool IsIncluded(AttributeData d)
                => Contains(ignoringSet, d);

            bool CanIgnore(ISymbol s)
                => s.GetAttributes().Any(IsIncluded);

            bool IsIncludedInclusively(AttributeData d)
                => Contains(inclusivelyIgnoringSet, d);

            bool CanIgnoreInclusively(ISymbol s)
                => AllContainingSymbol(s)
                    .SelectMany(e => e.GetAttributes())
                    .Any(IsIncludedInclusively);

            bool NeedsDiagnostics(ISymbol s)
                => !CanIgnore(s)
                    && !CanIgnoreInclusively(s)
                    && IsMissingDocument(s);

            var allSymbols = declaraions
                .Concat(declaraions.SelectMany(s => s.GetMembers()))
                .ToImmutableHashSet()
                .Where(NeedsDiagnostics);

            foreach (var symbol in allSymbols)
            {
                var firstNode = symbol.DeclaringSyntaxReferences
                    .Where(r => r.SyntaxTree == model.SyntaxTree)
                    .Select(r => r.GetSyntax())
                    .FirstOrDefault();
                if (firstNode == null)
                {
                    continue;
                }
                var firstToken = ToToken(firstNode);
                var diagnostic = Diagnostic.Create(
                    Rule,
                    firstToken.GetLocation(),
                    firstToken);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
