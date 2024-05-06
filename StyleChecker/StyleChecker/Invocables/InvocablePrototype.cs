namespace StyleChecker.Invocables;

using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// The abstract factory methods of <see cref="MethodDeclarationSyntax"/> and
/// <see cref="LocalFunctionStatementSyntax"/> objects.
/// </summary>
/// <typeparam name="T">
/// The <see cref="InvocableNodePod"/> class or its descendants.
/// </typeparam>
public interface InvocablePrototype<out T> : InvocableBasePrototype<T>
{
    /// <summary>
    /// Gets a new <c>T</c> object with the specified <see
    /// cref="TypeParameterListSyntax"/> object.
    /// </summary>
    /// <param name="node">
    /// The <see cref="TypeParameterListSyntax"/> object.
    /// </param>
    /// <returns>
    /// The new <c>T</c> object.
    /// </returns>
    T With(TypeParameterListSyntax node);
}
