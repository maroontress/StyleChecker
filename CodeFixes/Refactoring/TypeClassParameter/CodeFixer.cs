namespace CodeFixes.Refactoring.TypeClassParameter;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzers;
using Analyzers.Refactoring.TypeClassParameter;
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
public sealed class CodeFixer : AbstractCodeFixProvider
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
        => [Analyzer.DiagnosticId];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        var title = localize(nameof(R.FixTitle))
            .ToString(CompilerCulture);

        var document = context.Document;
        var cancellationToken = context.CancellationToken;
        if (await document.GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        if (root.FindNodeOfType<ParameterSyntax>(diagnosticSpan)
            is not {} node)
        {
            return;
        }

        var action = CodeAction.Create(
            title: title,
            createChangedSolution: c => Replace(document, node, c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
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
        if (await Documents.GetSymbols(document, node, cancellationToken)
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
            solution, document, typeName, cancellationToken);
        var name = methodSymbol.Name;
        var namesakes = GetNamesakes(name, methodSymbol, parameterSymbol)
            .ToList();
        return namesakes.Any()
            ? await kit.GetRenamedSolution(
                namesakes[0], name, allSymbolNameSet, documentId, realNode)
            : await kit.GetNewSolution(parameterSymbol, methodSymbol, root);
    }

    private static IEnumerable<IMethodSymbol> GetNamesakes(
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
                && IsSameTypes(m.Parameters, newParameters));
    }

    private static string? GetTypeName(ISet<string> set)
    {
        var name = "T";
        var all = Enumerable.Range(1, int.MaxValue)
            .Prepend(0)
            .Select(i => $"{name}{i}")
            .Prepend(name)
            .Where(n => !set.Contains(n))
            .FirstOrDefault();
        if (all is not {} found)
        {
            return null;
        }
        set.Add(found);
        return found;
    }

    private static bool IsSameTypes(
        IEnumerable<IParameterSymbol> p1, IEnumerable<IParameterSymbol> p2)
    {
        static ITypeSymbol ToType(IParameterSymbol s) => s.Type;

        static IReadOnlyList<ITypeSymbol> ToList(
                IEnumerable<IParameterSymbol> p)
            => [.. p.Select(ToType)];

        var t1 = ToList(p1);
        var t2 = ToList(p2);
        var n = t1.Count;
        return n == t2.Count
            && Enumerable.Range(0, n)
                .All(k => Symbols.AreEqual(t1[k], t2[k]));
    }
}
