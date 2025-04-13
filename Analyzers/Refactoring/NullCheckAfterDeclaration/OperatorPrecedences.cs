namespace StyleChecker.Analyzers.Refactoring.NullCheckAfterDeclaration;

using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Provides methods to get the operator precedence for different kinds of
/// expressions.
/// </summary>
public static class OperatorPrecedences
{
    /// <summary>
    /// Gets the operator precedence for the specified <see cref="SyntaxKind"/>
    /// representing a expression.
    /// </summary>
    /// <param name="kind">
    /// The <see cref="SyntaxKind"/> to get the precedence for.
    /// </param>
    /// <returns>
    /// The precedence of the operator as an integer. A smaller integer means a
    /// higher precedence.
    /// </returns>
    public static int Of(SyntaxKind kind)
    {
        /*
            See:
            https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/#operator-precedence
        */
        return kind switch
        {
            SyntaxKind.SimpleMemberAccessExpression
            or SyntaxKind.InvocationExpression
            or SyntaxKind.ElementAccessExpression
            or SyntaxKind.ConditionalAccessExpression
            or SyntaxKind.PostIncrementExpression
            or SyntaxKind.PostDecrementExpression
            or SyntaxKind.ObjectCreationExpression
            or SyntaxKind.ImplicitArrayCreationExpression
            or SyntaxKind.ImplicitObjectCreationExpression
            or SyntaxKind.TypeOfExpression
            or SyntaxKind.CheckedExpression
            or SyntaxKind.UncheckedExpression
            or SyntaxKind.DefaultExpression
            or SyntaxKind.SizeOfExpression
            or SyntaxKind.StackAllocArrayCreationExpression
            or SyntaxKind.ImplicitStackAllocArrayCreationExpression
            or SyntaxKind.PointerMemberAccessExpression
            or SyntaxKind.CollectionExpression
            or SyntaxKind.ParenthesizedExpression
            or SyntaxKind.IdentifierName
                => 1,
            SyntaxKind.UnaryMinusExpression
            or SyntaxKind.UnaryPlusExpression
            or SyntaxKind.BitwiseNotExpression
            or SyntaxKind.LogicalNotExpression
            or SyntaxKind.PreIncrementExpression
            or SyntaxKind.PreDecrementExpression
            or SyntaxKind.IndexExpression
            or SyntaxKind.CastExpression
            or SyntaxKind.AwaitExpression
            or SyntaxKind.PointerIndirectionExpression
            or SyntaxKind.AddressOfExpression
            /*
                See:
                https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/true-false-operators
            */
                => 2,
            SyntaxKind.RangeExpression => 3,
            SyntaxKind.SwitchExpression
            or SyntaxKind.WithExpression
                => 4,
            SyntaxKind.MultiplyExpression
            or SyntaxKind.DivideExpression
            or SyntaxKind.ModuloExpression
                => 5,
            SyntaxKind.AddExpression
            or SyntaxKind.SubtractExpression
                => 6,
            SyntaxKind.LeftShiftExpression
            or SyntaxKind.RightShiftExpression
            or SyntaxKind.UnsignedRightShiftExpression
                => 7,
            SyntaxKind.LessThanExpression
            or SyntaxKind.GreaterThanExpression
            or SyntaxKind.LessThanOrEqualExpression
            or SyntaxKind.GreaterThanOrEqualExpression
            or SyntaxKind.IsPatternExpression
            or SyntaxKind.AsExpression
                => 8,
            SyntaxKind.EqualsExpression
            or SyntaxKind.NotEqualsExpression
                => 9,
            SyntaxKind.BitwiseAndExpression => 10,
            SyntaxKind.ExclusiveOrExpression => 11,
            SyntaxKind.BitwiseOrExpression => 12,
            SyntaxKind.LogicalAndExpression => 13,
            SyntaxKind.LogicalOrExpression => 14,
            SyntaxKind.CoalesceExpression => 15,
            SyntaxKind.ConditionalExpression => 16,
            _ => 17,
        };
    }
}
