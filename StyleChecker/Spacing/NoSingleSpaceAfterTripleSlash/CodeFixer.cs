namespace StyleChecker.Spacing.NoSingleSpaceAfterTripleSlash;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Analyzers;
using Analyzers.Spacing.NoSingleSpaceAfterTripleSlash;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using R = Resources;

/// <summary>
/// NoSingleSpaceAfterTripleSlash code fix provider.
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
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        static string Unify(string t)
            => $" {t.TrimStart()}";

        static string Prepend(string t)
            => $" {t}";

        static CodeAction NewAction(string fixTitle, Func<Document> toDocument)
            => CodeAction.Create(
                title: fixTitle,
                createChangedDocument: c => Task.Run(toDocument, c),
                equivalenceKey: fixTitle);

        var localize = Localizers.Of<R>(R.ResourceManager);
        var insertFixTitle = localize(nameof(R.InsertFixTitle))
            .ToString(CompilerCulture);
        var replaceFixTitle = localize(nameof(R.ReplaceFixTitle))
            .ToString(CompilerCulture);

        (string Title, Func<string, string> Replacer)
                ToTuple(XmlTextSyntax node)
            => (node.Parent is DocumentationCommentTriviaSyntax)
                ? (replaceFixTitle, Unify)
                : (insertFixTitle, Prepend);

        var document = context.Document;
        if (await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }
        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;
        var token = root.FindToken(span.Start, findInsideTrivia: true);
        if (token == default)
        {
            return;
        }

        CodeAction ToReplaceTokenAction(XmlTextSyntax node)
        {
            var (title, replacer) = ToTuple(node);
            return NewAction(
                title,
                () => Replace(document, root, token, replacer));
        }

        CodeAction ToInsertTextAction() => NewAction(
            insertFixTitle,
            () => InsertText(document, root, token));

        CodeAction ToInsertTriviaAction() => NewAction(
            insertFixTitle,
            () => InsertTrivia(document, root, token));

        CodeAction ToReplaceTriviaAction(SyntaxTrivia trivia) => NewAction(
            replaceFixTitle,
            () => ReplaceTrivia(document, root, token, trivia));

        IEnumerable<CodeAction> ToActions()
        {
            var parent = token.Parent;
            if (parent is XmlCDataSectionSyntax)
            {
                return [ToInsertTextAction()];
            }
            if (token.IsKind(SyntaxKind.XmlTextLiteralToken)
                && parent is XmlTextSyntax node)
            {
                return [ToReplaceTokenAction(node)];
            }
            var trivias = token.LeadingTrivia;
            var index = trivias.IndexOf(
                SyntaxKind.DocumentationCommentExteriorTrivia);
            if (trivias.Count == index + 1)
            {
                return [ToInsertTriviaAction()];
            }
            var nextTrivia = trivias[index + 1];
            return nextTrivia.IsKind(SyntaxKind.WhitespaceTrivia)
                ? [ToReplaceTriviaAction(nextTrivia)]
                : [];
        }

        var all = ToActions();
        foreach (var action in all)
        {
            context.RegisterCodeFix(action, diagnostic);
        }
    }

    private static Document Replace(
        Document document,
        SyntaxNode root,
        SyntaxToken token,
        Func<string, string> toNewText)
    {
        var text = token.Text;
        var newText = toNewText(text);
        var newToken = SyntaxFactory.Token(
            token.LeadingTrivia,
            SyntaxKind.XmlTextLiteralToken,
            newText,
            newText,
            token.TrailingTrivia);
        var newRoot = root.ReplaceToken(token, newToken);
        return document.WithSyntaxRoot(newRoot);
    }

    private static Document InsertText(
        Document document, SyntaxNode root, SyntaxToken token)
    {
        var newToken = SyntaxFactory.Token(
            token.LeadingTrivia,
            token.Kind(),
            " " + token.Text,
            " " + token.ValueText,
            token.TrailingTrivia);
        var newRoot = root.ReplaceToken(token, newToken);
        return document.WithSyntaxRoot(newRoot);
    }

    private static Document InsertTrivia(
        Document document, SyntaxNode root, SyntaxToken token)
    {
        var trivias = token.LeadingTrivia;
        var newTrivias = trivias.Add(
            SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "));
        var newToken = token.WithLeadingTrivia(newTrivias);
        var newRoot = root.ReplaceToken(token, newToken);
        return document.WithSyntaxRoot(newRoot);
    }

    private static Document ReplaceTrivia(
        Document document,
        SyntaxNode root,
        SyntaxToken token,
        SyntaxTrivia whiteSpaceTrivia)
    {
        var trivias = token.LeadingTrivia;
        var newWhiteSpaceTrivia = SyntaxFactory.SyntaxTrivia(
            SyntaxKind.WhitespaceTrivia, " ");
        var newTrivias = trivias.Replace(
            whiteSpaceTrivia, newWhiteSpaceTrivia);
        var newToken = token.WithLeadingTrivia(newTrivias);
        var newRoot = root.ReplaceToken(token, newToken);
        return document.WithSyntaxRoot(newRoot);
    }
}
