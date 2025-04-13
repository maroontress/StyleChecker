namespace StyleChecker.CodeFixes.Naming.Underscore;

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Maroontress.Roastery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Rename;
using StyleChecker.Analyzers;
using StyleChecker.Analyzers.Naming.Underscore;
using StyleChecker.CodeFixes;
using R = Resources;

/// <summary>
/// Underscore code fix provider.
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
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(
        CodeFixContext context)
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        var title = localize(nameof(R.FixTitle))
            .ToString(CompilerCulture);

        if (await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var token = root.FindToken(diagnosticSpan.Start);

        var action = CodeAction.Create(
            title: title,
            createChangedSolution:
                c => RemoveUnderscore(context.Document, token, c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Solution> RemoveUnderscore(
        Document document,
        SyntaxToken token,
        CancellationToken cancellationToken)
    {
        var solution = document.Project
            .Solution;
        if (await document.GetSemanticModelAsync(cancellationToken)
                .ConfigureAwait(false) is not {} model
            || token.Parent is not {} parent
            || model.GetDeclaredSymbol(parent, cancellationToken)
                is not {} symbol)
        {
            return solution;
        }

        static string NewName(string[] array, int capacity)
        {
            if (array.Length is 0)
            {
                return "";
            }
            var b = new StringBuilder(capacity);
            b.Append(array[0]);
            foreach (var component in array.Skip(1))
            {
                b.Append(char.ToUpper(component[0]))
                    .Append(component.Substring(1));
            }
            return b.ToString();
        }

        var s = token.ToString();
        var array = s.Split(['_'], StringSplitOptions.RemoveEmptyEntries);
        var newName = NewName(array, s.Length)
            .OrElseIfEmpty("underscore");

        var options = default(SymbolRenameOptions);
        return await Renamer.RenameSymbolAsync(
                solution, symbol, options, newName, cancellationToken)
            .ConfigureAwait(false);
    }
}
