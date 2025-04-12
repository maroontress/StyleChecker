namespace CodeFixes.Refactoring.EmptyArrayCreation;

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzers;
using Analyzers.Refactoring.EmptyArrayCreation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using AceSyntax
    = Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax;
using R = Resources;

/// <summary>
/// EmptyArrayCreation CodeFix provider.
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
        var title = localize(nameof(R.FixTitle)).ToString(CompilerCulture);

        if (await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        if (root.FindNodeOfType<AceSyntax>(diagnosticSpan) is not {} node)
        {
            return;
        }

        var action = CodeAction.Create(
            title: title,
            createChangedSolution: c => Replace(context.Document, node, c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Solution> Replace(
        Document document,
        AceSyntax node,
        CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        if (await document.GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return solution;
        }
        var type = node.Type.ElementType;
        var newNode = SyntaxFactory.ParseExpression(
            $"System.Array.Empty<{type}>()");
        if (root.ReplaceNode(node, newNode) is not {} newRoot)
        {
            return solution;
        }
        var workspace = solution.Workspace;
        var formattedNode = Formatter.Format(
           newRoot,
           Formatter.Annotation,
           workspace,
           workspace.Options,
           cancellationToken);
        return solution.WithDocumentSyntaxRoot(document.Id, formattedNode);
    }
}
