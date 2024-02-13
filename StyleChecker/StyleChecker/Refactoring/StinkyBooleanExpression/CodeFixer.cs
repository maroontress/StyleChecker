namespace StyleChecker.Refactoring.StinkyBooleanExpression;

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
/// StinkyBooleanExpression CodeFix provider.
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
        string FixTitle(string key)
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            return localize(key).ToString(CompilerCulture);
        }

        if (await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;

        void Register(
            Func<ConditionalExpressionSyntax, SyntaxNode> replacer,
            SyntaxKind kind,
            string key)
        {
            if (root.FindNodeOfType<ConditionalExpressionSyntax>(span)
                is not {} node
                || !node.BothIsKind(kind))
            {
                return;
            }
            var title = FixTitle(key);
            var action = CodeAction.Create(
                title: title,
                createChangedSolution:
                    c => Replace(context.Document, node, replacer, c),
                equivalenceKey: title);
            context.RegisterCodeFix(action, diagnostic);
        }

        Register(
            ReplaceConditionWithLogicalAnd,
            SyntaxKind.FalseLiteralExpression,
            nameof(R.FixTitleUseConditionalLogicalAnd));
        Register(
            ReplaceConditionWithLogicalOr,
            SyntaxKind.TrueLiteralExpression,
            nameof(R.FixTitleUseConditionalLogicalOr));
    }

    private static ParenthesizedExpressionSyntax Parenthesize(
        ExpressionSyntax s)
    {
        return s is ParenthesizedExpressionSyntax parenthesized
            ? parenthesized
            : SyntaxFactory.ParenthesizedExpression(s);
    }

    private static PrefixUnaryExpressionSyntax? GetLogicalNot(
        ExpressionSyntax s)
    {
        return (s.IsKind(SyntaxKind.LogicalNotExpression)
                && s is PrefixUnaryExpressionSyntax logicalNot)
            ? logicalNot
            : null;
    }

    private static ExpressionSyntax Negate(ExpressionSyntax node)
    {
        static ExpressionSyntax Reverse(ExpressionSyntax s)
        {
            return (s.IsKind(SyntaxKind.LogicalNotExpression)
                    && s is PrefixUnaryExpressionSyntax logicalNot)
                /* !... => (...) */
                ? Parenthesize(logicalNot.Operand)
                    .WithTriviaFrom(s)
                /* ... => !(...) */
                : SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    Parenthesize(s));
        }

        return (node is ParenthesizedExpressionSyntax parenthesized
                && GetLogicalNot(parenthesized.Expression)
                    is PrefixUnaryExpressionSyntax logicalNot)
            /* (!...) => (...) */
            ? Parenthesize(logicalNot.Operand)
                .WithTriviaFrom(node)
            : Reverse(node);
    }

    private static SyntaxNode ReplaceConditionWithLogicalAnd(
        ConditionalExpressionSyntax node)
    {
        return ReplaceConditionWith(
            node,
            SyntaxKind.FalseLiteralExpression,
            Parenthesize,
            Negate,
            /* logical and (&&) */
            SyntaxFactory.Token(SyntaxKind.AmpersandAmpersandToken),
            SyntaxKind.LogicalAndExpression);
    }

    private static SyntaxNode ReplaceConditionWithLogicalOr(
        ConditionalExpressionSyntax node)
    {
        return ReplaceConditionWith(
            node,
            SyntaxKind.TrueLiteralExpression,
            Negate,
            Parenthesize,
            /* logical or (||) */
            SyntaxFactory.Token(SyntaxKind.BarBarToken),
            SyntaxKind.LogicalOrExpression);
    }

    private static SyntaxNode ReplaceConditionWith(
        ConditionalExpressionSyntax node,
        SyntaxKind boolLiteralKind,
        Func<ExpressionSyntax, ExpressionSyntax> trueWrap,
        Func<ExpressionSyntax, ExpressionSyntax> falseWrap,
        SyntaxToken operatorToken,
        SyntaxKind binaryOperatorKind)
    {
        var (wrap, rightNode) = node.WhenFalse.IsKind(boolLiteralKind)
            ? (trueWrap, node.WhenTrue)
            : (falseWrap, node.WhenFalse);
        var newNode = SyntaxFactory.BinaryExpression(
                binaryOperatorKind,
                wrap(node.Condition),
                operatorToken.WithTriviaFrom(node.QuestionToken),
                Parenthesize(rightNode))
            .WithTriviaFrom(node);
        return node.Parent is ParenthesizedExpressionSyntax
            /* ... => ... */
            ? newNode
            /* ... => (...) */
            : SyntaxFactory.ParenthesizedExpression(newNode);
    }

    private static async Task<Solution> Replace<T>(
        Document document,
        T node,
        Func<T, SyntaxNode> getNewNode,
        CancellationToken cancellationToken)
        where T : SyntaxNode
    {
        var solution = document.Project.Solution;
        if (await document.GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false) is not {} root
            || root.ReplaceNode(node, getNewNode(node)) is not {} newRoot)
        {
            return solution;
        }
        var workspace = solution.Workspace;
        var formattedNode = Formatter.Format(
           newRoot,
           Formatter.Annotation,
           workspace,
           workspace.Options);
        return solution.WithDocumentSyntaxRoot(document.Id, formattedNode);
    }
}
