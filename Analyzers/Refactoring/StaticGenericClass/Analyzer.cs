namespace Analyzers.Refactoring.StaticGenericClass;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roastery;
using R = Resources;

/// <summary>
/// StaticGenericClass analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "StaticGenericClass";

    private const string Category = Categories.Refactoring;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

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

    private static void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        static bool IsClassTypeParameter(ISymbol s, ISymbol classSymbol)
            => s.Kind is SymbolKind.TypeParameter
                && Symbols.AreEqual(s.ContainingSymbol, classSymbol);

        static Func<ClassDeclarationSyntax, bool> NewIsTarget(
            SemanticModel model, CancellationToken cancellationToken)
        {
            var toSymbol = (ClassDeclarationSyntax node)
                => model.GetDeclaredSymbol(node, cancellationToken);
            var toSymbolInfo = (SyntaxNode node)
                => model.GetSymbolInfo(node, cancellationToken);
            var toFirstMethod = (ClassDeclarationSyntax node, ISymbol symbol)
                =>
            {
                var isTargetMethod = (MethodDeclarationSyntax m)
                    => m.DescendantNodes()
                        .Where(n => n.IsKind(SyntaxKind.IdentifierName))
                        .Select(n => toSymbolInfo(n).Symbol)
                        .FilterNonNullReference()
                        .Any(s => IsClassTypeParameter(s, symbol));
                return node.Members
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(isTargetMethod);
            };
            return node => toSymbol(node) is {} symbol
                && symbol.IsStatic
                && node.TypeParameterList is {} list
                && list.Parameters.Any()
                && toFirstMethod(node, symbol) is not null;
        }

        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        var isTarget = NewIsTarget(model, cancellationToken);
        var root = model.SyntaxTree
            .GetCompilationUnitRoot(context.CancellationToken);
        var all = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(isTarget)
            .ToList();
        foreach (var node in all)
        {
            var location = node.ChildTokens()
                .First(t => t.IsKind(SyntaxKind.IdentifierToken))
                .GetLocation();
            var diagnostic = Diagnostic.Create(
                Rule, location, node.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
