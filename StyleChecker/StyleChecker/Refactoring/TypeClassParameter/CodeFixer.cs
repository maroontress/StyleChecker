namespace StyleChecker.Refactoring.TypeClassParameter;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using R = Resources;

/// <summary>
/// TypeClassParameter CodeFix provider.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
[Shared]
public sealed class CodeFixer : CodeFixProvider
{
    private const string ParamName = "param";
    private const string TypeparamName = "typeparam";

    private const SyntaxKind MldcTriviaKind
        = SyntaxKind.MultiLineDocumentationCommentTrivia;

    private const SyntaxKind SldcTriviaKind
        = SyntaxKind.SingleLineDocumentationCommentTrivia;

    private static readonly Func<SimpleNameSyntax, ExpressionSyntax>
        Identity = s => s;

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(Analyzer.DiagnosticId);

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(
        CodeFixContext context)
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        var title = localize(nameof(R.FixTitle))
            .ToString(CultureInfo.CurrentCulture);

        var document = context.Document;
        var cancellationToken = context.CancellationToken;
        var root = await document.GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNodeOfType<ParameterSyntax>(diagnosticSpan);
        if (node is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedSolution: c => Replace(document, node, c),
                equivalenceKey: title),
            diagnostic);
    }

    private static async Task<Solution> Replace(
        Document document,
        ParameterSyntax node,
        CancellationToken cancellationToken)
    {
        var solution = await Replace2(document, node, cancellationToken)
            .ConfigureAwait(false);
        return solution ?? document.Project.Solution;
    }

    private static async Task<Solution?> Replace2(
        Document realDocument,
        ParameterSyntax realNode,
        CancellationToken cancellationToken)
    {
        var documentId = realDocument.Id;
        var realRoot = realNode.SyntaxTree
            .GetRoot(cancellationToken);
        var solution = realDocument.Project
            .Solution
            .WithDocumentSyntaxRoot(
                documentId, realRoot.TrackNodes(realNode));
        if (solution.GetDocument(documentId) is not {} document
            || await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false) is not {} root)
        {
            return null;
        }
        var node = root.FindNode(realNode.Span);
        if (await Documents.GetSymbols(document, cancellationToken, node)
                .ConfigureAwait(false) is not {} symbols
            || node.Parent is not {} parent)
        {
            return null;
        }
        var (model, parameterSymbol, methodSymbol) = symbols;
        var allSymbolNameSet = new HashSet<string>(
            model.LookupSymbols(parent.SpanStart)
                .Select(s => s.Name));
        if (GetTypeName(allSymbolNameSet) is not {} typeName)
        {
            return null;
        }
        var kit = new SolutionKit(
            cancellationToken, solution, document, typeName);
        var name = methodSymbol.Name;
        var namesakes = GetNamesakes(name, methodSymbol, parameterSymbol);
        return namesakes.Any()
            ? await kit.GetRenamedSolution(
                namesakes[0], name, allSymbolNameSet, documentId, realNode)
            : await kit.GetNewSolution(parameterSymbol, methodSymbol, root);
    }

    private static IMethodSymbol[] GetNamesakes(
        string name, IMethodSymbol method, ISymbol parameter)
    {
        if (method.ContainingSymbol is not INamedTypeSymbol namedType)
        {
            return [];
        }
        var newTypeParameterLength = method.TypeParameters.Length + 1;
        var newParameters = method.Parameters
            .Where(p => !Symbols.AreEqual(p, parameter));
        return namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.Name == name
                && m.TypeParameters.Length == newTypeParameterLength
                && IsSameTypes(m.Parameters, newParameters))
            .ToArray();
    }

    private static string? GetTypeName(ISet<string> set)
    {
        var name = "T";
        if (!set.Contains(name))
        {
            return name;
        }
        for (var k = 0; k >= 0; ++k)
        {
            var n = $"{name}{k}";
            if (!set.Contains(n))
            {
                set.Add(n);
                return n;
            }
        }
        return null;
    }

    private static bool IsSameTypes(
        IEnumerable<IParameterSymbol> p1, IEnumerable<IParameterSymbol> p2)
    {
        static ITypeSymbol ToType(IParameterSymbol s) => s.Type;

        var t1 = p1.Select(ToType)
            .ToList();
        var t2 = p2.Select(ToType)
            .ToList();
        var n = t1.Count;
        return n == t2.Count
            && Enumerable.Range(0, n)
                .All(k => Symbols.AreEqual(t1[k], t2[k]));
    }
}
