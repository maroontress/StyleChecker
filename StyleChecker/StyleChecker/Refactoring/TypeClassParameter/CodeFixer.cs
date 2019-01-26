namespace StyleChecker.Refactoring.TypeClassParameter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.FindSymbols;
    using Microsoft.CodeAnalysis.Formatting;
    using R = Resources;

    /// <summary>
    /// TypeClassParameter CodeFix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            var title = localize(nameof(R.FixTitle)).ToString();

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var token = root.FindToken(diagnosticSpan.Start);
            var node = token.Parent;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedSolution:
                        c => Replace(context.Document, node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static async Task<Solution> Replace(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken)
                .ConfigureAwait(false);
            var parameterNode = node as ParameterSyntax;
            var parameterSymbol = model.GetDeclaredSymbol(parameterNode);
            var methodSymbol
                = parameterSymbol.ContainingSymbol as IMethodSymbol;
            var allReferences = await SymbolFinder.FindReferencesAsync(
                    methodSymbol,
                    document.Project.Solution,
                    cancellationToken)
                .ConfigureAwait(false);
            var documentGroups = allReferences
                .SelectMany(r => r.Locations)
                .GroupBy(w => w.Document);
            var changeMap = new Dictionary<SyntaxNode, SyntaxNode>();

            var parameterArray = methodSymbol.Parameters.ToArray();
            var index = Array.FindIndex(
                parameterArray, p => p.Equals(parameterSymbol));
            var solution = document.Project.Solution;
            if (index == -1)
            {
                return solution;
            }

            UpdateMainDocument(
                changeMap,
                document,
                root,
                methodSymbol,
                index,
                documentGroups);
            var newRoot = root.ReplaceNodes(
                changeMap.Keys,
                (original, n) => changeMap[original]);

            var workspace = solution.Workspace;
            var formattedNode = Formatter.Format(
               newRoot,
               Formatter.Annotation,
               workspace,
               workspace.Options);
            var newSolution
                = solution.WithDocumentSyntaxRoot(document.Id, formattedNode);
            return await UpdateReferencingDocumentsAsync(
                    document,
                    index,
                    documentGroups,
                    newSolution,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private static bool UpdateLocalFunction(
            Dictionary<SyntaxNode, SyntaxNode> changeMap,
            LocalFunctionStatementSyntax localFunctionNode,
            int index)
        {
            // Removes the parameter.
            var parameterList = localFunctionNode.ParameterList;
            var parameterNodeList = parameterList.Parameters;
            var newParameterNodeList = parameterNodeList.RemoveAt(index);
            var newParameterList
                = parameterList.WithParameters(newParameterNodeList);

            // Adds the type parameter.
            var typeParameterList = localFunctionNode.TypeParameterList;
            var deltaParameter = SyntaxFactory.TypeParameter("T");
            var newTypeParameterList = (typeParameterList == null)
                ? SyntaxFactory.TypeParameterList(
                    SyntaxFactory.SingletonSeparatedList(deltaParameter))
                : typeParameterList.AddParameters(deltaParameter);

            // Add "var ID = typeof(T);"
            var id = parameterNodeList[index].Identifier;
            var statement = SyntaxFactory.ParseStatement(
                $"var {id.ValueText} = typeof(T);{Environment.NewLine}");
            var body = localFunctionNode.Body;
            if (body == null)
            {
                return false;
            }
            var statements = body.Statements;
            var newStatements = statements.Insert(0, statement);
            var newBody = body.WithStatements(newStatements)
                .WithAdditionalAnnotations(Formatter.Annotation);

            // Replaces the old node with the new node.
            var newLocalFunctionNode = localFunctionNode
                .WithTypeParameterList(newTypeParameterList)
                .WithBody(newBody)
                .WithParameterList(newParameterList)
                .WithAdditionalAnnotations(Formatter.Annotation);
            changeMap[localFunctionNode] = newLocalFunctionNode;
            return true;
        }

        private static bool UpdateMethod(
            Dictionary<SyntaxNode, SyntaxNode> changeMap,
            MethodDeclarationSyntax methodNode,
            int index)
        {
            // Removes the parameter.
            var parameterList = methodNode.ParameterList;
            var parameterNodeList = parameterList.Parameters;
            var newParameterNodeList = parameterNodeList.RemoveAt(index);
            var newParameterList
                = parameterList.WithParameters(newParameterNodeList);

            // Adds the type parameter.
            var typeParameterList = methodNode.TypeParameterList;
            var deltaParameter = SyntaxFactory.TypeParameter("T");
            var newTypeParameterList = (typeParameterList == null)
                ? SyntaxFactory.TypeParameterList(
                    SyntaxFactory.SingletonSeparatedList(deltaParameter))
                : typeParameterList.AddParameters(deltaParameter);

            // Add "var ID = typeof(T);"
            var id = parameterNodeList[index].Identifier;
            var statement = SyntaxFactory.ParseStatement(
                $"var {id.ValueText} = typeof(T);{Environment.NewLine}");
            var body = methodNode.Body;
            if (body == null)
            {
                return false;
            }
            var statements = body.Statements;
            var newStatements = statements.Insert(0, statement);
            var newBody = body.WithStatements(newStatements)
                .WithAdditionalAnnotations(Formatter.Annotation);

            // Replaces the old node with the new node.
            var newMethodNode = methodNode
                .WithTypeParameterList(newTypeParameterList)
                .WithBody(newBody)
                .WithParameterList(newParameterList)
                .WithAdditionalAnnotations(Formatter.Annotation);
            changeMap[methodNode] = newMethodNode;
            return true;
        }

        private static void UpdateMainDocument(
            Dictionary<SyntaxNode, SyntaxNode> changeMap,
            Document document,
            SyntaxNode root,
            IMethodSymbol targetMethod,
            int index,
            IEnumerable<IGrouping<Document, ReferenceLocation>> documentGroups)
        {
            var reference = targetMethod.DeclaringSyntaxReferences.FirstOrDefault();
            if (reference == null)
            {
                return;
            }
            var node = reference.GetSyntax();
            if (node is LocalFunctionStatementSyntax localFunctionNode
                && !UpdateLocalFunction(changeMap, localFunctionNode, index))
            {
                return;
            }
            else if (node is MethodDeclarationSyntax methodNode
                && !UpdateMethod(changeMap, methodNode, index))
            {
                return;
            }

            var mainDocumentGroup = documentGroups
                .FirstOrDefault(g => g.Key.Equals(document));
            if (mainDocumentGroup == null)
            {
                return;
            }
            var invocations = mainDocumentGroup
                .Select(w => root.FindNode(w.Location.SourceSpan))
                .Select(n => n.Parent)
                .OfType<InvocationExpressionSyntax>();
            foreach (var i in invocations)
            {
                var targetArgument = i.ArgumentList.Arguments[index];
                if (!(targetArgument.Expression
                    is TypeOfExpressionSyntax typeOfExpression))
                {
                    continue;
                }
                var additionalType = typeOfExpression.Type;

                var argumentList = i.ArgumentList;
                var newArguments = argumentList.Arguments.RemoveAt(index);
                var newArgumentList
                    = argumentList.WithArguments(newArguments);

                void Replace(ExpressionSyntax newExpression)
                {
                    changeMap[i] = i.WithExpression(newExpression)
                        .WithArgumentList(newArgumentList);
                }

                if (i.Expression is IdentifierNameSyntax nonGeneric)
                {
                    var newTypeArguments = SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(additionalType));
                    var newExpression = SyntaxFactory
                        .GenericName(nonGeneric.Identifier)
                        .WithTypeArgumentList(newTypeArguments);
                    Replace(newExpression);
                    continue;
                }
                if (i.Expression is GenericNameSyntax generic)
                {
                    var newTypeArguments = generic.TypeArgumentList
                        .AddArguments(additionalType);
                    var newExpression = generic
                        .WithTypeArgumentList(newTypeArguments);
                    Replace(newExpression);
                    continue;
                }
            }
        }

        private static async Task<Solution> UpdateReferencingDocumentsAsync(
            Document document,
            int index,
            IEnumerable<IGrouping<Document, ReferenceLocation>> documentGroups,
            Solution solution,
            CancellationToken cancellationToken)
        {
            var newSolution = solution;
            var groups = documentGroups
                .Where(g => !g.Key.Equals(document));
            foreach (var g in groups)
            {
                var d = g.Key;
                d = newSolution.GetDocument(d.Id);
                var root = await d.GetSyntaxRootAsync(cancellationToken)
                    .ConfigureAwait(false);
                var invocations = g
                    .Select(w => root.FindNode(w.Location.SourceSpan))
                    .Select(n => n.Parent)
                    .OfType<InvocationExpressionSyntax>();
                var changeMap = new Dictionary<SyntaxNode, SyntaxNode>();

                foreach (var i in invocations)
                {
                    var targetArgument = i.ArgumentList.Arguments[index];
                    if (!(targetArgument.Expression
                        is TypeOfExpressionSyntax typeOfExpression))
                    {
                        continue;
                    }
                    var additionalType = typeOfExpression.Type;

                    var argumentList = i.ArgumentList;
                    var newArguments = argumentList.Arguments.RemoveAt(index);
                    var newArgumentList
                        = argumentList.WithArguments(newArguments);

                    void Replace(ExpressionSyntax newExpression)
                    {
                        changeMap[i] = i.WithExpression(newExpression)
                            .WithArgumentList(newArgumentList);
                    }

                    if (i.Expression is IdentifierNameSyntax nonGeneric)
                    {
                        var newTypeArguments = SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(additionalType));
                        var newExpression = SyntaxFactory
                            .GenericName(nonGeneric.Identifier)
                            .WithTypeArgumentList(newTypeArguments);
                        Replace(newExpression);
                        continue;
                    }
                    if (i.Expression is GenericNameSyntax generic)
                    {
                        var newTypeArguments = generic.TypeArgumentList
                            .AddArguments(additionalType);
                        var newExpression = generic
                            .WithTypeArgumentList(newTypeArguments);
                        Replace(newExpression);
                        continue;
                    }
                }
                root = root.ReplaceNodes(
                    changeMap.Keys,
                    (original, node) => changeMap[original]);
                newSolution = newSolution.WithDocumentSyntaxRoot(d.Id, root);
            }
            return newSolution;
        }
    }
}
