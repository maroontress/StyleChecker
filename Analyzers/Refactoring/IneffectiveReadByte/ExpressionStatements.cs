namespace StyleChecker.Analyzers.Refactoring.IneffectiveReadByte;

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Provides utility methods for an expression statement.
/// </summary>
public static class ExpressionStatements
{
    /// <summary>
    /// Gets the properties of the array access associated with the specified
    /// ElementAccessExpression node, where the expression is IdentifierName
    /// and the argument is only one and also IdentifierName, and the type of
    /// the array is the specified type.
    /// </summary>
    /// <param name="model">
    /// The semantic model.
    /// </param>
    /// <param name="node">
    /// The ElementAccessExpression node.
    /// </param>
    /// <param name="arrayType">
    /// The type of the array.
    /// </param>
    /// <returns>
    /// The properties of the array access if the <paramref name="node"/> is
    /// represented with the form <c>array[index]</c> and the type of the
    /// <c>array</c> is <paramref name="arrayType"/>, <c>null</c>
    /// otherwise.
    /// </returns>
    public static ArrayAccess? AccessArrayElement(
        SemanticModel model, SyntaxNode node, string arrayType)
    {
        if (node is not ElementAccessExpressionSyntax elementAccessExpr)
        {
            return null;
        }
        var arguments = elementAccessExpr.ArgumentList.Arguments;
        if (arguments.Count != 1
            || arguments[0].Expression is not IdentifierNameSyntax arg
            || elementAccessExpr.Expression
                is not IdentifierNameSyntax arrayName
            || GetSymbolIfWhoseTypeIs(model, arrayName.Identifier, arrayType)
                is not {} arraySymbol)
        {
            return null;
        }
        var token = arg.Identifier;
        var span = token.Span;
        return model.LookupSymbols(span.Start, null, token.Text)
                .FirstOrDefault() is not {} symbol
            ? null
            : new ArrayAccess(arraySymbol, symbol);
    }

    /// <summary>
    /// Gets the symbol of the instance associated with the specified
    /// InvocationExpression node, where the expression is
    /// MemberAccessExpressionSyntax node and the argument list is empty. The
    /// MemberAccessExpressionSyntax is composed of IdentifierName, a dot
    /// operator "." and the specified member name. The type of the instance is
    /// the specified type.
    /// </summary>
    /// <param name="model">
    /// The semantic model.
    /// </param>
    /// <param name="node">
    /// The InvocationExpressionSyntax node.
    /// </param>
    /// <param name="instanceType">
    /// The type of instance.
    /// </param>
    /// <param name="memberName">
    /// The method name.
    /// </param>
    /// <returns>
    /// The symbol of the instance if the <paramref name="node"/> is
    /// represented with the form <c>instance.method()</c> and the type of
    /// instance is the specified <paramref name="instanceType"/> and the
    /// method name is the specified <paramref name="memberName"/>, <c>null</c>
    /// otherwise.
    /// </returns>
    public static ISymbol? InvocationWithNoArgument(
        SemanticModel model,
        SyntaxNode node,
        string instanceType,
        string memberName)
    {
        if (node is not InvocationExpressionSyntax invocationExpr)
        {
            return null;
        }
        var argumentList = invocationExpr.ArgumentList.Arguments;
        if (argumentList.Any()
            || invocationExpr.Expression
                is not MemberAccessExpressionSyntax memberAccessExpr
            || memberAccessExpr.Expression
                is not IdentifierNameSyntax instanceId)
        {
            return null;
        }
        var instanceToken = instanceId.Identifier;
        var memberToken = memberAccessExpr.Name.Identifier;
        return GetSymbolIfWhoseTypeIs(model, instanceToken, instanceType)
                is not {} symbol
                || memberToken.Text != memberName
            ? null
            : symbol;
    }

    private static ISymbol? GetSymbolIfWhoseTypeIs(
        SemanticModel model, SyntaxToken token, string instanceType)
    {
        var span = token.Span;
        if (model.LookupSymbols(span.Start, null, token.Text)
            .FirstOrDefault() is not {} symbol
            || GetType(symbol) is not {} typeSymbol)
        {
            return null;
        }
        var typeFullName = TypeSymbols.GetFullName(typeSymbol);
        return instanceType != typeFullName ? null : symbol;
    }

    private static ITypeSymbol? GetType(ISymbol symbol)
    {
        if (symbol is ILocalSymbol localSymbol)
        {
            return localSymbol.Type;
        }
        if (symbol is IParameterSymbol parameterSymbol)
        {
            return parameterSymbol.Type;
        }
        if (symbol is IFieldSymbol fieldSymbol)
        {
            return fieldSymbol.Type;
        }
        if (symbol is not IPropertySymbol propertySymbol
            || propertySymbol.GetMethod is not IMethodSymbol getMethod)
        {
            return null;
        }
        var all = getMethod.DeclaringSyntaxReferences;
        return all.FirstOrDefault() is not {} reference
                || reference.GetSyntax() is not AccessorDeclarationSyntax node
                || node.Body is not null
                || node.ExpressionBody is not null
            ? null
            : propertySymbol.Type;
    }
}
