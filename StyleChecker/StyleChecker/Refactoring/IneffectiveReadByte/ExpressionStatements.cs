namespace StyleChecker.Refactoring.IneffectiveReadByte;

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Provides utility methods for an expression statement.
/// </summary>
public static class ExpressionStatements
{
    /// <summary>
    /// Gets the properties of the array access associated with the
    /// specified ElementAccessExpression node, where the expression is
    /// IdentifierName and the argument is only one and also
    /// IdentifierName, and the type of the array is the specified type.
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
    /// The properties of the array access if the <paramref name="node"/>
    /// is represented with the form <c>array[index]</c> and the type
    /// of the <c>array</c> is <paramref name="arrayType"/>, <c>null</c>
    /// otherwise.
    /// </returns>
    public static ArrayAccess? AccessArrayElement(
        SemanticModel model,
        SyntaxNode node,
        string arrayType)
    {
        if (!(node is ElementAccessExpressionSyntax elementAccessExpr))
        {
            return null;
        }
        var arguments = elementAccessExpr.ArgumentList.Arguments;
        if (arguments.Count != 1)
        {
            return null;
        }
        if (!(arguments[0].Expression is IdentifierNameSyntax arg))
        {
            return null;
        }
        if (!(elementAccessExpr.Expression
            is IdentifierNameSyntax arrayId))
        {
            return null;
        }
        var arratToken = arrayId.Identifier;
        var arraySymbol
            = GetSymbolIfWhoseTypeIs(model, arratToken, arrayType);
        if (arraySymbol is null)
        {
            return null;
        }
        var token = arg.Identifier;
        var span = token.Span;
        var symbol = model.LookupSymbols(span.Start, null, token.Text)
            .FirstOrDefault();
        return symbol is null
            ? null
            : new ArrayAccess(arraySymbol, symbol);
    }

    /// <summary>
    /// Gets the symbol of the instance associated with the specified
    /// InvocationExpression node, where the expression is
    /// MemberAccessExpressionSyntax node and the argument list is empty.
    /// The MemberAccessExpressionSyntax is composed of IdentifierName, a
    /// dot operator "." and the specified member name. The type of the
    /// instance is the specified type.
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
    /// method name is the specified <paramref name="memberName"/>,
    /// <c>null</c> otherwise.
    /// </returns>
    public static ISymbol? InvocationWithNoArgument(
        SemanticModel model,
        SyntaxNode node,
        string instanceType,
        string memberName)
    {
        if (!(node is InvocationExpressionSyntax invocationExpr))
        {
            return null;
        }
        var argumentList = invocationExpr.ArgumentList.Arguments;
        if (argumentList.Any())
        {
            return null;
        }
        if (!(invocationExpr.Expression
            is MemberAccessExpressionSyntax memberAccessExpr))
        {
            return null;
        }
        if (!(memberAccessExpr.Expression
            is IdentifierNameSyntax instanceId))
        {
            return null;
        }
        var instanceToken = instanceId.Identifier;
        var memberToken = memberAccessExpr.Name.Identifier;
        var symbol = GetSymbolIfWhoseTypeIs(
            model, instanceToken, instanceType);
        return symbol is null || memberToken.Text != memberName
            ? null
            : symbol;
    }

    private static ISymbol? GetSymbolIfWhoseTypeIs(
        SemanticModel model,
        SyntaxToken token,
        string instanceType)
    {
        var span = token.Span;
        var symbol = model.LookupSymbols(span.Start, null, token.Text)
            .FirstOrDefault();
        if (symbol is null)
        {
            return null;
        }
        var typeSymbol = GetType(symbol);
        if (typeSymbol is null)
        {
            return null;
        }
        var typeFullName = TypeSymbols.GetFullName(typeSymbol);
        return (instanceType != typeFullName) ? null : symbol;
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
        if (symbol is IPropertySymbol propertySymbol
            && propertySymbol.GetMethod is IMethodSymbol getMethod)
        {
            var reference = getMethod.DeclaringSyntaxReferences
                .FirstOrDefault();
            return (reference is null
                    || !(reference.GetSyntax()
                        is AccessorDeclarationSyntax node)
                    || !(node.Body is null)
                    || !(node.ExpressionBody is null))
                ? null
                : propertySymbol.Type;
        }
        return null;
    }
}
