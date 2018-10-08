namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static partial class ExpressionStatements
    {
        public static ArrayAccess AccessArrayElement(
            SemanticModel model,
            SyntaxNode node,
            string arrayType)
        {
            var elementAccessExpr = node as ElementAccessExpressionSyntax;
            if (elementAccessExpr == null)
            {
                return null;
            }
            var arguments = elementAccessExpr.ArgumentList.Arguments;
            if (arguments.Count() != 1)
            {
                return null;
            }
            var arg = arguments[0].Expression as IdentifierNameSyntax;
            if (arg == null)
            {
                return null;
            }
            var arrayId = elementAccessExpr.Expression as IdentifierNameSyntax;
            if (arrayId == null)
            {
                return null;
            }
            var arratToken = arrayId.Identifier;
            var arraySymbol = GetSymbolIfWhoseTypeIs(model, arratToken, arrayType);
            if (arraySymbol == null)
            {
                return null;
            }
            var token = arg.Identifier;
            var span = token.Span;
            var symbol = model.LookupSymbols(span.Start, null, token.Text)
                .FirstOrDefault();
            if (symbol == null)
            {
                return null;
            }
            return new ArrayAccess(arraySymbol, symbol);
        }

        public static ISymbol InvocationWithNoArgument(
            SemanticModel model,
            SyntaxNode node,
            string instanceType,
            string memberName)
        {
            var invocationExpr = node as InvocationExpressionSyntax;
            if (invocationExpr == null)
            {
                return null;
            }
            var argumentList = invocationExpr.ArgumentList.Arguments;
            if (argumentList.Any())
            {
                return null;
            }
            var memberAccessExpr = invocationExpr.Expression
                as MemberAccessExpressionSyntax;
            if (memberAccessExpr == null)
            {
                return null;
            }
            var instanceId = memberAccessExpr.Expression
                as IdentifierNameSyntax;
            if (instanceId == null)
            {
                return null;
            }
            var instanceToken = instanceId.Identifier;
            var memberToken = memberAccessExpr.Name.Identifier;
            var symbol = GetSymbolIfWhoseTypeIs(model, instanceToken, instanceType);
            if (symbol == null)
            {
                return null;
            }
            if (!memberToken.Text.Equals(memberName))
            {
                return null;
            }
            return symbol;
        }

        private static ISymbol GetSymbolIfWhoseTypeIs(
            SemanticModel model,
            SyntaxToken token,
            string instanceType)
        {
            var span = token.Span;
            var symbol = model.LookupSymbols(span.Start, null, token.Text)
                .FirstOrDefault();
            if (symbol == null)
            {
                return null;
            }
            var typeSymbol = GetType(symbol);
            if (typeSymbol == null)
            {
                return null;
            }
            var typeFullName = GetFullName(typeSymbol);
            return !instanceType.Equals(typeFullName) ? null : symbol;
        }

        private static ITypeSymbol GetType(ISymbol symbol)
        {
            if (symbol is ILocalSymbol localSymbol)
            {
                return localSymbol.Type;
            }
            if (symbol is IFieldSymbol fieldSymbol)
            {
                return fieldSymbol.Type;
            }
            return null;
        }

        private static string GetFullName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                return GetFullName(arrayTypeSymbol.ElementType) + "[]";
            }
            var typeFullName = typeSymbol.ContainingNamespace
                + "."
                + typeSymbol.Name;
            return typeFullName;
        }
    }
}
