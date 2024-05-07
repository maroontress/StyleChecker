namespace StyleChecker.Refactoring.IneffectiveReadByte;

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using R = Resources;

/// <summary>
/// IneffectiveReadByte code fix provider.
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
        var localize = Localizers.Of<R>(R.ResourceManager);
        var title = localize(nameof(R.FixTitle))
            .ToString(CompilerCulture);

        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        string GetValue(string key)
        {
            var property = diagnostic.Properties[key];
            return property
                ?? throw new NullReferenceException(nameof(property));
        }

        var node = root.FindNodeOfType<ForStatementSyntax>(diagnosticSpan);
        if (node is null)
        {
            return;
        }

        var action = CodeAction.Create(
            title: title,
            createChangedDocument:
                c => Replace(context.Document, node, GetValue, c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> Replace(
        Document document,
        ForStatementSyntax node,
        Func<string, string> getValue,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }
        var formatAnnotation = Formatter.Annotation;

        static string GetFixTemplate() => EmbeddedResources.GetText(
            "Refactoring.IneffectiveReadByte", "FixTemplate.txt");

        var statement = Texts.Substitute(GetFixTemplate(), getValue);
        var newNode = SyntaxFactory.ParseStatement(statement)
            .WithTriviaFrom(node)
            .WithAdditionalAnnotations(formatAnnotation);

        var solution = document.Project.Solution;
        var workspace = solution.Workspace;

        var formattedNode = Formatter.Format(
           newNode,
           formatAnnotation,
           workspace,
           workspace.Options);
        var newRoot = root.ReplaceNode(node, formattedNode);
        return document.WithSyntaxRoot(newRoot);
    }
}
