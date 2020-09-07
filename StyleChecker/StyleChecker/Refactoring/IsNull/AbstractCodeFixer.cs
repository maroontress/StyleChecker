namespace StyleChecker.Refactoring.IsNull
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using R = Resources;

    /// <summary>
    /// Provides abstraction of the CodeFix providers.
    /// </summary>
    public abstract class AbstractCodeFixer : AbstractRevisingCodeFixer
    {
        /// <inheritdoc/>
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        /// <inheritdoc/>
        protected sealed override Func<string, LocalizableString> Localize
            => Localizers.Of<R>(R.ResourceManager);

        /// <summary>
        /// Gets the syntax nodes of <c>!(... is null)</c> which the specified
        /// span in the specified root represents.
        /// </summary>
        /// <param name="root">
        /// The root node.
        /// </param>
        /// <param name="span">
        /// The text span representing <c>!(... is null)</c>.
        /// </param>
        /// <returns>
        /// The tuple containing syntax nodes for <c>!</c>, <c>(...)</c>, and
        /// <c>is null</c> if the span represents <c>!(... is null)</c>.
        /// Otherwise, <c>null</c>.
        /// </returns>
        protected static (PrefixUnaryExpressionSyntax Node,
                ParenthesizedExpressionSyntax Paren,
                IsPatternExpressionSyntax IsPattern)?
            FindNotIsNull(SyntaxNode root, TextSpan span)
        {
            if (!(root.FindNodeOfType<PrefixUnaryExpressionSyntax>(span)
                    is { } node)
                || !(node.Operand is ParenthesizedExpressionSyntax paren)
                || !(paren.Expression is IsPatternExpressionSyntax isPattern))
            {
                return null;
            }
            return (node, paren, isPattern);
        }
    }
}
