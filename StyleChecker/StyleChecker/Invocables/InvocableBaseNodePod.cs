namespace StyleChecker.Invocables
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The wrapper of <see cref="BaseMethodDeclarationSyntax"/>
    /// and <see cref="LocalFunctionStatementSyntax"/> objects.
    /// </summary>
    public sealed class InvocableBaseNodePod : InvocableBaseProperties,
        InvocableBasePrototype<InvocableBaseNodePod>
    {
        private InvocableBaseNodePod(BaseMethodDeclarationSyntax node)
        {
            Node = node;
            ParameterList = node.ParameterList;
            Body = node.Body;
            ExpressionBody = node.ExpressionBody;
            Modifiers = node.Modifiers;

            Func<T, InvocableBaseNodePod> With<T>(
                Func<T, BaseMethodDeclarationSyntax> with)
                => n => new InvocableBaseNodePod(with(n));

            WithBlockSyntax = With<BlockSyntax>(node.WithBody);
            WithParameterListSyntax
                = With<ParameterListSyntax>(node.WithParameterList);
            WithArrowExpressionClauseSyntax
                = With<ArrowExpressionClauseSyntax>(node.WithExpressionBody);
            WithSemicolonToken = With<SyntaxToken>(node.WithSemicolonToken);
        }

        private InvocableBaseNodePod(LocalFunctionStatementSyntax node)
        {
            Node = node;
            ParameterList = node.ParameterList;
            Body = node.Body;
            ExpressionBody = node.ExpressionBody;
            Modifiers = node.Modifiers;

            Func<T, InvocableBaseNodePod> With<T>(
                Func<T, LocalFunctionStatementSyntax> with)
                => n => new InvocableBaseNodePod(with(n));

            WithBlockSyntax = With<BlockSyntax>(node.WithBody);
            WithParameterListSyntax
                = With<ParameterListSyntax>(node.WithParameterList);
            WithArrowExpressionClauseSyntax
                = With<ArrowExpressionClauseSyntax>(node.WithExpressionBody);
            WithSemicolonToken = With<SyntaxToken>(node.WithSemicolonToken);
        }

        /// <inheritdoc/>
        public override ParameterListSyntax ParameterList { get; }

        /// <inheritdoc/>
        public override BlockSyntax Body { get; }

        /// <inheritdoc/>
        public override ArrowExpressionClauseSyntax ExpressionBody { get; }

        /// <inheritdoc/>
        public override SyntaxNode Node { get; }

        /// <inheritdoc/>
        public override SyntaxTokenList Modifiers { get; }

        private Func<BlockSyntax, InvocableBaseNodePod>
            WithBlockSyntax { get; }

        private Func<ParameterListSyntax, InvocableBaseNodePod>
            WithParameterListSyntax { get; }

        private Func<ArrowExpressionClauseSyntax, InvocableBaseNodePod>
            WithArrowExpressionClauseSyntax { get; }

        private Func<SyntaxToken, InvocableBaseNodePod>
            WithSemicolonToken { get; }

        /// <summary>
        /// Gets a new <see cref="InvocableBaseNodePod"/> object wrappings the
        /// specified <see cref="SyntaxNode"/> node.
        /// </summary>
        /// <param name="node">
        /// The <see cref="LocalFunctionStatementSyntax"/> or
        /// <see cref="BaseMethodDeclarationSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <see cref="InvocableBaseNodePod"/> object.
        /// </returns>
        public static InvocableBaseNodePod Of(SyntaxNode node)
        {
            return node is LocalFunctionStatementSyntax localFunction
                ? new InvocableBaseNodePod(localFunction)
                : node is BaseMethodDeclarationSyntax method
                    ? new InvocableBaseNodePod(method)
                    : null;
        }

        /// <inheritdoc/>
        public InvocableBaseNodePod With(BlockSyntax node)
            => WithBlockSyntax(node);

        /// <inheritdoc/>
        public InvocableBaseNodePod With(ParameterListSyntax node)
            => WithParameterListSyntax(node);

        /// <inheritdoc/>
        public InvocableBaseNodePod With(ArrowExpressionClauseSyntax node)
            => WithArrowExpressionClauseSyntax(node);

        /// <inheritdoc/>
        public InvocableBaseNodePod With(SyntaxToken node)
            => WithSemicolonToken(node);
    }
}
