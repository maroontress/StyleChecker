namespace StyleChecker.Naming.SingleTypeParameter;

using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
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
        var title = localize(nameof(R.FixTitle))
            .ToString(CultureInfo.CurrentCulture);

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
                    c => ReplaceWithT(
                        context.Document, token, c),
                equivalenceKey: title),
            diagnostic);
    }

    private async Task<Solution> ReplaceWithT(
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
        var optionSet = solution.Workspace.Options;
        var newSolution = await Renamer.RenameSymbolAsync(
                document.Project.Solution,
                symbol,
                "T",
                optionSet,
                cancellationToken)
            .ConfigureAwait(false);
        return newSolution;
    }
}
