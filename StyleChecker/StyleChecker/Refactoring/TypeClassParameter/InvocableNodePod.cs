namespace StyleChecker.Refactoring.TypeClassParameter
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The wrapper of <see cref="MethodDeclarationSyntax"/>
    /// and <see cref="LocalFunctionStatementSyntax"/> objects.
    /// </summary>
    public sealed class InvocableNodePod
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

            WithTypeParameterListSyntax
                = With<TypeParameterListSyntax>(node.WithTypeParameterList);
            WithBlockSyntax = With<BlockSyntax>(node.WithBody);
            WithParameterListSyntax
                = With<ParameterListSyntax>(node.WithParameterList);
            WithArrowExpressionClauseSyntax
                = With<ArrowExpressionClauseSyntax>(node.WithExpressionBody);
            WithSemicolonToken = With<SyntaxToken>(node.WithSemicolonToken);
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

            WithTypeParameterListSyntax
                = With<TypeParameterListSyntax>(node.WithTypeParameterList);
            WithBlockSyntax = With<BlockSyntax>(node.WithBody);
            WithParameterListSyntax
                = With<ParameterListSyntax>(node.WithParameterList);
            WithArrowExpressionClauseSyntax
                = With<ArrowExpressionClauseSyntax>(node.WithExpressionBody);
            WithSemicolonToken = With<SyntaxToken>(node.WithSemicolonToken);
        }

        /// <summary>
        /// Gets the parameter list.
        /// </summary>
        public ParameterListSyntax ParameterList { get; }

        /// <summary>
        /// Gets the type parameter list.
        /// </summary>
        public TypeParameterListSyntax TypeParameterList { get; }

        /// <summary>
        /// Gets the body.
        /// </summary>
        public BlockSyntax Body { get; }

        /// <summary>
        /// Gets the expression body.
        /// </summary>
        public ArrowExpressionClauseSyntax ExpressionBody { get; }

        /// <summary>
        /// Gets the return type.
        /// </summary>
        public TypeSyntax ReturnType { get; }

        /// <summary>
        /// Gets the wrapped node.
        /// </summary>
        public SyntaxNode Node { get; }

        private Func<TypeParameterListSyntax, InvocableNodePod>
            WithTypeParameterListSyntax { get; }

        private Func<BlockSyntax, InvocableNodePod>
            WithBlockSyntax { get; }

        private Func<ParameterListSyntax, InvocableNodePod>
            WithParameterListSyntax { get; }

        private Func<ArrowExpressionClauseSyntax, InvocableNodePod>
            WithArrowExpressionClauseSyntax { get; }

        private Func<SyntaxToken, InvocableNodePod>
            WithSemicolonToken { get; }

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

        /// <summary>
        /// Gets a new <see cref="InvocableNodePod"/> object
        /// with the specified <see cref="TypeParameterListSyntax"/>
        /// object.
        /// </summary>
        /// <param name="node">
        /// The <see cref="TypeParameterListSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <see cref="InvocableNodePod"/> object.
        /// </returns>
        public InvocableNodePod With(TypeParameterListSyntax node)
            => WithTypeParameterListSyntax(node);

        /// <summary>
        /// Gets a new <see cref="InvocableNodePod"/> object
        /// with the specified <see cref="BlockSyntax"/>
        /// object.
        /// </summary>
        /// <param name="node">
        /// The <see cref="BlockSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <see cref="InvocableNodePod"/> object.
        /// </returns>
        public InvocableNodePod With(BlockSyntax node)
            => WithBlockSyntax(node);

        /// <summary>
        /// Gets a new <see cref="InvocableNodePod"/> object
        /// with the specified <see cref="ParameterListSyntax"/>
        /// object.
        /// </summary>
        /// <param name="node">
        /// The <see cref="ParameterListSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <see cref="InvocableNodePod"/> object.
        /// </returns>
        public InvocableNodePod With(ParameterListSyntax node)
            => WithParameterListSyntax(node);

        /// <summary>
        /// Gets a new <see cref="InvocableNodePod"/> object
        /// with the specified <see cref="ArrowExpressionClauseSyntax"/>
        /// object.
        /// </summary>
        /// <param name="node">
        /// The <see cref="ArrowExpressionClauseSyntax"/> object.
        /// </param>
        /// <returns>
        /// The new <see cref="InvocableNodePod"/> object.
        /// </returns>
        public InvocableNodePod With(ArrowExpressionClauseSyntax node)
            => WithArrowExpressionClauseSyntax(node);

        public InvocableNodePod With(SyntaxToken node)
            => WithSemicolonToken(node);
    }
}
