namespace StyleChecker.Naming.Underscore
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Rename;
    using R = Resources;

    /// <summary>
    /// Underscore code fix provider.
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
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            var title = localize(nameof(R.FixTitle)).ToString();

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var token = root.FindToken(diagnosticSpan.Start);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution:
                        c => RemoveUnderscore(
                            context.Document, token, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Solution> RemoveUnderscore(
            Document document,
            SyntaxToken token,
            CancellationToken cancellationToken)
        {
            var s = token.ToString();
            var array = s.Split(
                new[] { '_' },
                StringSplitOptions.RemoveEmptyEntries);
            var n = array.Length;
            for (var k = 1; k < n; ++k)
            {
                var component = array[k];
                array[k] = char.ToUpper(component[0]) + component.Substring(1);
            }
            var newName = string.Concat(array);
            if (newName.Length == 0)
            {
                newName = "underscore";
            }

            var semanticModel = await document.GetSemanticModelAsync(
                cancellationToken).ConfigureAwait(false);
            var symbol = semanticModel.GetDeclaredSymbol(
                token.Parent, cancellationToken);

            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(
                    document.Project.Solution,
                    symbol,
                    newName,
                    optionSet,
                    cancellationToken)
                .ConfigureAwait(false);

            return newSolution;
        }
    }
}
