namespace StyleChecker.Refactoring.EqualsNull;

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using R = Resources;

/// <summary>
/// EqualsNull CodeFix provider (with Property Pattern).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IsBracesCodeFixer))]
[Shared]
public sealed class IsBracesCodeFixer : AbstractCodeFixer
{
    private static readonly ImmutableList<ReviserKit> KitList
        = ImmutableList.Create(
            new ReviserKit(
                nameof(R.FixTitleIsBraces),
                Replace(SyntaxKind.ExclamationEqualsToken, NotEqual)),
            new ReviserKit(
                nameof(R.FixTitleIsNotBraces),
                Replace(SyntaxKind.EqualsEqualsToken, EqualEqual)));

    /// <inheritdoc/>
    protected override ImmutableList<ReviserKit> ReviserKitList => KitList;

    private static RecursivePatternSyntax NewBraces()
    {
        var emptyProperty = SyntaxFactory.PropertyPatternClause(
            SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
            SyntaxFactory.SeparatedList<SubpatternSyntax>(),
            SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        return SyntaxFactory.RecursivePattern()
            .WithPropertyPatternClause(emptyProperty);
    }

    private static SyntaxNode EqualEqual(BinaryExpressionSyntax node)
    {
        var braces = NewBraces();
        var @is = NewIsToken(node);
        var right = node.Right;
        var n = SyntaxFactory.IsPatternExpression(
            node.Left,
            @is,
            braces.WithLeadingTrivia(right.GetLeadingTrivia()));
        return SyntaxFactory.PrefixUnaryExpression(
                SyntaxKind.LogicalNotExpression,
                SyntaxFactory.ParenthesizedExpression(n)
                    .WithTrailingTrivia(right.GetTrailingTrivia()))
            .WithTriviaFrom(node);
    }

    private static SyntaxNode NotEqual(BinaryExpressionSyntax node)
    {
        var braces = NewBraces();
        var @is = NewIsToken(node);
        var right = node.Right;
        return SyntaxFactory.IsPatternExpression(
                node.Left,
                @is,
                braces.WithTriviaFrom(right))
            .WithTriviaFrom(node);
    }
}
