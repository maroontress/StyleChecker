namespace CodeFixes.Naming.SingleTypeParameter;

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Analyzers;
using Analyzers.Naming.SingleTypeParameter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Rename;
using R = Resources;

/// <summary>
/// SingleTypeParameter code fix provider.
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

        var document = context.Document;
        if (await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var token = root.FindToken(diagnosticSpan.Start);
        var action = CodeAction.Create(
            title: title,
            createChangedSolution: c => ReplaceWithT(document, token, c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Solution> ReplaceWithT(
        Document document,
        SyntaxToken token,
        CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        if (await document.GetSemanticModelAsync(cancellationToken)
            .ConfigureAwait(false) is not {} model
            || token.Parent is not {} parent
            || model.GetDeclaredSymbol(parent, cancellationToken)
                is not {} symbol)
        {
            return solution;
        }
        var options = default(SymbolRenameOptions);
        return await Renamer.RenameSymbolAsync(
                solution, symbol, options, "T", cancellationToken)
            .ConfigureAwait(false);
    }
}
