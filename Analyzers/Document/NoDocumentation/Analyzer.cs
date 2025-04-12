namespace Analyzers.Document.NoDocumentation;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Analyzers.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roastery;
using R = Resources;

/// <summary>
/// NoDocumentation analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "NoDocumentation";

    private const string Category = Categories.Document;

    private static readonly DiagnosticDescriptor Rule = NewRule();

    private static readonly IReadOnlyCollection<Accessibility> VisibleSet = [
            Accessibility.Public,
            Accessibility.Protected,
            Accessibility.ProtectedOrInternal,
        ];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        ConfigBank.LoadRootConfig(context, StartAction);
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

    private static SyntaxToken? ToMaybeToken(SyntaxNode node)
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
            _ => null,
        };
    }

    private static IEnumerable<ISymbol> AllContainingSymbol(ISymbol top)
    {
        var s = top;
        while (s is not null)
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
            && methodSymbol.AssociatedSymbol is not null;

    private static bool IsMissingDocument(ISymbol symbol)
        => !symbol.IsImplicitlyDeclared
            && !IsAccessor(symbol)
            && IsDocumentVisible(symbol)
            && symbol.DeclaringSyntaxReferences
                .Max(r => r.SyntaxTree.Options.DocumentationMode)
                >= DocumentationMode.Diagnose
            && IsNullOrEmpty(symbol.GetDocumentationCommentXml());

    private static bool IsNullOrEmpty(string? s)
        => string.IsNullOrEmpty(s);

    private static bool Contains(IImmutableSet<string> set, AttributeData d)
    {
        return d.AttributeClass is {} clazz
            && set.Contains(clazz.ToString());
    }

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context,
        ConfigPod pod)
    {
        var config = pod.RootConfig.NoDocumentation;
        var ignoringSet = config.GetAttributes()
            .ToImmutableHashSet();
        var inclusivelyIgnoringSet = config.GetInclusiveAttributes()
            .ToImmutableHashSet();
        AnalyzeModel(context, ignoringSet, inclusivelyIgnoringSet);
    }

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context,
        IImmutableSet<string> ignoringSet,
        IImmutableSet<string> inclusivelyIgnoringSet)
    {
        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        var root = model.SyntaxTree
            .GetCompilationUnitRoot(cancellationToken);
        var all = root.DescendantNodes();

        INamedTypeSymbol? BaseTypeSymbol(BaseTypeDeclarationSyntax s)
            => model.GetDeclaredSymbol(s);

        INamedTypeSymbol? DelegateSymbol(DelegateDeclarationSyntax s)
            => model.GetDeclaredSymbol(s);

        static IEnumerable<INamedTypeSymbol> ToSymbol<T>(
            IEnumerable<SyntaxNode> a,
            Func<T, INamedTypeSymbol?> f)
        {
            return a.OfType<T>()
                .Select(f)
                .FilterNonNullReference();
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

        IEnumerable<SyntaxToken> ToFirstToken(IEnumerable<SyntaxReference> all)
        {
            return all.Where(r => r.SyntaxTree == model.SyntaxTree)
                .Select(r => ToMaybeToken(r.GetSyntax()))
                .FilterNonNullValue()
                .Take(1);
        }

        var allTokens = declaraions.Concat(
                declaraions.SelectMany(s => s.GetMembers()))
            .ToRigidSet()
            .Where(NeedsDiagnostics)
            .SelectMany(s => ToFirstToken(s.DeclaringSyntaxReferences));

        foreach (var firstToken in allTokens)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                firstToken.GetLocation(),
                firstToken);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void StartAction(
        CompilationStartAnalysisContext context, ConfigPod pod)
    {
        context.RegisterSemanticModelAction(c => AnalyzeModel(c, pod));
    }
}
