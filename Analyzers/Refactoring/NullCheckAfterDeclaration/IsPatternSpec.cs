namespace Analyzers.Refactoring.NullCheckAfterDeclaration;

using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Represents the components for an 'is' pattern matching expression.
/// </summary>
/// <remarks>
/// Provides information to create <see cref="DeclarationPatternSyntax"/>
/// objects such as <c>E is T name</c> and <c>E is {} name</c>.
/// </remarks>
/// <param name="Expression">
/// The expression <c>E</c> being pattern matched.
/// </param>
/// <param name="TypePattern">
/// The optional type <c>T</c> used in the type pattern. If it is <c>null</c>,
/// use the property pattern <c>{}</c> instead of the type pattern.
/// </param>
public record struct IsPatternSpec(
    ExpressionSyntax Expression,
    TypeSyntax? TypePattern);
