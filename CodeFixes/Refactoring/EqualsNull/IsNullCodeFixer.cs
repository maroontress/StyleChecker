namespace CodeFixes.Refactoring.EqualsNull;

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using R = Resources;

/// <summary>
/// EqualsNull CodeFix provider (with Constant Pattern).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IsNullCodeFixer))]
[Shared]
public sealed class IsNullCodeFixer : AbstractCodeFixer
{
    private static readonly ImmutableList<ReviserKit> KitList = [
        new ReviserKit(
            nameof(R.FixTitleIsNotNull),
            Replace(SyntaxKind.EqualsEqualsToken, NotEqual)),
        new ReviserKit(
            nameof(R.FixTitleIsNull),
            Replace(SyntaxKind.ExclamationEqualsToken, EqualEqual))];

    /// <inheritdoc/>
    protected override ImmutableList<ReviserKit> ReviserKitList => KitList;

    private static SyntaxNode NotEqual(BinaryExpressionSyntax node)
    {
        var right = node.Right;
        var @is = NewIsToken(node);
        var @null = NewNullConstant();
        return SyntaxFactory.IsPatternExpression(
                node.Left,
                @is,
                @null.WithTriviaFrom(right))
            .WithTriviaFrom(node);
    }

    private static SyntaxNode EqualEqual(BinaryExpressionSyntax node)
    {
        var right = node.Right;
        var @is = NewIsToken(node);
        var @null = NewNullConstant();
        var n = SyntaxFactory.IsPatternExpression(
            node.Left,
            @is,
            @null.WithLeadingTrivia(right.GetLeadingTrivia()));
        return SyntaxFactory.PrefixUnaryExpression(
                SyntaxKind.LogicalNotExpression,
                SyntaxFactory.ParenthesizedExpression(n)
                    .WithTrailingTrivia(right.GetTrailingTrivia()))
            .WithTriviaFrom(node);
    }

    private static ConstantPatternSyntax NewNullConstant()
    {
        return SyntaxFactory.ConstantPattern(
            SyntaxFactory.LiteralExpression(
                SyntaxKind.NullLiteralExpression));
    }
}
