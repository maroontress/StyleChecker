namespace StyleChecker.Refactoring;

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

/// <summary>
/// Provides abstraction of the simple CodeFix providers using Reviser class.
/// </summary>
public abstract class AbstractRevisingCodeFixer : AbstractCodeFixProvider
{
    /// <summary>
    /// Gets the list of <see cref="ReviserKit"/>s.
    /// </summary>
    protected abstract ImmutableList<ReviserKit> ReviserKitList { get; }

    /// <summary>
    /// Gets the function that returns the localized string associated with
    /// the specified key.
    /// </summary>
    protected abstract Func<string, LocalizableString> Localize { get; }

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(
        CodeFixContext context)
    {
        string FixTitle(string key)
        {
            return Localize(key).ToString(CompilerCulture);
        }

        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;

        foreach (var kit in ReviserKitList)
        {
            if (kit.FindReviser(root, span) is not {} reviser)
            {
                continue;
            }

            Task<Document> NewTask(CancellationToken c)
                => Task.Run(() => Replace(document, reviser), c);

            var title = FixTitle(kit.Key);
            var action = CodeAction.Create(
                title: title,
                createChangedDocument: NewTask,
                equivalenceKey: title);
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private static Document Replace(Document document, Reviser reviser)
    {
        var solution = document.Project.Solution;
        var root = reviser.Root;
        var node = reviser.Node;
        var newNode = reviser.NewNode;
        var newRoot = root.ReplaceNode(node, newNode);
        if (newRoot is null)
        {
            return document;
        }
        var workspace = solution.Workspace;
        var formattedNode = Formatter.Format(
           newRoot,
           Formatter.Annotation,
           workspace,
           workspace.Options);
        return document.WithSyntaxRoot(formattedNode);
    }
}
