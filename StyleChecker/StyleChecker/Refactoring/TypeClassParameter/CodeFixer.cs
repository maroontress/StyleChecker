namespace StyleChecker.Refactoring.TypeClassParameter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Globalization;
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
    using StyleChecker.Invocables;
    using R = Resources;

    /// <summary>
    /// TypeClassParameter CodeFix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
        private const string ParamName = "param";
        private const string TypeparamName = "typeparam";

        private const SyntaxKind MldcTriviaKind
            = SyntaxKind.MultiLineDocumentationCommentTrivia;

        private const SyntaxKind SldcTriviaKind
            = SyntaxKind.SingleLineDocumentationCommentTrivia;

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
            var title = localize(nameof(R.FixTitle))
                .ToString(CultureInfo.CurrentCulture);

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNodeOfType<ParameterSyntax>(diagnosticSpan);
            if (node is null)
            {
                return;
            }

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
            ParameterSyntax node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            var model = await document.GetSemanticModelAsync(cancellationToken)
                .ConfigureAwait(false);
            var parameterSymbol = model.GetDeclaredSymbol(node);
            var methodSymbol
                = parameterSymbol.ContainingSymbol as IMethodSymbol;
            var allSymbolNameSet = model
                .LookupSymbols(node.Parent.SpanStart)
                .Select(s => s.Name)
                .ToImmutableHashSet();

            string GetTypeName()
            {
                var name = "T";
                if (!allSymbolNameSet.Contains(name))
                {
                    return name;
                }
                for (var k = 0; k >= 0; ++k)
                {
                    var n = $"{name}{k}";
                    if (!allSymbolNameSet.Contains(n))
                    {
                        return n;
                    }
                }
                return null;
            }

            var solution = document.Project.Solution;
            var typeName = GetTypeName();
            var parameterArray = methodSymbol.Parameters.ToArray();
            var index = Array.FindIndex(
                parameterArray, p => p.Equals(parameterSymbol));
            if (typeName is null
                || index == -1)
            {
                return solution;
            }
            var allReferences = await SymbolFinder.FindReferencesAsync(
                    methodSymbol,
                    document.Project.Solution,
                    cancellationToken)
                .ConfigureAwait(false);
            var documentGroups = allReferences
                .SelectMany(r => r.Locations)
                .GroupBy(w => w.Document);
            var newRoot = UpdateMainDocument(
                typeName,
                document,
                root,
                methodSymbol,
                index,
                documentGroups);
            if (newRoot is null)
            {
                return solution;
            }
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

        private static SyntaxNode UpdateMainDocument(
            string typeName,
            Document document,
            SyntaxNode root,
            IMethodSymbol targetMethod,
            int index,
            IEnumerable<IGrouping<Document, ReferenceLocation>> documentGroups)
        {
            var reference = targetMethod.DeclaringSyntaxReferences
                .FirstOrDefault();
            if (reference is null)
            {
                return null;
            }
            var node = reference.GetSyntax();
            var pod = InvocableNodePod.Of(node);
            if (pod is null)
            {
                return null;
            }
            var changeMap = new Dictionary<SyntaxNode, SyntaxNode>();
            if (!UpdateInvocableNode(typeName, changeMap, pod, index))
            {
                return null;
            }

            var mainDocumentGroup = documentGroups
                .FirstOrDefault(g => g.Key.Equals(document));
            if (!(mainDocumentGroup is null))
            {
                var invocations = mainDocumentGroup
                    .Select(w => root.FindNode(w.Location.SourceSpan))
                    .Select(n => n.Parent)
                    .OfType<InvocationExpressionSyntax>();
                UpdateReferencingInvocators(index, invocations, changeMap);
            }
            return root.ReplaceNodes(
                changeMap.Keys,
                (original, n) => changeMap[original]);
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
                    .Select(n => n.Parent.Parent)
                    .OfType<InvocationExpressionSyntax>();
                var changeMap = new Dictionary<SyntaxNode, SyntaxNode>();

                UpdateReferencingInvocators(index, invocations, changeMap);

                root = root.ReplaceNodes(
                    changeMap.Keys,
                    (original, node) => changeMap[original]);
                newSolution = newSolution.WithDocumentSyntaxRoot(d.Id, root);
            }
            return newSolution;
        }

        private static XmlNameAttributeSyntax GetNameAttribute(
            SyntaxNode node, string parameterId)
        {
            bool Equals(SyntaxToken t, string s) => t.ValueText == s;

            string GetTagName(XmlElementSyntax n)
                => n.StartTag.Name.LocalName.ValueText;

            string GetAttributeName(XmlAttributeSyntax n)
                => n.Name.LocalName.ValueText;

            T GetAttribute<T>(XmlElementSyntax n, string name)
                where T : XmlAttributeSyntax
                => n.StartTag.Attributes
                    .Where(a => GetAttributeName(a) == name)
                    .OfType<T>()
                    .FirstOrDefault();

            XmlNameAttributeSyntax GetAttributeOf(
                XmlElementSyntax n, string name, string value)
            {
                var v = GetAttribute<XmlNameAttributeSyntax>(n, name);
                return (!(v is null) && Equals(v.Identifier.Identifier, value))
                    ? v : null;
            }

            XmlNameAttributeSyntax ToAttribute(XmlElementSyntax n)
                => !(GetTagName(n) is ParamName)
                    ? null : GetAttributeOf(n, "name", parameterId);

            return node.GetFirstToken()
                .LeadingTrivia
                .Where(t => t.IsKindOneOf(SldcTriviaKind, MldcTriviaKind))
                .SelectMany(t => t.GetStructure().DescendantNodes())
                .Where(n => n.IsKind(SyntaxKind.XmlElement))
                .OfType<XmlElementSyntax>()
                .Select(ToAttribute)
                .FirstOrDefault(a => !(a is null));
        }

        private static SyntaxNode ReplaceDocumentComment(
            SyntaxNode node, string parameterId, string typeName)
        {
            var nameAttribute = GetNameAttribute(node, parameterId);
            if (nameAttribute is null)
            {
                return node;
            }
            var paramElement = nameAttribute.Parent.Parent
                as XmlElementSyntax;
            var newNameAttribute = SyntaxFactory.XmlNameAttribute("name")
                .WithIdentifier(SyntaxFactory.IdentifierName(typeName));
            var newAttributes = paramElement.StartTag.Attributes
                .Replace(nameAttribute, newNameAttribute);
            var newTagName = SyntaxFactory.XmlName(TypeparamName);
            var newParamElement = paramElement
                .WithStartTag(SyntaxFactory.XmlElementStartTag(newTagName)
                    .WithAttributes(newAttributes))
                .WithEndTag(SyntaxFactory.XmlElementEndTag(newTagName));
            return node.ReplaceNode(paramElement, newParamElement);
        }

        private static bool UpdateInvocableNode(
            string typeName,
            Dictionary<SyntaxNode, SyntaxNode> changeMap,
            InvocableNodePod nodePod,
            int index)
        {
            // Removes the parameter.
            var parameterList = nodePod.ParameterList;
            var parameterNodeList = parameterList.Parameters;
            var newParameterNodeList = parameterNodeList.RemoveAt(index);
            var newParameterList
                = parameterList.WithParameters(newParameterNodeList);

            // Adds the type parameter.
            var typeParameterList = nodePod.TypeParameterList;
            var deltaParameter = SyntaxFactory.TypeParameter(typeName);
            var newTypeParameterList = (typeParameterList is null)
                ? SyntaxFactory.TypeParameterList(
                    SyntaxFactory.SingletonSeparatedList(deltaParameter))
                : typeParameterList.AddParameters(deltaParameter);

            // Add "var ID = typeof(T);"
            var parameterId = parameterNodeList[index].Identifier;
            var statement = SyntaxFactory.ParseStatement(
                $"var {parameterId.ValueText} = typeof({typeName});"
                + $"{Environment.NewLine}");
            var body = nodePod.Body;
            if (body is null)
            {
                var expressionBody = nodePod.ExpressionBody;
                if (expressionBody is null)
                {
                    return false;
                }
                var e = expressionBody.Expression;
                var s = (nodePod.ReturnType is PredefinedTypeSyntax returnType
                        && returnType.Keyword.ValueText == "void")
                    ? SyntaxFactory.ExpressionStatement(e)
                    : SyntaxFactory.ReturnStatement(e) as StatementSyntax;
                body = SyntaxFactory.Block(s);
            }
            var statements = body.Statements;
            var newStatements = statements.Insert(0, statement);
            var newBody = body.WithStatements(newStatements)
                .WithAdditionalAnnotations(Formatter.Annotation);

            // Replaces the old node with the new node.
            var node = nodePod.Node;
            var newNode = nodePod
                .With(newTypeParameterList)
                .With(newParameterList)
                .With(newBody)
                .With(default(ArrowExpressionClauseSyntax))
                .With(default(SyntaxToken))
                .Node;
            newNode = ReplaceDocumentComment(
                newNode, parameterId.ValueText, typeName);
            changeMap[node]
                = newNode.WithAdditionalAnnotations(Formatter.Annotation);
            return true;
        }

        private static void UpdateReferencingInvocators(
            int index,
            IEnumerable<InvocationExpressionSyntax> invocations,
            Dictionary<SyntaxNode, SyntaxNode> changeMap)
        {
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

                GenericNameSyntax NewGenericName(IdentifierNameSyntax name)
                {
                    var newTypeArguments = SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(additionalType));
                    return SyntaxFactory.GenericName(name.Identifier)
                        .WithTypeArgumentList(newTypeArguments);
                }

                GenericNameSyntax AppendTypeArgument(GenericNameSyntax name)
                {
                    var newTypeArguments = name.TypeArgumentList
                        .AddArguments(additionalType);
                    return name.WithTypeArgumentList(newTypeArguments);
                }

                var expression = i.Expression;
                Func<SimpleNameSyntax, ExpressionSyntax> map = s => s;
                if (expression is MemberAccessExpressionSyntax access)
                {
                    expression = access.Name;
                    map = access.WithName;
                }
                if (expression is IdentifierNameSyntax nonGeneric)
                {
                    Replace(map(NewGenericName(nonGeneric)));
                    continue;
                }
                if (expression is GenericNameSyntax generic)
                {
                    Replace(map(AppendTypeArgument(generic)));
                    continue;
                }
            }
        }
    }
}
