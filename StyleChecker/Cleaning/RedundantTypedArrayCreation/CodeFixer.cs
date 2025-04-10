namespace StyleChecker.Cleaning.RedundantTypedArrayCreation;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Analyzers;
using Analyzers.Cleaning.RedundantTypedArrayCreation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using R = Resources;

/// <summary>
/// RedundantTypedArrayCreation CodeFix provider.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
[Shared]
public sealed class CodeFixer : AbstractCodeFixProvider
{
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
        if (await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (root.FindNodeOfType<ArrayTypeSyntax>(diagnosticSpan)
                is not {} arrayTypeNode
            || arrayTypeNode.Parent is not ArrayCreationExpressionSyntax node)
        {
            return;
        }

        var action = CodeAction.Create(
            title: title,
            createChangedSolution:
                c => Task.Run(() => Replace(document, root, node), c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static Solution Replace(
        Document document,
        SyntaxNode root,
        ArrayCreationExpressionSyntax node)
    {
        var solution = document.Project.Solution;
        var lastRankSpecifiers = node.Type.RankSpecifiers.Last();
        var childTokens = lastRankSpecifiers.ChildTokens()
            .ToList();
        var tokenCount = childTokens.Count;
        if (tokenCount < 2
            || node.Initializer is not {} initializer)
        {
            return solution;
        }
        var commas = SyntaxFactory.TokenList(
            childTokens.Skip(1).Take(tokenCount - 2));
        var keyword = node.NewKeyword;
        var newKeyword = keyword.TrailingTrivia
                .All(t => t.IsKind(SyntaxKind.WhitespaceTrivia))
            ? keyword.WithoutTrivia().WithLeadingTrivia(keyword.LeadingTrivia)
            : keyword;
        var newNode = SyntaxFactory.ImplicitArrayCreationExpression(
            newKeyword,
            childTokens[0],
            commas,
            childTokens[tokenCount - 1],
            initializer);
        if (root.ReplaceNode(node, newNode) is not {} newRoot)
        {
            return solution;
        }
        var workspace = solution.Workspace;
        var formattedNode = Formatter.Format(
           newRoot,
           Formatter.Annotation,
           workspace,
           workspace.Options);
        return solution.WithDocumentSyntaxRoot(document.Id, formattedNode);
    }
}
