namespace StyleChecker.Refactoring
{
    using System;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Formatting;

    /// <summary>
    /// Provides abstraction of the simple CodeFix providers using Reviser
    /// class.
    /// </summary>
    public abstract class AbstractRevisingCodeFixer : CodeFixProvider
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
                return Localize(key).ToString(CultureInfo.CurrentCulture);
            }

            var document = context.Document;
            var root = await document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics[0];
            var span = diagnostic.Location.SourceSpan;

            foreach (var kit in ReviserKitList)
            {
                if (!(kit.FindReviser(root, span) is {} r))
                {
                    continue;
                }

                Task<Document> NewTask(CancellationToken c)
                    => Task.Run(() => Replace(document, r, c));

                var title = FixTitle(kit.Key);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: NewTask,
                        equivalenceKey: title),
                    diagnostic);
            }
        }

        private static Document Replace(
            Document document,
            Reviser bar,
            CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;
            var root = bar.Root;
            var node = bar.Node;
            var newNode = bar.NewNode;
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
               workspace.Options,
               cancellationToken);
            return document.WithSyntaxRoot(formattedNode);
        }
    }
}
