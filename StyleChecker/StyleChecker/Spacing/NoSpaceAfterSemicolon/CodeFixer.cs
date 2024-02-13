namespace StyleChecker.Spacing.NoSpaceAfterSemicolon;

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using R = Resources;

/// <summary>
/// NoSpaceAfterSemicolon code fix provider.
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
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
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
        var span = diagnostic.Location.SourceSpan;
        var token = root.FindToken(span.Start, findInsideTrivia: true);

        var action = CodeAction.Create(
            title: title,
            createChangedDocument: Fix(context.Document, root, token),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private Document FixDocument(
        Document document, SyntaxNode root, SyntaxToken token)
    {
        var list = token.TrailingTrivia;
        var space = SyntaxFactory.ParseTrailingTrivia(" ");
        var newList = list.InsertRange(0, space);
        var newToken = token.WithTrailingTrivia(newList);
        var newRoot = root.ReplaceToken(token, newToken);
        return document.WithSyntaxRoot(newRoot);
    }

    private Func<CancellationToken, Task<Document>> Fix(
        Document document, SyntaxNode root, SyntaxToken token)
    {
        return c => Task.Run(() => FixDocument(document, root, token), c);
    }
}
