namespace StyleChecker.Refactoring.UnnecessaryUsing;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
/// UnnecessaryUsing CodeFix provider.
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
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        if (await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false) is not {} root
            || root.FindToken(diagnosticSpan.Start)
                .Parent is not UsingStatementSyntax node)
        {
            return;
        }

        var localize = Localizers.Of<R>(R.ResourceManager);
        var title = localize(nameof(R.FixTitle))
            .ToString(CompilerCulture);
        var action = CodeAction.Create(
            title: title,
            createChangedSolution: c => RemoveUsing(context.Document, node, c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Solution> RemoveUsing(
        Document document,
        UsingStatementSyntax node,
        CancellationToken cancellationToken)
    {
        Func<VariableDeclaratorSyntax,
                IEnumerable<(VariableDeclaratorSyntax Node, string TypeName)>>
            TupleSupplier(SemanticModel model)
        {
            return v => (v.Initializer is not {} initializer
                    || model.GetOperation(initializer.Value, cancellationToken)
                        is not {} o
                    || o.Type is not {} valueType)
                ? []
                : [(v, TypeSymbols.GetFullName(valueType))];
        }

        var solution = document.Project
            .Solution;
        if (await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false) is not {} root
            || await document.GetSemanticModelAsync(cancellationToken)
                .ConfigureAwait(false) is not {} model
            || node.Declaration is not {} declaration)
        {
            return solution;
        }

        var toTuple = TupleSupplier(model);
        var type = declaration.Type;
        var variables = declaration.Variables
            .SelectMany(toTuple);
        var list = variables.RunLengthGroupBy(
                i => Classes.DisposesNothing(i.TypeName))
            .ToList();

        VariableDeclarationSyntax ToDeclaration(
            IEnumerable<VariableDeclaratorSyntax> declarators)
        {
            return SyntaxFactory.VariableDeclaration(
                type, SyntaxFactory.SeparatedList(declarators));
        }

        static UsingStatementSyntax NewUsingStatement(
            VariableDeclarationSyntax d, StatementSyntax s)
        {
            return SyntaxFactory.UsingStatement(d, null, s);
        }

        StatementSyntax ToInStatement(
                IEnumerable<VariableDeclaratorSyntax> list)
            => SyntaxFactory.LocalDeclarationStatement(ToDeclaration(list));

        StatementSyntax ToOutStatement(
                IReadOnlyList<VariableDeclaratorSyntax> list,
                StatementSyntax s)
            => (list.Count > 0)
                ? NewUsingStatement(ToDeclaration(list), s)
                : s;

        var newNode = node.Statement;
        IReadOnlyList<VariableDeclaratorSyntax> o = [];
        foreach (var io in list.AsEnumerable().Reverse())
        {
            if (io.Key)
            {
                var i = io.Select(t => t.Node);
                newNode = SyntaxFactory.Block(
                        ToInStatement(i), ToOutStatement(o, newNode))
                    .WithAdditionalAnnotations(Formatter.Annotation);
                continue;
            }
            o = io.Select(t => t.Node).ToList();
        }
        foreach (var io in list.Take(1))
        {
            if (io.Key)
            {
                continue;
            }
            newNode = ToOutStatement(o, newNode)
                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        var targetNode = (newNode is BlockSyntax
                && node.Parent is BlockSyntax parent
                && parent.ChildNodes().Count() is 1)
            ? parent as SyntaxNode : node;
        var workspace = solution.Workspace;
        var formattedNode = Formatter.Format(
                newNode,
                Formatter.Annotation,
                workspace,
                workspace.Options)
            .WithTriviaFrom(targetNode);
        var newRoot = root.ReplaceNode(targetNode, formattedNode);
        return solution.WithDocumentSyntaxRoot(document.Id, newRoot);
    }
}
