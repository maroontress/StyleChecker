namespace StyleChecker.Naming
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Provides utility methods for local variables.
    /// </summary>
    public static class LocalVariables
    {
        /// <summary>
        /// Gets all the tokens of local variables declared with
        /// LocalDeclarationStatement included in the specified compilation
        /// unit.
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
                .Where(n => n.IsKind(SyntaxKind.LocalDeclarationStatement))
                .Select(n => n as LocalDeclarationStatementSyntax)
                .SelectMany(s => s.Declaration.Variables)
                .Select(s => s.Identifier);
        }

        /// <summary>
        /// Gets all the tokens of local variables declared with
        /// SingleVariableDesignation, that is Out Variable Declarations or
        /// Pattern Matching, included in the specified compilation unit.
        /// </summary>
        /// <param name="root">
        /// The compilation unit.
        /// </param>
        /// <returns>
        /// All the tokens of local variables.
        /// </returns>
        public static IEnumerable<SyntaxToken> DesignationTokens(
            CompilationUnitSyntax root)
        {
            /*
                Out Variable Declarations:
                https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/out-var.md

                ArgumentSyntax
                  DeclarationExpressionSyntax <- Expression
                    SingleVariableDesignationSyntax <- Designation
                      SyntaxToken <- Identifier

                Pattern Matching:
                https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/pattern-matching.md

                IsPatternExpressionSyntax
                  DeclarationPatternSyntax <- Pattern
                    SingleVariableDesignationSyntax <- Designation
                      SyntaxToken <- Identifier
            */
            return root.DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.SingleVariableDesignation))
                .Select(n => n as SingleVariableDesignationSyntax)
                .Select(n => n.Identifier);
        }

        /// <summary>
        /// Gets all the tokens of local variables declared with Parameter
        /// included in the specified compilation unit.
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
                .Where(n => n.IsKind(SyntaxKind.Parameter))
                .Select(n => n as ParameterSyntax)
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
                .Where(n => n.IsKind(SyntaxKind.LocalFunctionStatement))
                .Select(n => n as LocalFunctionStatementSyntax)
                .Select(n => n.Identifier);
        }
    }
}
