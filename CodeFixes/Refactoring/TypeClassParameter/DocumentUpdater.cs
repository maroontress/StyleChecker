namespace StyleChecker.CodeFixes.Refactoring.TypeClassParameter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maroontress.Roastery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using StyleChecker.Analyzers;
using StyleChecker.Analyzers.Invocables;

/// <summary>
/// Provides methods to update the main document and referencing documents.
/// </summary>
public static class DocumentUpdater
{
    private const string ParamName = "param";
    private const string TypeparamName = "typeparam";

    private const SyntaxKind MldcTriviaKind
        = SyntaxKind.MultiLineDocumentationCommentTrivia;

    private const SyntaxKind SldcTriviaKind
        = SyntaxKind.SingleLineDocumentationCommentTrivia;

    private static readonly Func<SimpleNameSyntax, ExpressionSyntax>
        Identity = s => s;

    /// <summary>
    /// Updates the main document by modifying the invocable node.
    /// </summary>
    /// <param name="typeName">
    /// The name of the type parameter.
    /// </param>
    /// <param name="mainDocument">
    /// The document to update.
    /// </param>
    /// <param name="root">
    /// The root syntax node of the document.
    /// </param>
    /// <param name="targetMethod">
    /// The target method symbol.
    /// </param>
    /// <param name="index">
    /// The index of the parameter to update.
    /// </param>
    /// <param name="documentGroups">
    /// The groups of documents and reference locations.
    /// </param>
    /// <returns>
    /// The updated syntax node of the main document.
    /// </returns>
    public static SyntaxNode? UpdateMainDocument(
        string typeName,
        Document mainDocument,
        SyntaxNode root,
        IMethodSymbol targetMethod,
        int index,
        IEnumerable<IGrouping<Document, ReferenceLocation>> documentGroups)
    {
        static InvocableNodePod? ToPod(IMethodSymbol m)
        {
            return (m.DeclaringSyntaxReferences
                     .FirstOrDefault() is not {} reference)
                 ? null
                 : InvocableNodePod.Of(reference.GetSyntax());
        }

        static Func<ReferenceLocation, SyntaxNode?> NewSyntaxNodeSupplier(
            SyntaxNode root)
        {
            return w => root.FindNode(w.Location.SourceSpan)
                .Parent;
        }

        if (ToPod(targetMethod) is not {} pod)
        {
            return null;
        }
        var changeMap = new Dictionary<SyntaxNode, SyntaxNode>();
        if (!UpdateInvocableNode(typeName, changeMap, pod, index))
        {
            return null;
        }
        var toSyntaxNode = NewSyntaxNodeSupplier(root);
        var invocations = documentGroups.Where(g => g.Key.Equals(mainDocument))
            .Take(1)
            .SelectMany(g => g.Select(toSyntaxNode))
            .OfType<InvocationExpressionSyntax>();
        UpdateReferencingInvocators(index, invocations, changeMap);
        return root.ReplaceNodes(
            changeMap.Keys, (original, ignored) => changeMap[original]);
    }

    /// <summary>
    /// Updates the referencing documents asynchronously.
    /// </summary>
    /// <param name="document">
    /// The document to update.
    /// </param>
    /// <param name="index">
    /// The index of the parameter to update.
    /// </param>
    /// <param name="documentGroups">
    /// The groups of documents and reference locations.
    /// </param>
    /// <param name="solution">
    /// The solution containing the documents.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// The updated solution.
    /// </returns>
    public static async Task<Solution> UpdateReferencingDocumentsAsync(
        Document document,
        int index,
        IEnumerable<IGrouping<Document, ReferenceLocation>> documentGroups,
        Solution solution,
        CancellationToken cancellationToken)
    {
        static Func<ReferenceLocation, SyntaxNode?> NewSyntaxNodeSupplier(
            SyntaxNode root)
        {
            return w => root.FindNode(w.Location.SourceSpan)
                .Parent?.Parent;
        }

        async Task<IEnumerable<(DocumentId Id, SyntaxNode Root)>>
            ToNewDocument(IGrouping<Document, ReferenceLocation> g)
        {
            var id = g.Key.Id;
            if (solution.GetDocument(id) is not {} d
                || await d.GetSyntaxRootAsync(cancellationToken)
                    .ConfigureAwait(false) is not {} root)
            {
                return [];
            }
            var toSyntaxNode = NewSyntaxNodeSupplier(root);
            var invocations = g.Select(toSyntaxNode)
                .OfType<InvocationExpressionSyntax>();
            var changeMap = new Dictionary<SyntaxNode, SyntaxNode>();
            UpdateReferencingInvocators(index, invocations, changeMap);
            var newRoot = root.ReplaceNodes(
                changeMap.Keys, (original, ignored) => changeMap[original]);
            return [(id, newRoot)];
        }

        var allTasks = documentGroups
            .Where(g => !g.Key.Equals(document))
            .Select(ToNewDocument);
        var all = (await Task.WhenAll(allTasks))
            .SelectMany(t => t)
            .ToList();
        var newSolution = solution;
        foreach (var (id, root) in all)
        {
            newSolution = newSolution.WithDocumentSyntaxRoot(id, root);
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
            return (v is not null && Equals(v.Identifier.Identifier, value))
                ? v : null;
        }

        XmlNameAttributeSyntax? ToAttribute(XmlElementSyntax n)
            => GetTagName(n) is not ParamName
                ? null : GetAttributeOf(n, "name", parameterId);

        return node.GetFirstToken()
            .LeadingTrivia
            .Where(t => t.IsKindOneOf(SldcTriviaKind, MldcTriviaKind))
            .Select(t => t.GetStructure())
            .FilterNonNullReference()
            .SelectMany(t => t.DescendantNodes())
            .Where(n => n.IsKind(SyntaxKind.XmlElement))
            .OfType<XmlElementSyntax>()
            .Select(ToAttribute)
            .FirstOrDefault(a => a is not null);
    }

    private static SyntaxNode ReplaceDocumentComment(
        SyntaxNode node, string parameterId, string typeName)
    {
        if (GetNameAttribute(node, parameterId) is not {} nameAttribute
            || nameAttribute.Parent?.Parent
                is not XmlElementSyntax paramElement)
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
            + Platforms.NewLine());
        if (nodePod.Body is not {} body)
        {
            if (nodePod.ExpressionBody is not {} expressionBody)
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
            if (targetArgument.Expression
                is not TypeOfExpressionSyntax typeOfExpression)
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

            var expressionPart = i.Expression;
            var (expression, map) = expressionPart switch
            {
                MemberAccessExpressionSyntax access
                    => (access.Name, access.WithName),
                _ => (expressionPart, Identity),
            };

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
