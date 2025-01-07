namespace StyleChecker.Refactoring.NoUsingDeclaration;

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using R = Resources;

/// <summary>
/// NoUsingDeclaration CodeFix provider.
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
        string FixTitle(string key)
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            return localize(key).ToString(CompilerCulture);
        }

        var document = context.Document;
        if (await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }
        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;
        if (root.FindNodeOfType<TypeSyntax>(span) is not {} typeNode
            || typeNode.Parent is not VariableDeclarationSyntax declNode
            || declNode.Parent is not LocalDeclarationStatementSyntax node
            || node.UsingKeyword != default)
        {
            return;
        }
        var title = FixTitle(R.FixTitle);
        var action = CodeAction.Create(
            title: title,
            createChangedDocument:
                c => Replace(document, root, node, AddUsing),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static SyntaxNode AddUsing(LocalDeclarationStatementSyntax node)
        => node.WithoutLeadingTrivia()
            .WithUsingKeyword(SyntaxFactory.Token(SyntaxKind.UsingKeyword))
            .WithLeadingTrivia(node.GetLeadingTrivia())
            .WithAdditionalAnnotations(Formatter.Annotation);

    private static Task<Document> Replace(
        Document document,
        SyntaxNode root,
        LocalDeclarationStatementSyntax node,
        Func<LocalDeclarationStatementSyntax, SyntaxNode> replacer)
    {
        var newRoot = root.ReplaceNode(node, replacer(node));
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}
