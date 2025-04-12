namespace CodeFixes.Refactoring;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

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
    /// Gets the function that returns the localized string associated with the
    /// specified key.
    /// </summary>
    protected abstract Func<string, LocalizableString> Localize { get; }

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(
        CodeFixContext context)
    {
        static Func<ReviserKit, IEnumerable<CodeAction>> CodeActionsSupplier(
            Document document,
            Func<string, string> fixTitle,
            SyntaxNode root,
            TextSpan span)
        {
            CodeAction ToCodeAction(string title, Reviser riviser)
                => CodeAction.Create(
                    title: title,
                    createChangedDocument:
                        c => Task.Run(() => Replace(document, riviser), c),
                    equivalenceKey: title);

            return kit => kit.FindReviser(root, span) is not {} reviser
                ? []
                : [ToCodeAction(fixTitle(kit.Key), reviser)];
        }

        string FixTitle(string key)
            => Localize(key).ToString(CompilerCulture);

        var document = context.Document;
        if (await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }
        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;

        var toCodeActions = CodeActionsSupplier(
            document, FixTitle, root, span);
        var all = ReviserKitList.SelectMany(toCodeActions)
            .ToList();
        foreach (var action in all)
        {
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private static Document Replace(Document document, Reviser reviser)
    {
        var solution = document.Project.Solution;
        var root = reviser.Root;
        var node = reviser.Node;
        var newNode = reviser.NewNode;
        if (root.ReplaceNode(node, newNode) is not {} newRoot)
        {
            return document;
        }
        var workspace = solution.Workspace;
        var formattedNode = Formatter.Format(
           newRoot, Formatter.Annotation, workspace, workspace.Options);
        return document.WithSyntaxRoot(formattedNode);
    }
}
