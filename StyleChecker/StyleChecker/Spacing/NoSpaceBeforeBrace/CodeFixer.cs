namespace StyleChecker.Spacing.NoSpaceBeforeBrace
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using R = Resources;

    /// <summary>
    /// NoSpaceBeforeBrace code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
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

            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics[0];
            var span = diagnostic.Location.SourceSpan;
            var token = root.FindToken(span.Start, findInsideTrivia: true);

            Task<Document> FixTask(CancellationToken c)
                => TokenFix.AddSpaceBeforeToken(context.Document, token);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: FixTask,
                    equivalenceKey: title),
                diagnostic);
        }
    }
}
