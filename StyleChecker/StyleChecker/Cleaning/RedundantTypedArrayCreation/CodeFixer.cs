namespace StyleChecker.Cleaning.RedundantTypedArrayCreation;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using AceSyntax
    = Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax;
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
        => ImmutableArray.Create(Analyzer.DiagnosticId);

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
        var root = await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (root.FindNodeOfType<ArrayTypeSyntax>(diagnosticSpan)
            is not {} arrayTypeNode
            || arrayTypeNode.Parent is not AceSyntax aceNode)
        {
            return;
        }

        var action = CodeAction.Create(
            title: title,
            createChangedSolution:
                c => Task.Run(() => Replace(document, root, aceNode), c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static Solution Replace(
        Document document, SyntaxNode root, AceSyntax node)
    {
        var solution = document.Project.Solution;
        var newSpecifiers = new[] { node.Type.RankSpecifiers.Last() };
        var newType = SyntaxFactory.ArrayType(
            SyntaxFactory.OmittedTypeArgument(),
            SyntaxFactory.List(newSpecifiers));
        var newNode = node.WithType(newType);
        var newRoot = root.ReplaceNode(node, newNode);
        if (newRoot is null)
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
