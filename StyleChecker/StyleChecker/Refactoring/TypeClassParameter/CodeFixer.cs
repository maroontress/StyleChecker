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
    using Microsoft.CodeAnalysis.Rename;
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

            var document = context.Document;
            var cancellationToken = context.CancellationToken;
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

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
                    createChangedSolution: c => Replace(document, node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static async Task<Solution> Replace(
            Document document,
            ParameterSyntax node,
            CancellationToken cancellationToken)
        {
            var solution = await Replace2(document, node, cancellationToken)
                .ConfigureAwait(false);
            return solution ?? document.Project.Solution;
        }

        private static async Task<Solution?> Replace2(
            Document realDocument,
            ParameterSyntax realNode,
            CancellationToken cancellationToken)
        {
            var documentId = realDocument.Id;
            var realRoot = realNode.SyntaxTree
                .GetRoot(cancellationToken);
            var solution = realDocument.Project
                .Solution
                .WithDocumentSyntaxRoot(
                    documentId, realRoot.TrackNodes(realNode));
            var document = solution.GetDocument(documentId);
            if (document is null)
            {
                return null;
            }
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            if (root is null)
            {
                return null;
            }
            var node = root.FindNode(realNode.Span);

            static async Task<(SemanticModel Model,
                    ISymbol Parameter,
                    IMethodSymbol Method)?>
                GetSymbols(Document d, CancellationToken t, SyntaxNode n)
            {
                var model = await d.GetSemanticModelAsync(t)
                    .ConfigureAwait(false);
                if (model is null)
                {
                    return null;
                }
                var parameter = model.GetDeclaredSymbol(n, t);
                if (parameter is null)
                {
                    return null;
                }
                if (!(parameter.ContainingSymbol is IMethodSymbol method))
                {
                    return null;
                }
                return (model, parameter, method);
            }

            var symbols = await GetSymbols(document, cancellationToken, node)
                .ConfigureAwait(false);
            if (symbols is null)
            {
                return null;
            }
            var (model, parameterSymbol, methodSymbol) = symbols.Value;

            var parent = node.Parent;
            if (parent is null)
            {
                return null;
            }
            var allSymbolNameSet = new HashSet<string>(
                model.LookupSymbols(parent.SpanStart)
                    .Select(s => s.Name));

            string? GetTypeName()
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
                        allSymbolNameSet.Add(n);
                        return n;
                    }
                }
                return null;
            }

            int? GetRenamingIndex(int start, string name)
            {
                for (var k = start; k >= 0; ++k)
                {
                    var n = $"{name}_{k}";
                    if (!allSymbolNameSet.Contains(n))
                    {
                        allSymbolNameSet.Add(n);
                        return k;
                    }
                }
                return null;
            }

            static bool IsSameTypes(
                IEnumerable<IParameterSymbol> p1,
                IEnumerable<IParameterSymbol> p2)
            {
                static ITypeSymbol ToType(IParameterSymbol s) => s.Type;

                var t1 = p1.Select(ToType)
                    .ToArray();
                var t2 = p2.Select(ToType)
                    .ToArray();
                if (t1.Length != t2.Length)
                {
                    return false;
                }
                var n = t1.Length;
                for (var k = 0; k < n; ++k)
                {
                    if (!t1[k].Equals(t2[k]))
                    {
                        return false;
                    }
                }
                return true;
            }

            var typeName = GetTypeName();
            if (typeName is null)
            {
                return null;
            }

            ISymbol[] GetNamesakes(string name, IMethodSymbol method)
            {
                if (!(method.ContainingSymbol is INamedTypeSymbol namedType))
                {
                    return Array.Empty<ISymbol>();
                }

                var newTypeParameterLength
                    = methodSymbol.TypeParameters.Length + 1;
                var newParameters = methodSymbol.Parameters
                    .Where(p => !p.Equals(parameterSymbol));
                return namedType.GetMembers()
                    .Where(m => m is IMethodSymbol s
                        && s.Name == name
                        && s.TypeParameters.Length == newTypeParameterLength
                        && IsSameTypes(s.Parameters, newParameters))
                    .ToArray();
            }

            var name = methodSymbol.Name;
            var namesakes = GetNamesakes(name, methodSymbol);
            if (namesakes.Any())
            {
                var s = namesakes[0];
                var optionSet = solution.Workspace.Options;
                var k = GetRenamingIndex(0, name);
                if (k is null)
                {
                    return null;
                }
                solution = await Renamer.RenameSymbolAsync(
                        solution,
                        s,
                        $"{name}_{k.Value}",
                        optionSet,
                        cancellationToken)
                    .ConfigureAwait(false);
                var projectId = document.Project.Id;
                var project = solution.GetProject(projectId);
                if (project is null)
                {
                    return null;
                }
                document = project.Documents
                    .Where(d => d.Id == documentId)
                    .First();
                root = await document.GetSyntaxRootAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (root is null)
                {
                    return null;
                }
                node = root.GetCurrentNode(realNode);

                symbols = await GetSymbols(document, cancellationToken, node)
                    .ConfigureAwait(false);
                if (symbols is null)
                {
                    return null;
                }
                (model, parameterSymbol, methodSymbol) = symbols.Value;
            }

            var parameterArray = methodSymbol.Parameters
                .ToArray();
            var index = Array.FindIndex(
                parameterArray, p => p.Equals(parameterSymbol));
            if (index == -1)
            {
                return null;
            }

            var allReferences = await SymbolFinder.FindReferencesAsync(
                    methodSymbol, solution, cancellationToken)
                .ConfigureAwait(false);
            var documentGroups = allReferences.SelectMany(r => r.Locations)
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
                return null;
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

        private static SyntaxNode? UpdateMainDocument(
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
                var d = newSolution.GetDocument(g.Key.Id);
                if (d is null)
                {
                    continue;
                }
                var root = await d.GetSyntaxRootAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (root is null)
                {
                    continue;
                }
                var invocations = g
                    .Select(w => root.FindNode(w.Location.SourceSpan))
                    .Select(n => n.Parent?.Parent)
                    .OfType<InvocationExpressionSyntax>();
                var changeMap = new Dictionary<SyntaxNode, SyntaxNode>();

                UpdateReferencingInvocators(index, invocations, changeMap);

                var newRoot = root.ReplaceNodes(
                    changeMap.Keys,
                    (original, node) => changeMap[original]);
                newSolution = newSolution.WithDocumentSyntaxRoot(
                    d.Id, newRoot);
            }
            return newSolution;
        }

        private static XmlNameAttributeSyntax? GetNameAttribute(
            SyntaxNode node, string parameterId)
        {
            static bool Equals(SyntaxToken t, string s) => t.ValueText == s;

            static string GetTagName(XmlElementSyntax n)
                => n.StartTag.Name.LocalName.ValueText;

            static string GetAttributeName(XmlAttributeSyntax n)
                => n.Name.LocalName.ValueText;

            static T GetAttribute<T>(XmlElementSyntax n, string name)
                where T : XmlAttributeSyntax
                => n.StartTag.Attributes
                    .Where(a => GetAttributeName(a) == name)
                    .OfType<T>()
                    .FirstOrDefault();

            static XmlNameAttributeSyntax? GetAttributeOf(
                XmlElementSyntax n, string name, string value)
            {
                var v = GetAttribute<XmlNameAttributeSyntax>(n, name);
                return (!(v is null) && Equals(v.Identifier.Identifier, value))
                    ? v : null;
            }

            XmlNameAttributeSyntax? ToAttribute(XmlElementSyntax n)
                => !(GetTagName(n) is ParamName)
                    ? null : GetAttributeOf(n, "name", parameterId);

            return node.GetFirstToken()
                .LeadingTrivia
                .Where(t => t.IsKindOneOf(SldcTriviaKind, MldcTriviaKind))
                .Select(t => t.GetStructure())
                .OfType<SyntaxNode>()
                .SelectMany(t => t.DescendantNodes())
                .Where(n => n.IsKind(SyntaxKind.XmlElement))
                .OfType<XmlElementSyntax>()
                .Select(ToAttribute)
                .FirstOrDefault(a => !(a is null));
        }

        private static SyntaxNode ReplaceDocumentComment(
            SyntaxNode node, string parameterId, string typeName)
        {
            var nameAttribute = GetNameAttribute(node, parameterId);
            if (nameAttribute is null
                || !(nameAttribute.Parent?.Parent
                    is XmlElementSyntax paramElement))
            {
                return node;
            }
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
                .WithoutArrowExpressionClauseSyntax()
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
