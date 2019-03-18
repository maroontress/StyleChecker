namespace StyleChecker.Invocables
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The wrapper of <see cref="MethodDeclarationSyntax"/>
    /// and <see cref="LocalFunctionStatementSyntax"/> objects.
    /// </summary>
    public sealed class InvocableNodePod : InvocableProperties,
        InvocablePrototype<InvocableNodePod>
    {
        private InvocableNodePod(MethodDeclarationSyntax node)
        {
            Node = node;
            ParameterList = node.ParameterList;
            TypeParameterList = node.TypeParameterList;
            Body = node.Body;
            ExpressionBody = node.ExpressionBody;
            ReturnType = node.ReturnType;

            Func<T, InvocableNodePod> With<T>(
                Func<T, MethodDeclarationSyntax> with)
                => n => new InvocableNodePod(with(n));

            WithBlockSyntax = With<BlockSyntax>(node.WithBody);
            WithParameterListSyntax
                = With<ParameterListSyntax>(node.WithParameterList);
            WithArrowExpressionClauseSyntax
                = With<ArrowExpressionClauseSyntax>(node.WithExpressionBody);
            WithSemicolonToken = With<SyntaxToken>(node.WithSemicolonToken);
            WithTypeParameterListSyntax
                = With<TypeParameterListSyntax>(node.WithTypeParameterList);
        }

        private InvocableNodePod(LocalFunctionStatementSyntax node)
        {
            Node = node;
            ParameterList = node.ParameterList;
            TypeParameterList = node.TypeParameterList;
            Body = node.Body;
            ExpressionBody = node.ExpressionBody;
            ReturnType = node.ReturnType;

            Func<T, InvocableNodePod> With<T>(
                Func<T, LocalFunctionStatementSyntax> with)
                => n => new InvocableNodePod(with(n));

            WithBlockSyntax = With<BlockSyntax>(node.WithBody);
            WithParameterListSyntax
                = With<ParameterListSyntax>(node.WithParameterList);
            WithArrowExpressionClauseSyntax
                = With<ArrowExpressionClauseSyntax>(node.WithExpressionBody);
            WithSemicolonToken = With<SyntaxToken>(node.WithSemicolonToken);
            WithTypeParameterListSyntax
                = With<TypeParameterListSyntax>(node.WithTypeParameterList);
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
        public override TypeParameterListSyntax TypeParameterList { get; }

        /// <inheritdoc/>
        public override TypeSyntax ReturnType { get; }

        private Func<BlockSyntax, InvocableNodePod>
            WithBlockSyntax { get; }

        private Func<ParameterListSyntax, InvocableNodePod>
            WithParameterListSyntax { get; }

        private Func<ArrowExpressionClauseSyntax, InvocableNodePod>
            WithArrowExpressionClauseSyntax { get; }

        private Func<SyntaxToken, InvocableNodePod>
            WithSemicolonToken { get; }

        private Func<TypeParameterListSyntax, InvocableNodePod>
            WithTypeParameterListSyntax { get; }

        /// <summary>
        /// Gets a new <see cref="InvocableNodePod"/> object wrappings the
        /// specified <see cref="SyntaxNode"/> node.
        /// </summary>
        /// <param name="node">
        /// The <see cref="LocalFunctionStatementSyntax"/> or
        /// <see cref="MethodDeclarationSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <see cref="InvocableNodePod"/> object.
        /// </returns>
        public static InvocableNodePod Of(SyntaxNode node)
        {
            return node is LocalFunctionStatementSyntax localFunction
                ? new InvocableNodePod(localFunction)
                : node is MethodDeclarationSyntax method
                    ? new InvocableNodePod(method)
                    : null;
        }

        /// <inheritdoc/>
        public InvocableNodePod With(BlockSyntax node)
            => WithBlockSyntax(node);

        /// <inheritdoc/>
        public InvocableNodePod With(ParameterListSyntax node)
            => WithParameterListSyntax(node);

        /// <inheritdoc/>
        public InvocableNodePod With(ArrowExpressionClauseSyntax node)
            => WithArrowExpressionClauseSyntax(node);

        /// <inheritdoc/>
        public InvocableNodePod With(SyntaxToken node)
            => WithSemicolonToken(node);

        public InvocableNodePod With(TypeParameterListSyntax node)
            => WithTypeParameterListSyntax(node);
    }
}
