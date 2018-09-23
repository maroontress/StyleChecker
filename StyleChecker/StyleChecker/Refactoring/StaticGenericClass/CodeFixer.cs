namespace StyleChecker.Refactoring.StaticGenericClass
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
    using Microsoft.CodeAnalysis.Formatting;
    using R = Resources;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
        private const string TypeparamName = "typeparam";

        private const string SldcPrefix = "/// ";

        private const string SldcExteriorSuffix = "///";

        private static readonly SyntaxKind MldcTriviaKind
            = SyntaxKind.MultiLineDocumentationCommentTrivia;

        private static readonly SyntaxKind SldcTriviaKind
            = SyntaxKind.SingleLineDocumentationCommentTrivia;

        private static readonly SyntaxKind DceTriviaKind
            = SyntaxKind.DocumentationCommentExteriorTrivia;

        private static readonly XmlTextSyntax XmlEol
            = SyntaxFactory.XmlText(
                SyntaxFactory.XmlTextNewLine("\r\n", false));

        private static readonly XmlTextSyntax XmlSldcPrefix
            = SyntaxFactory.XmlText(SldcPrefix);

        private static readonly
            ImmutableDictionary<SyntaxKind, CommentKit> CommentKitMap;

        static CodeFixer()
        {
            var map = new Dictionary<SyntaxKind, CommentKit>
            {
                [SldcTriviaKind] = NewSldcLeadingTrivia,
                [MldcTriviaKind] = NewMldcLeadingTrivia,
                [default] = NewLeadingTriviaFromScratch
            };
            CommentKitMap = map.ToImmutableDictionary();
        }

        private delegate SyntaxTriviaList CommentKit(
            SyntaxToken token,
            ImmutableList<XmlElementSyntax> documentComments);


        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
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
                    createChangedDocument:
                        c => Move(context.Document, node, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private static ImmutableList<XmlElementSyntax>
            GetTypeParamComments(SyntaxNode node)
        {
            return node.GetFirstToken()
                .LeadingTrivia
                .Where(t => t.IsKindOneOf(SldcTriviaKind, MldcTriviaKind))
                .Select(t => t.GetStructure())
                .SelectMany(n => n.DescendantNodes())
                .Where(n => n.IsKind(SyntaxKind.XmlElement))
                .Select(n => n as XmlElementSyntax)
                .Where(n => n.StartTag.Name.LocalName.Text
                    .Equals(TypeparamName))
                .ToImmutableList();
        }

        private static string BestIndentOfMldc(
            StructuredTriviaSyntax structureNode)
        {
            var first = structureNode.DescendantTokens()
                .SelectMany(token => token.LeadingTrivia)
                .Where(trivia => trivia.IsKind(DceTriviaKind))
                .OrderByDescending(trivia => trivia.Span.Length)
                .FirstOrDefault();
            return (first == default) ? "" : first.ToString();
        }

        private static ImmutableList<XmlElementSyntax> PureExteriorTrivia(
            ImmutableList<XmlElementSyntax> documentComments,
            SyntaxTrivia newDceTrivia)
        {
            XmlElementSyntax NewComment(XmlElementSyntax c)
            {
                var p = c.WithLeadingTrivia(c.GetLeadingTrivia()
                    .Where(t => !t.IsKind(DceTriviaKind))
                    .ToList());

                var changeSet = new Dictionary<SyntaxToken, SyntaxToken>();
                var allTokens = p.DescendantTokens()
                    .Where(token => token.LeadingTrivia
                        .Where(t => t.Kind() == DceTriviaKind)
                        .Any());
                foreach (var oldToken in allTokens)
                {
                    changeSet[oldToken] = oldToken
                        .WithLeadingTrivia(oldToken.LeadingTrivia
                            .Select(t => t.IsKind(DceTriviaKind)
                                ? newDceTrivia : t)
                            .ToList());
                }
                return p.ReplaceTokens(
                    changeSet.Keys, (original, token) => changeSet[original]);
            }
            return documentComments.Select(c => NewComment(c))
                .ToImmutableList();
        }

        private static MethodDeclarationSyntax AddTypeParamComment(
            MethodDeclarationSyntax newMethod,
            ImmutableList<XmlElementSyntax> documentComments)
        {
            var oldFirstToken = newMethod.GetFirstToken();
            var oldLeadingTrivia = oldFirstToken.LeadingTrivia;
            var kind = oldLeadingTrivia
                .Where(t => t.IsKindOneOf(SldcTriviaKind, MldcTriviaKind))
                .Select(t => t.Kind())
                .FirstOrDefault();

            var newLeadingTrivia
                = CommentKitMap[kind](oldFirstToken, documentComments);
            var newFirstToken = oldFirstToken
                .WithLeadingTrivia(newLeadingTrivia);
            return newMethod.ReplaceToken(oldFirstToken, newFirstToken);
        }

        private static SyntaxTriviaList NewMldcLeadingTrivia(
            SyntaxToken oldFirstToken,
            ImmutableList<XmlElementSyntax> documentComments)
        {
            IEnumerable<SyntaxNode>
                NewNodeList(StructuredTriviaSyntax structureNode)
            {
                var indent = BestIndentOfMldc(structureNode);
                var n = indent.Length;
                var exteriorString = (n > 0 && indent[n - 1] != ' ')
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
                oldFirstToken, documentComments, MldcTriviaKind, NewNodeList);
        }

        private static string Indent(int width)
        {
            var indentChars = new char[width];
            for (var k = 0; k < width; ++k)
            {
                indentChars[k] = ' ';
            }
            return new string(indentChars);
        }

        private static SyntaxTriviaList NewSldcLeadingTrivia(
            SyntaxToken oldFirstToken,
            ImmutableList<XmlElementSyntax> documentComments)
        {
            var indentWidth = oldFirstToken.GetLocation()
                .GetLineSpan()
                .StartLinePosition
                .Character;
            var indentString = Indent(indentWidth);
            var exteriorString = indentString + SldcPrefix;
            var exterior = SyntaxFactory.XmlText(exteriorString);
            var newDceTrivia = indentString + SldcExteriorSuffix;

            IEnumerable<SyntaxNode>
                NewNodeList(StructuredTriviaSyntax ignored)
            {
                var pureComments = PureExteriorTrivia(
                    documentComments,
                    SyntaxFactory.DocumentationCommentExterior(newDceTrivia));
                return pureComments.SelectMany(
                        c => new XmlNodeSyntax[] { exterior, c, XmlEol })
                    .ToList();
            }
            return NewDocumentCommentLeadingTrivia(
                oldFirstToken, documentComments, SldcTriviaKind, NewNodeList);
        }

        private static SyntaxTriviaList NewDocumentCommentLeadingTrivia(
            SyntaxToken oldFirstToken,
            ImmutableList<XmlElementSyntax> documentComments,
            SyntaxKind kind,
            Func<StructuredTriviaSyntax, IEnumerable<SyntaxNode>> nodeList)
        {
            var oldLeadingTrivia = oldFirstToken.LeadingTrivia;
            var triviaNode = oldLeadingTrivia
                .Where(t => t.IsKind(kind))
                .First();
            var structureNode = triviaNode.GetStructure()
                as StructuredTriviaSyntax;
            var end = structureNode
                .DescendantNodes()
                .Last();

            var newStructureNode = structureNode
                .InsertNodesAfter(end, nodeList(structureNode));
            var newTriviaNode = SyntaxFactory.Trivia(newStructureNode);
            return oldLeadingTrivia.Replace(triviaNode, newTriviaNode);
        }

        private static SyntaxTriviaList NewLeadingTriviaFromScratch(
            SyntaxToken oldFirstToken,
            ImmutableList<XmlElementSyntax> documentComments)
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
                SldcTriviaKind, new SyntaxList<XmlNodeSyntax>(list));
            return oldLeadingTrivia.Add(SyntaxFactory.Trivia(triviaSyntax));
        }

        private static ClassDeclarationSyntax
            RemoveTypeParamComment(ClassDeclarationSyntax newNode)
        {
            var firstToken = newNode.GetFirstToken();
            var leadingTrivia = firstToken.LeadingTrivia;
            var newLeadingTrivia = leadingTrivia;
            var targetNodes = leadingTrivia
                .Where(t => t.IsKindOneOf(SldcTriviaKind, MldcTriviaKind));
            foreach (var triviaNode in targetNodes)
            {
                var structureNode = triviaNode.GetStructure()
                        as StructuredTriviaSyntax;
                var list = structureNode.DescendantNodes()
                    .Where(n => n.IsKind(SyntaxKind.XmlElement))
                    .Select(n => n as XmlElementSyntax)
                    .Where(n => n.StartTag.Name.LocalName.Text
                        .Equals(TypeparamName))
                    .ToImmutableList();
                var newStructureNode = structureNode
                    .RemoveNodes(list, SyntaxRemoveOptions.KeepEndOfLine);
                var newTriviaNode
                    = SyntaxFactory.Trivia(newStructureNode);

                newLeadingTrivia = newLeadingTrivia.Replace(
                        triviaNode, newTriviaNode);
            }
            var newFirstToken = firstToken
                .WithLeadingTrivia(newLeadingTrivia)
                .WithAdditionalAnnotations(Formatter.Annotation);
            return newNode.ReplaceToken(firstToken, newFirstToken);
        }

        private static async Task<Document> Move(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            var childNodes = node.ChildNodes();
            var typeParameterList = childNodes
                .First(n => n.IsKind(SyntaxKind.TypeParameterList))
                .WithoutTrivia() as TypeParameterListSyntax;
            var constraintClauseList = childNodes
                .Where(
                    n => n.IsKind(SyntaxKind.TypeParameterConstraintClause))
                .Select(n => n.WithAdditionalAnnotations(Formatter.Annotation)
                    as TypeParameterConstraintClauseSyntax)
                .ToImmutableList();

            var constraintClauses
                = new SyntaxList<TypeParameterConstraintClauseSyntax>(
                    constraintClauseList);

            var changeMap = new Dictionary<MethodDeclarationSyntax,
                MethodDeclarationSyntax>();

            var documentComments = GetTypeParamComments(node);

            var methodList = childNodes
                .Where(n => n.IsKind(SyntaxKind.MethodDeclaration))
                .ToImmutableList();
            foreach (var method in methodList)
            {
                var oldMethod = method as MethodDeclarationSyntax;

                var oldTypeParameterList = oldMethod.TypeParameterList;
                var newTypeParameterList = oldTypeParameterList != null
                    ? typeParameterList.AddParameters(
                        oldTypeParameterList.Parameters.ToArray())
                    : typeParameterList;

                var oldConstraintClauses = oldMethod.ConstraintClauses;
                var newConstraintClauses = oldConstraintClauses != null
                    ? constraintClauses.AddRange(oldConstraintClauses)
                    : constraintClauses;

                var newParameterList = oldMethod.ParameterList
                    .WithoutTrailingTrivia()
                    .WithAdditionalAnnotations(Formatter.Annotation);

                var newMethod = oldMethod
                    .WithTypeParameterList(newTypeParameterList)
                    .WithParameterList(newParameterList)
                    .WithConstraintClauses(newConstraintClauses);
                if (documentComments.Count > 0)
                {
                    newMethod = AddTypeParamComment(
                        newMethod, documentComments);
                }
                changeMap[oldMethod] = newMethod;
            }

            var empty = Array.Empty<TypeParameterConstraintClauseSyntax>();
            var emptyClause
                = new SyntaxList<TypeParameterConstraintClauseSyntax>(empty);
            var newNode = node
                .ReplaceNodes(
                    changeMap.Keys,
                    (original, n) => changeMap[original])
                as ClassDeclarationSyntax;
            var newIdentifier = newNode.Identifier
                .WithAdditionalAnnotations(Formatter.Annotation);
            newNode = newNode.WithTypeParameterList(null)
                .WithIdentifier(newIdentifier)
                .WithConstraintClauses(emptyClause);
            if (documentComments.Count > 0)
            {
                newNode = RemoveTypeParamComment(newNode);
            }
            var formattedNode = Formatter.Format(
               newNode,
               Formatter.Annotation,
               document.Project.Solution.Workspace,
               document.Project.Solution.Workspace.Options);
            var newRoot = root.ReplaceNode(node, formattedNode);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
