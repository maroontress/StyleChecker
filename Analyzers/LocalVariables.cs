namespace Analyzers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Provides utility methods for local variables.
/// </summary>
public static class LocalVariables
{
    /// <summary>
    /// Gets all the tokens of local variables declared with
    /// LocalDeclarationStatement included in the specified compilation unit.
    /// </summary>
    /// <param name="root">
    /// The compilation unit.
    /// </param>
    /// <returns>
    /// All the tokens of local variables.
    /// </returns>
    public static IEnumerable<SyntaxToken> DeclarationTokens(
        CompilationUnitSyntax root)
    {
        /*
            Local variable declarations:

            LocalDeclarationStatementSyntax
              VariableDeclarationSyntax <- Declaration
                SeparatedSyntaxList<VariableDeclaratorSyntax> <- Variables
                  SyntaxToken <- Identifier
        */
        return root.DescendantNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .SelectMany(s => s.Declaration.Variables)
            .Select(s => s.Identifier);
    }

    /// <summary>
    /// Gets all the tokens of local variables declared with Out Variable
    /// Declarations included in the specified compilation unit.
    /// </summary>
    /// <param name="root">
    /// The compilation unit.
    /// </param>
    /// <returns>
    /// All the tokens of local variables.
    /// </returns>
    public static IEnumerable<SyntaxToken> OutVariableTokens(
        CompilationUnitSyntax root)
    {
        /*
            Out Variable Declarations:
            https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/out-var.md

            ArgumentSyntax
              DeclarationExpressionSyntax <- Expression
                SingleVariableDesignationSyntax <- Designation
                  SyntaxToken <- Identifier
        */
        return root.DescendantNodes()
            .OfType<DeclarationExpressionSyntax>()
            .Select(n => n.Designation)
            .OfType<SingleVariableDesignationSyntax>()
            .Select(n => n.Identifier);
    }

    /// <summary>
    /// Gets all the tokens of local variables declared with Pattern Matching,
    /// included in the specified compilation unit.
    /// </summary>
    /// <param name="root">
    /// The compilation unit.
    /// </param>
    /// <returns>
    /// All the tokens of local variables.
    /// </returns>
    public static IEnumerable<SyntaxToken> PatternMatchingTokens(
        CompilationUnitSyntax root)
    {
        /*
            Pattern Matching:
            https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/pattern-matching.md

            IsPatternExpressionSyntax
              DeclarationPatternSyntax <- Pattern
                SingleVariableDesignationSyntax <- Designation
                  SyntaxToken <- Identifier
        */
        return root.DescendantNodes()
            .OfType<DeclarationPatternSyntax>()
            .Select(n => n.Designation)
            .OfType<SingleVariableDesignationSyntax>()
            .Select(n => n.Identifier);
    }

    /// <summary>
    /// Gets all the tokens of local variables declared with ForEachStatement
    /// included in the specified compilation unit.
    /// </summary>
    /// <param name="root">
    /// The compilation unit.
    /// </param>
    /// <returns>
    /// All the tokens of local variables.
    /// </returns>
    public static IEnumerable<SyntaxToken> ForEachTokens(
        CompilationUnitSyntax root)
    {
        /*
            ForEach:

            ForEachStatementSyntax
              SyntaxToken <- Identifier
        */
        return root.DescendantNodes()
            .OfType<ForEachStatementSyntax>()
            .Select(n => n.Identifier);
    }

    /// <summary>
    /// Gets all the tokens of local variables declared with CatchDeclaration
    /// included in the specified compilation unit.
    /// </summary>
    /// <param name="root">
    /// The compilation unit.
    /// </param>
    /// <returns>
    /// All the tokens of local variables.
    /// </returns>
    public static IEnumerable<SyntaxToken> CatchTokens(
        CompilationUnitSyntax root)
    {
        /*
            Catch:

            CatchDeclarationSyntax
              SyntaxToken <- Identifier
        */
        return root.DescendantNodes()
            .OfType<CatchDeclarationSyntax>()
            .Where(n => n.Identifier != default)
            .Select(n => n.Identifier);
    }

    /// <summary>
    /// Gets all the tokens of local variables declared with Parameter included
    /// in the specified compilation unit.
    /// </summary>
    /// <param name="root">
    /// The compilation unit.
    /// </param>
    /// <returns>
    /// All the tokens of local variables.
    /// </returns>
    public static IEnumerable<SyntaxToken> ParameterTokens(
        CompilationUnitSyntax root)
    {
        /*
            Parameters:

            ParameterSyntax
              SyntaxToken <- Identifier
        */
        return root.DescendantNodes()
            .OfType<ParameterSyntax>()
            .Select(n => n.Identifier);
    }

    /// <summary>
    /// Gets all the tokens of local variables declared with LocalFunction
    /// included in the specified compilation unit.
    /// </summary>
    /// <param name="root">
    /// The compilation unit.
    /// </param>
    /// <returns>
    /// All the tokens of local variables.
    /// </returns>
    public static IEnumerable<SyntaxToken> FunctionTokens(
        CompilationUnitSyntax root)
    {
        /*
            Local function:
            https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/local-functions.md

            LocalFunctionStatementSyntax
              SyntaxToken <- Identifier
        */
        return root.DescendantNodes()
            .OfType<LocalFunctionStatementSyntax>()
            .Select(n => n.Identifier);
    }

    /// <summary>
    /// Gets all the tokens and symbols representing local variables (except
    /// parameters) declared in the specified semantic model.
    /// </summary>
    /// <param name="model">
    /// The semantic model.
    /// </param>
    /// <returns>
    /// The tuples of the token and its symbol.
    /// </returns>
    public static IEnumerable<(SyntaxToken Token, ILocalSymbol Symbol)>
        Symbols(SemanticModel model)
    {
        T? ToOperationOf<T>(SyntaxToken t)
            where T : class, IOperation
        {
            return (t.Parent is not {} p)
                ? null
                : model.GetOperation(p) as T;
        }

        (SyntaxToken Token, ILocalSymbol? Symbol)
                ToTuple<T>(SyntaxToken t, Func<T?, ILocalSymbol?> f)
            where T : class, IOperation
            => (t, f(ToOperationOf<T>(t)));

        (SyntaxToken Token, ILocalSymbol? Symbol)
                ToDeclaratorTuple(SyntaxToken t)
            => ToTuple<IVariableDeclaratorOperation>(t, o => o?.Symbol);

        (SyntaxToken Token, ILocalSymbol? Symbol)
                ToOutVariableTuple(SyntaxToken t)
            => ToTuple<ILocalReferenceOperation>(t, o => o?.Local);

        (SyntaxToken Token, ILocalSymbol? Symbol)
            ToPatternMatchingTuple(SyntaxToken t)
        {
            var symbol = (t.Parent?.Parent is not {} n
                    || model.GetOperation(n)
                        is not IDeclarationPatternOperation s)
                ? null
                : s.DeclaredSymbol as ILocalSymbol;
            return (t, symbol);
        }

        (SyntaxToken Token, ILocalSymbol? Symbol) ToForEachTuple(SyntaxToken t)
        {
            var symbol = (t.Parent is not {} n
                || model.GetOperation(n) is not IForEachLoopOperation loop
                || loop.LoopControlVariable
                    is not IVariableDeclaratorOperation variable)
                ? null
                : variable.Symbol;
            return (t, symbol);
        }

        static IEnumerable<(SyntaxToken Token, T Symbol)>
                ToArray<T>((SyntaxToken Token, T? Symbol) p)
            where T : class, ISymbol
        {
            return (p.Symbol is not {} symbol)
                ? Enumerable.Empty<(SyntaxToken, T)>()
                : [(p.Token, symbol)];
        }

        var root = model.SyntaxTree.GetCompilationUnitRoot();
        var declarators = DeclarationTokens(root)
            .Concat(CatchTokens(root))
            .Select(ToDeclaratorTuple);
        var outVariables = OutVariableTokens(root)
            .Select(ToOutVariableTuple);
        var patternMatches = PatternMatchingTokens(root)
            .Select(ToPatternMatchingTuple);
        var forEaches = ForEachTokens(root)
            .Select(ToForEachTuple);
        return declarators
            .Concat(outVariables)
            .Concat(patternMatches)
            .Concat(forEaches)
            .SelectMany(ToArray);
    }
}
