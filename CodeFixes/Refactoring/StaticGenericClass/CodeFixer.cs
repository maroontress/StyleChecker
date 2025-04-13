namespace StyleChecker.CodeFixes.Refactoring.StaticGenericClass;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maroontress.Roastery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using StyleChecker.Analyzers;
using StyleChecker.Analyzers.Refactoring.StaticGenericClass;
using StyleChecker.CodeFixes;
using R = Resources;

/// <summary>
/// StaticGenericClass CodeFix provider.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
[Shared]
public sealed class CodeFixer : AbstractCodeFixProvider
{
    private const string TypeparamName = "typeparam";

    private const string SldcPrefix = "/// ";

    private const string SldcExteriorSuffix = "///";

    private const SyntaxKind MldcTriviaKind
        = SyntaxKind.MultiLineDocumentationCommentTrivia;

    private const SyntaxKind SldcTriviaKind
        = SyntaxKind.SingleLineDocumentationCommentTrivia;

    private const SyntaxKind DceTriviaKind
        = SyntaxKind.DocumentationCommentExteriorTrivia;

    private static readonly XmlTextSyntax XmlEol
        = SyntaxFactory.XmlText(
            SyntaxFactory.XmlTextNewLine(Platforms.NewLine(), false));

    private static readonly XmlTextSyntax XmlSldcPrefix
        = SyntaxFactory.XmlText(SldcPrefix);

    private static readonly IReadOnlyDictionary<SyntaxKind, CommentKit>
        CommentKitMap = new Dictionary<SyntaxKind, CommentKit>()
        {
            [SldcTriviaKind] = NewSldcLeadingTrivia,
            [MldcTriviaKind] = NewMldcLeadingTrivia,
            [default] = NewLeadingTriviaFromScratch,
        };

    private delegate SyntaxTriviaList CommentKit(
        SyntaxToken token,
        IEnumerable<XmlElementSyntax> documentComments);

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds
        => [Analyzer.DiagnosticId];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        var title = localize(nameof(R.FixTitle))
            .ToString(CompilerCulture);

        if (await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var token = root.FindToken(diagnosticSpan.Start);
        if (token.Parent is not ClassDeclarationSyntax node)
        {
            return;
        }

        var action = CodeAction.Create(
            title: title,
            createChangedSolution: c => Move(context.Document, node, c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static IEnumerable<XmlElementSyntax> GetTypeParamComments(
        SyntaxNode node)
    {
        return node.GetFirstToken()
            .LeadingTrivia
            .Where(t => t.IsKindOneOf(SldcTriviaKind, MldcTriviaKind))
            .Select(t => t.GetStructure())
            .FilterNonNullReference()
            .SelectMany(n => n.DescendantNodes())
            .OfType<XmlElementSyntax>()
            .Where(n => n.StartTag.Name.LocalName.Text == TypeparamName);
    }

    private static string BestIndentOfMldc(StructuredTriviaSyntax node)
    {
        return node.DescendantTokens()
            .SelectMany(token => token.LeadingTrivia)
            .Where(trivia => trivia.IsKind(DceTriviaKind))
            .OrderByDescending(trivia => trivia.Span.Length)
            .Select(trivia => trivia.ToString())
            .FirstOrDefault() ?? "";
    }

    private static IEnumerable<XmlElementSyntax> PureExteriorTrivia(
        IEnumerable<XmlElementSyntax> documentComments,
        SyntaxTrivia newDceTrivia)
    {
        SyntaxToken ToNewToken(SyntaxToken token)
            => token.WithLeadingTrivia(token.LeadingTrivia
                .Select(t => t.IsKind(DceTriviaKind) ? newDceTrivia : t)
                .ToList());

        XmlElementSyntax NewComment(XmlElementSyntax c)
        {
            var p = c.WithLeadingTrivia(c.GetLeadingTrivia()
                .Where(t => !t.IsKind(DceTriviaKind))
                .ToList());

            var changeSet = p.DescendantTokens()
                .Where(token => token.LeadingTrivia
                    .Any(t => t.IsKind(DceTriviaKind)))
                .ToDictionary(token => token, ToNewToken);

            return p.ReplaceTokens(
                changeSet.Keys, (original, token) => changeSet[original]);
        }
        return documentComments.Select(NewComment);
    }

    private static MethodDeclarationSyntax AddTypeParamComment(
        MethodDeclarationSyntax newMethod,
        IEnumerable<XmlElementSyntax> documentComments)
    {
        var oldFirstToken = newMethod.GetFirstToken();
        var oldLeadingTrivia = oldFirstToken.LeadingTrivia;
        var kind = oldLeadingTrivia.Select(t => t.Kind())
            .Where(k => k is SldcTriviaKind || k is MldcTriviaKind)
            .FirstOrDefault();
        var newLeadingTrivia
            = CommentKitMap[kind](oldFirstToken, documentComments);
        var newFirstToken = oldFirstToken.WithLeadingTrivia(newLeadingTrivia);
        return newMethod.ReplaceToken(oldFirstToken, newFirstToken);
    }

    private static SyntaxTriviaList NewMldcLeadingTrivia(
        SyntaxToken oldFirstToken,
        IEnumerable<XmlElementSyntax> documentComments)
    {
        IEnumerable<SyntaxNode>
            NewNodeList(StructuredTriviaSyntax structureNode)
        {
            var indent = BestIndentOfMldc(structureNode);
            var n = indent.Length;
            var exteriorString = n > 0 && indent[n - 1] is not ' '
                ? indent + " "
                : indent;
            var exterior = SyntaxFactory.XmlText(exteriorString);
            var pureComments = PureExteriorTrivia(
                documentComments,
                SyntaxFactory.DocumentationCommentExterior(indent));
            return pureComments.SelectMany(
                    c => new XmlNodeSyntax[] { exterior, c, XmlEol })
                .ToList();
        }
        return NewDocumentCommentLeadingTrivia(
            oldFirstToken, MldcTriviaKind, NewNodeList);
    }

    private static string Indent(int width)
    {
        return new string(' ', width);
    }

    private static SyntaxTriviaList NewSldcLeadingTrivia(
        SyntaxToken oldFirstToken,
        IEnumerable<XmlElementSyntax> documentComments)
    {
        var indentWidth = oldFirstToken.GetLocation()
            .GetLineSpan()
            .StartLinePosition
            .Character;
        var indentString = Indent(indentWidth);
        var exteriorString = indentString + SldcPrefix;
        var exterior = SyntaxFactory.XmlText(exteriorString);
        var newDceTrivia = indentString + SldcExteriorSuffix;

        IEnumerable<SyntaxNode> NewNodeList()
        {
            var pureComments = PureExteriorTrivia(
                documentComments,
                SyntaxFactory.DocumentationCommentExterior(newDceTrivia));
            return pureComments.SelectMany(
                    c => new XmlNodeSyntax[] { exterior, c, XmlEol })
                .ToList();
        }
        return NewDocumentCommentLeadingTrivia(
            oldFirstToken, SldcTriviaKind, s => NewNodeList());
    }

    private static SyntaxTriviaList NewDocumentCommentLeadingTrivia(
        SyntaxToken oldFirstToken,
        SyntaxKind kind,
        Func<StructuredTriviaSyntax, IEnumerable<SyntaxNode>> nodeList)
    {
        var oldLeadingTrivia = oldFirstToken.LeadingTrivia;
        if (oldLeadingTrivia.Where(t => t.IsKind(kind))
                .FirstValue() is not {} triviaNode
            || triviaNode.GetStructure() is not StructuredTriviaSyntax node
            || node.DescendantNodes()
                .LastOrDefault() is not {} end)
        {
            return oldLeadingTrivia;
        }
        var newNode = node.InsertNodesAfter(end, nodeList(node));
        var newTriviaNode = SyntaxFactory.Trivia(newNode);
        return oldLeadingTrivia.Replace(triviaNode, newTriviaNode);
    }

    private static SyntaxTriviaList NewLeadingTriviaFromScratch(
        SyntaxToken oldFirstToken,
        IEnumerable<XmlElementSyntax> documentComments)
    {
        var oldLeadingTrivia = oldFirstToken.LeadingTrivia;
        var indentWidth = oldFirstToken.GetLocation()
            .GetLineSpan()
            .StartLinePosition
            .Character;
        var indentString = Indent(indentWidth);
        var exteriorString = indentString + SldcPrefix;
        var exterior = SyntaxFactory.XmlText(exteriorString);
        var newDceTrivia = indentString + SldcExteriorSuffix;
        var pureComments = PureExteriorTrivia(
            documentComments,
            SyntaxFactory.DocumentationCommentExterior(newDceTrivia));
        var list = pureComments.SelectMany(
                c => new XmlNodeSyntax[] { exterior, c, XmlEol })
            .ToList();
        list[0] = XmlSldcPrefix;
        var triviaSyntax = SyntaxFactory.DocumentationCommentTrivia(
            SldcTriviaKind, [.. list]);
        return oldLeadingTrivia.Add(SyntaxFactory.Trivia(triviaSyntax));
    }

    private static ClassDeclarationSyntax
        RemoveTypeParamComment(ClassDeclarationSyntax node)
    {
        static (SyntaxTrivia Node, StructuredTriviaSyntax Trivia)?
                ToTuple(SyntaxTrivia triviaNode)
            => triviaNode.GetStructure() is not StructuredTriviaSyntax trivia
                ? null
                : (triviaNode, trivia);

        static SyntaxTriviaList ToNewLeadingTrivia(
            SyntaxTrivia triviaNode,
            StructuredTriviaSyntax trivia,
            SyntaxTriviaList leadingTrivia)
        {
            var list = trivia.DescendantNodes()
                .OfType<XmlElementSyntax>()
                .Where(n => n.StartTag.Name.LocalName.Text
                    is TypeparamName)
                .ToList();
            if (trivia.RemoveNodes(list, SyntaxRemoveOptions.KeepEndOfLine)
                is not {} newTrivia)
            {
                return leadingTrivia.Remove(triviaNode);
            }
            var newTriviaNode = SyntaxFactory.Trivia(newTrivia);
            return leadingTrivia.Replace(triviaNode, newTriviaNode);
        }

        var firstToken = node.GetFirstToken();
        var leadingTrivia = firstToken.LeadingTrivia;
        var newLeadingTrivia = leadingTrivia;
        var targetTrivias = leadingTrivia.Where(
                t => t.IsKindOneOf(SldcTriviaKind, MldcTriviaKind))
            .Select(ToTuple)
            .FilterNonNullValue()
            .ToList();
        foreach (var (triviaNode, trivia) in targetTrivias)
        {
            newLeadingTrivia = ToNewLeadingTrivia(
                triviaNode, trivia, newLeadingTrivia);
        }
        var newFirstToken = firstToken.WithLeadingTrivia(newLeadingTrivia)
            .WithAdditionalAnnotations(Formatter.Annotation);
        return node.ReplaceToken(firstToken, newFirstToken);
    }

    private static async Task<Solution> Move(
        Document document,
        ClassDeclarationSyntax node,
        CancellationToken cancellationToken)
    {
        static Func<MethodDeclarationSyntax, MethodDeclarationSyntax>
                NewToNewMethod(
            TypeParameterListSyntax baseTypeParameters,
            SyntaxList<TypeParameterConstraintClauseSyntax> baseConstraints,
            IReadOnlyList<XmlElementSyntax> documentComments)
        {
            return method =>
            {
                var typeParameterList = method.TypeParameterList;
                var newTypeParameterList = typeParameterList is null
                    ? baseTypeParameters
                    : baseTypeParameters.AddParameters(
                        [.. typeParameterList.Parameters]);

                var constraintClauses = method.ConstraintClauses;
                var newConstraintClauses
                    = baseConstraints.AddRange(constraintClauses);

                var newParameterList = method.ParameterList
                    .WithoutTrailingTrivia()
                    .WithAdditionalAnnotations(Formatter.Annotation);

                var m = method.WithTypeParameterList(newTypeParameterList)
                    .WithParameterList(newParameterList)
                    .WithConstraintClauses(newConstraintClauses);
                var newMethod = documentComments.Count > 0
                    ? AddTypeParamComment(m, documentComments)
                    : m;
                return newMethod;
            };
        }

        var solution = document.Project
            .Solution;
        if (await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false) is not {} root
            || await document.GetSemanticModelAsync(cancellationToken)
                .ConfigureAwait(false) is not {} model
            || model.GetDeclaredSymbol(node, cancellationToken)
                is not {} symbol)
        {
            return solution;
        }

        async Task<SyntaxToken> GetNewUniqueId(SyntaxToken original)
        {
            var baseText = original.ValueText;
            var id = original;
            var count = 0;
            for (;;)
            {
                var all = await SymbolFinder.FindDeclarationsAsync(
                        document.Project,
                        id.ValueText,
                        false,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (all.All(s => Symbols.AreEqual(s, symbol)))
                {
                    return id.WithAdditionalAnnotations(Formatter.Annotation);
                }
                ++count;
                id = SyntaxFactory.Identifier(baseText + "_" + count);
            }
        }

        var newIdentifier = await GetNewUniqueId(node.Identifier)
            .ConfigureAwait(false);
        var allReferences = await SymbolFinder.FindReferencesAsync(
                symbol, solution, cancellationToken)
            .ConfigureAwait(false);
        var documentGroups = allReferences.SelectMany(r => r.Locations)
            .GroupBy(w => w.Document);
        var changeMap = new Dictionary<SyntaxNode, SyntaxNode>();
        root = root.TrackNodes(node);
        UpdateMainDocument(
            changeMap,
            document,
            root,
            newIdentifier,
            documentGroups);
        root = root.ReplaceNodes(
            changeMap.Keys, (original, n) => changeMap[original]);

        if (root.GetCurrentNode(node) is not {} currentNode)
        {
            return solution;
        }
        var childNodes = currentNode.ChildNodes();
        var typeParameters = childNodes.OfType<TypeParameterListSyntax>()
            .First()
            .WithoutTrivia();
        var constraintClauseList
                = childNodes.OfType<TypeParameterConstraintClauseSyntax>()
            .Select(n => n.WithAdditionalAnnotations(Formatter.Annotation));
        var constraintClauses
            = new SyntaxList<TypeParameterConstraintClauseSyntax>(
                constraintClauseList);
        var documentComments = GetTypeParamComments(currentNode).ToList();
        var toNewMethod = NewToNewMethod(
            typeParameters, constraintClauses, documentComments);
        var methodList = childNodes.Where(
                n => n.IsKind(SyntaxKind.MethodDeclaration))
            .OfType<MethodDeclarationSyntax>()
            .ToList();
        foreach (var m in methodList)
        {
            changeMap[m] = toNewMethod(m);
        }

        var empty = Array.Empty<TypeParameterConstraintClauseSyntax>();
        var emptyClause
            = new SyntaxList<TypeParameterConstraintClauseSyntax>(empty);
        var fixedNode = currentNode.ReplaceNodes(
                changeMap.Keys, (original, n) => changeMap[original])
            .WithTypeParameterList(null)
            .WithIdentifier(newIdentifier)
            .WithConstraintClauses(emptyClause);
        var newNode = documentComments.Count > 0
            ? RemoveTypeParamComment(fixedNode)
            : fixedNode;

        var workspace = solution.Workspace;
        var formattedNode = Formatter.Format(
           newNode,
           Formatter.Annotation,
           workspace,
           workspace.Options,
           cancellationToken);
        var newRoot = root.ReplaceNode(currentNode, formattedNode);
        solution = solution.WithDocumentSyntaxRoot(document.Id, newRoot);
        return await UpdateReferencingDocumentsAsync(
                document,
                documentGroups,
                newIdentifier,
                solution,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static void UpdateMainDocument(
        Dictionary<SyntaxNode, SyntaxNode> changeMap,
        Document document,
        SyntaxNode root,
        SyntaxToken newNameToken,
        IEnumerable<IGrouping<Document, ReferenceLocation>> documentGroups)
    {
        var newIdentifier = SyntaxFactory.IdentifierName(newNameToken);
        if (documentGroups.FirstOrDefault(g => g.Key.Equals(document))
            is not {} mainDocumentGroup)
        {
            return;
        }
        var nodes = mainDocumentGroup.Select(
                w => root.FindNode(w.Location.SourceSpan))
            .Where(n => n.IsKind(SyntaxKind.GenericName))
            .ToList();
        foreach (var n in nodes)
        {
            changeMap[n] = newIdentifier;
        }
    }

    private static async Task<Solution> UpdateReferencingDocumentsAsync(
        Document document,
        IEnumerable<IGrouping<Document, ReferenceLocation>> documentGroups,
        SyntaxToken newNameToken,
        Solution solution,
        CancellationToken cancellationToken)
    {
        var newIdentifier = SyntaxFactory.IdentifierName(newNameToken);
        var newSolution = solution;
        var groups = documentGroups.Where(g => !g.Key.Equals(document))
            .ToList();
        foreach (var g in groups)
        {
            var id = g.Key.Id;
            if (newSolution.GetDocument(id) is not {} d
                || await d.GetSyntaxRootAsync(cancellationToken)
                    .ConfigureAwait(false) is not {} root)
            {
                continue;
            }
            var changeMap = g.Select(w => root.FindNode(w.Location.SourceSpan))
                .Where(n => n.IsKind(SyntaxKind.GenericName))
                .ToDictionary(n => n, n => newIdentifier);
            var newRoot = root.ReplaceNodes(
                changeMap.Keys,
                (original, node) => changeMap[original]);
            newSolution = newSolution.WithDocumentSyntaxRoot(d.Id, newRoot);
        }
        return newSolution;
    }
}
