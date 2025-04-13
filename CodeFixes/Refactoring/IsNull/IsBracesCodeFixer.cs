namespace StyleChecker.CodeFixes.Refactoring.IsNull;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using StyleChecker.Analyzers;
using StyleChecker.CodeFixes.Refactoring;
using R = Resources;

/// <summary>
/// IsNull CodeFix provider (with Property Pattern).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IsBracesCodeFixer))]
[Shared]
public sealed class IsBracesCodeFixer : AbstractCodeFixer
{
    private static readonly ImmutableList<ReviserKit> KitList = [
        new ReviserKit(
            nameof(R.FixTitleIsBraces), ReplaceIsNullWithNotIsBraces),
        new ReviserKit(
            nameof(R.FixTitleNotIsBraces), ReplaceNotIsNullWithIsBraces)];

    /// <inheritdoc/>
    protected override ImmutableList<ReviserKit> ReviserKitList => KitList;

    private static Reviser? ReplaceIsNullWithNotIsBraces(
        SyntaxNode root, TextSpan span)
    {
        if (root.FindNodeOfType<IsPatternExpressionSyntax>(span)
            is not {} node)
        {
            return null;
        }
        /*
            expr is null
            => ! ( expr is {} )
        */
        var newIsPattern = WithEmptyPropertyPattern(node);
        var newNode = SyntaxFactory.PrefixUnaryExpression(
            SyntaxKind.LogicalNotExpression,
            SyntaxFactory.ParenthesizedExpression(newIsPattern));
        return new Reviser(root, node, newNode);
    }

    private static Reviser? ReplaceNotIsNullWithIsBraces(
        SyntaxNode root, TextSpan span)
    {
        if (FindNotIsNull(root, span) is not {} result)
        {
            return null;
        }
        var (node, paren, isPattern) = result;

        /*
            ! T0 ( T1 expr T2 is T3 null T4 ) T5
            => TO T1 expr T2 is T3 {} T4 T5
        */
        var newLeading = /*T0*/node.OperatorToken.GetAllTrivia()
            .Concat(/*T1*/paren.OpenParenToken.GetAllTrivia());
        var newTrailing = /*T4*/isPattern.GetTrailingTrivia()
            .Concat(/*T5*/paren.CloseParenToken.GetAllTrivia());
        var newNode = WithEmptyPropertyPattern(isPattern)
            .WithLeadingTrivia(newLeading)
            .WithTrailingTrivia(newTrailing);
        return new Reviser(root, node, newNode);
    }

    private static IsPatternExpressionSyntax WithEmptyPropertyPattern(
        IsPatternExpressionSyntax node)
    {
        var propertyPattern = SyntaxFactory.PropertyPatternClause(
            SyntaxFactory.Token(SyntaxKind.OpenBraceToken),
            SyntaxFactory.SeparatedList<SubpatternSyntax>(),
            SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        return node.WithPattern(
            SyntaxFactory.RecursivePattern()
                .WithPropertyPatternClause(propertyPattern));
    }
}
