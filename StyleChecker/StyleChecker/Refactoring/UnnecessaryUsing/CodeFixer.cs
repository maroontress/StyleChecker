namespace StyleChecker.Refactoring.UnnecessaryUsing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
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
            if (!(token.Parent is UsingStatementSyntax node))
            {
                return;
            }

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
            UsingStatementSyntax node,
            CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            if (root is null)
            {
                return solution;
            }
            var model = await document.GetSemanticModelAsync(cancellationToken)
                .ConfigureAwait(false);
            if (model is null)
            {
                return solution;
            }
            var declaration = node.Declaration;
            if (declaration is null)
            {
                return solution;
            }
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
                    var initializer = v.Initializer;
                    if (initializer is null)
                    {
                        continue;
                    }
                    var value = initializer.Value;
                    var o = model.GetOperation(value, cancellationToken);
                    if (o is null)
                    {
                        continue;
                    }
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
                List<VariableDeclaratorSyntax> InList,
                List<VariableDeclaratorSyntax> OutList)>();
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

            var newNode = node.Statement;
            if (list[0].InList.Count > 0)
            {
                for (var count = list.Count - 1; count >= 0; --count)
                {
                    var (inList, outList) = list[count];
                    var inStatement = SyntaxFactory.LocalDeclarationStatement(
                        ToDeclaration(inList));
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
                for (var count = list.Count - 1; count >= 0; --count)
                {
                    var (inList, outList) = list[count];
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
                    && parent.ChildNodes().Count() == 1)
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
}
