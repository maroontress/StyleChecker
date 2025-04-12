namespace CodeFixes.Spacing.NoSpaceBeforeBrace;

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Analyzers;
using Analyzers.Spacing.NoSpaceBeforeBrace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using R = Resources;

/// <summary>
/// NoSpaceBeforeBrace code fix provider.
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
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(
        CodeFixContext context)
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        var title = localize(nameof(R.FixTitle)).ToString(CompilerCulture);

        var document = context.Document;
        if (await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }
        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;
        var token = root.FindToken(span.Start, findInsideTrivia: true);
        var fixTask = TokenFix.NewTask(
            () => TokenFix.AddSpaceBeforeToken(document, root, token));
        var action = CodeAction.Create(
            title: title,
            createChangedDocument: fixTask,
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }
}
