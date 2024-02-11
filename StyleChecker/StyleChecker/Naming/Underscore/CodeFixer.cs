namespace StyleChecker.Naming.Underscore;

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Maroontress.Extensions;
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
public sealed class CodeFixer : AbstractCodeFixProvider
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
        var title = localize(nameof(R.FixTitle))
            .ToString(CompilerCulture);

        var root = await context
            .Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
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
        var solution = document.Project.Solution;
        var model = await document.GetSemanticModelAsync(cancellationToken)
            .ConfigureAwait(false);
        if (model is null)
        {
            return solution;
        }
        var parent = token.Parent;
        if (parent is null)
        {
            return solution;
        }
        var symbol = model.GetDeclaredSymbol(parent, cancellationToken);
        if (symbol is null)
        {
            return solution;
        }

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
        var newName = string.Concat(array)
            .OrElseIfEmpty("underscore");

        var options = default(SymbolRenameOptions);
        var newSolution = await Renamer.RenameSymbolAsync(
                document.Project.Solution,
                symbol,
                options,
                newName,
                cancellationToken)
            .ConfigureAwait(false);
        return newSolution;
    }
}
