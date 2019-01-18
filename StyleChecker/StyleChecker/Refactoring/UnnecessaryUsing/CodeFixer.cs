namespace StyleChecker.Refactoring.UnnecessaryUsing
{
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
    public sealed class CodeFixer : CodeFixProvider
    {
        static CodeFixer()
        {
        }

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            var title = localize(nameof(R.FixTitle)).ToString();

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var token = root.FindToken(diagnosticSpan.Start);
            var node = token.Parent;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution:
                        c => RemoveUsing(context.Document, node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static async Task<Solution> RemoveUsing(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken)
                .ConfigureAwait(false);
            var syntax = node as UsingStatementSyntax;
            var declaration = syntax.Declaration;
            var type = declaration.Type;
            var variables = declaration.Variables;
            var n = variables.Count;
            var k = 0;

            List<VariableDeclaratorSyntax> GetList(Func<string, bool> matches)
            {
                var syntaxList = new List<VariableDeclaratorSyntax>(n);
                for (; k < n; ++k)
                {
                    var v = variables[k];
                    var value = v.Initializer.Value;
                    var o = model.GetOperation(value, cancellationToken);
                    var valueType = o.Type;
                    var name = TypeSymbols.GetFullName(valueType);
                    if (matches(name))
                    {
                        break;
                    }
                    syntaxList.Add(v);
                }
                return syntaxList;
            }
            var list = new List<(
                List<VariableDeclaratorSyntax> inList,
                List<VariableDeclaratorSyntax> outList)>();
            do
            {
                var inList = GetList(s => !Analyzer.DisposesNothing(s));
                var outList = GetList(s => Analyzer.DisposesNothing(s));
                list.Add((inList, outList));
            }
            while (k < n);

            VariableDeclarationSyntax ToDeclaration(
                IEnumerable<VariableDeclaratorSyntax> declarators)
            {
                return SyntaxFactory.VariableDeclaration(
                    type, SyntaxFactory.SeparatedList(declarators));
            }

            StatementSyntax newNode;
            if (list[0].inList.Count > 0)
            {
                newNode = syntax.Statement;
                for (var count = list.Count - 1; count >= 0; --count)
                {
                    var item = list[count];
                    var inStatement = SyntaxFactory.LocalDeclarationStatement(
                        ToDeclaration(item.inList));
                    var outList = item.outList;
                    var outStatement = (outList.Count > 0)
                        ? SyntaxFactory.UsingStatement(
                            ToDeclaration(outList), null, newNode)
                        : newNode;
                    newNode = SyntaxFactory.Block(inStatement, outStatement)
                        .WithAdditionalAnnotations(Formatter.Annotation);
                }
            }
            else
            {
                newNode = syntax.Statement;
                for (var count = list.Count - 1; count >= 0; --count)
                {
                    var item = list[count];
                    var inList = item.inList;
                    var outList = item.outList;
                    var outStatement = (outList.Count > 0)
                        ? SyntaxFactory.UsingStatement(
                            ToDeclaration(outList), null, newNode)
                        : newNode;
                    newNode = ((inList.Count > 0)
                        ? SyntaxFactory.Block(
                            SyntaxFactory.LocalDeclarationStatement(
                                ToDeclaration(inList)),
                            outStatement)
                        : outStatement)
                        .WithAdditionalAnnotations(Formatter.Annotation);
                }
            }
            var targetNode = (newNode is BlockSyntax
                && node.Parent is BlockSyntax parent
                && parent.ChildNodes().Count() == 1) ? parent : node;
            var solution = document.Project.Solution;
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
}
