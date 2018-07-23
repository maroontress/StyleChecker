using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace StyleChecker
{
    [ExportCodeFixProvider(LanguageNames.CSharp,
        Name = nameof(UnderscoreCodeFixProvider)), Shared]
    public class UnderscoreCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove all underscores";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(UnderscoreAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
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
                        c => RemoveUnderscore(context.Document,
                            token, c), 
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Solution> RemoveUnderscore(Document document, 
            SyntaxToken token,
            CancellationToken cancellationToken)
        {
            var s = token.ToString();
            var array = s.Split(new char[] { '_' },
                StringSplitOptions.RemoveEmptyEntries);
            var n = array.Length;
            for (var k = 1; k < n; ++k)
            {
                var component = array[k];
                array[k] = Char.ToUpper(component[0]) + component.Substring(1);
            }
            var newName = string.Join("", array);

            var semanticModel = await document.GetSemanticModelAsync(
                cancellationToken);
            var symbol = semanticModel.GetDeclaredSymbol(token.Parent,
                cancellationToken);

            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(
                document.Project.Solution, symbol, newName, optionSet,
                cancellationToken)
                .ConfigureAwait(false);

            return newSolution;
        }
    }
}
