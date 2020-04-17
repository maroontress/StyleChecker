namespace StyleChecker.Ordering.PostIncrement
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using PueSyntax
        = Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax;
    using R = Resources;

    /// <summary>
    /// PostIncrement code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
        private static readonly Func<SyntaxKind, SyntaxKind> KindMap
            = NewKindMap();

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

            var node = root.FindNodeOfType<PueSyntax>(diagnosticSpan);
            if (node is null)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument:
                        c => Replace(context.Document, node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static Func<SyntaxKind, SyntaxKind> NewKindMap()
        {
            var map = new Dictionary<SyntaxKind, SyntaxKind>()
            {
                [SyntaxKind.PostDecrementExpression]
                    = SyntaxKind.PreDecrementExpression,
                [SyntaxKind.PostIncrementExpression]
                    = SyntaxKind.PreIncrementExpression,
            };
            SyntaxKind Map(SyntaxKind key)
                => map.TryGetValue(key, out var value)
                    ? value : default;
            return Map;
        }

        private static async Task<Document> Replace(
            Document document,
            PueSyntax node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            if (root is null)
            {
                return document;
            }
            var newKind = KindMap(node.Kind());
            if (newKind == default)
            {
                return document;
            }
            var operand = node.Operand;
            var token = node.OperatorToken;
            var newToken = token
                .WithLeadingTrivia(operand.GetLeadingTrivia())
                .WithTrailingTrivia(operand.GetTrailingTrivia());
            var newOperand = operand
                .WithLeadingTrivia(token.LeadingTrivia)
                .WithTrailingTrivia(token.TrailingTrivia);
            var newNode = SyntaxFactory
                .PrefixUnaryExpression(newKind, newToken, newOperand)
                .WithTriviaFrom(node);
            var newRoot = root.ReplaceNode(node, newNode);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
