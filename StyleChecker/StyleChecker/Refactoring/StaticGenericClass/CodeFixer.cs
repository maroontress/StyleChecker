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

        private async Task<Document> Move(
            Document document,
            SyntaxNode node,
            CancellationToken cancellationToken)
        {
            // T[] Singleton<T>(T instance) => new T[] { instance };
            // var crlf = SyntaxFactory.CarriageReturnLineFeed;

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
                changeMap[oldMethod] = newMethod;
            }

            var empty = Array.Empty<TypeParameterConstraintClauseSyntax>();
            var emptyClause
                = new SyntaxList<TypeParameterConstraintClauseSyntax>(empty);
            var newNode = node as ClassDeclarationSyntax;
            var newMembers = newNode.Members;
            foreach (var k in changeMap.Keys)
            {
                newMembers = newMembers.Replace(k, changeMap[k]);
            }
            var newIdentifier = newNode.Identifier
                .WithAdditionalAnnotations(Formatter.Annotation);
            newNode = newNode.WithTypeParameterList(null)
                .WithIdentifier(newIdentifier)
                .WithConstraintClauses(emptyClause)
                .WithMembers(newMembers);

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

// public static ClassDeclarationSyntax ClassDeclaration(
//   SyntaxList<AttributeListSyntax> attributeLists,
//   SyntaxTokenList modifiers,
//   SyntaxToken keyword,
//   SyntaxToken identifier,
//   TypeParameterListSyntax typeParameterList,
//   BaseListSyntax baseList,
//   SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses,
//   SyntaxToken openBraceToken,
//   SyntaxList<MemberDeclarationSyntax> members,
//   SyntaxToken closeBraceToken,
//   SyntaxToken semicolonToken);

// public static MethodDeclarationSyntax MethodDeclaration(
//   SyntaxList<AttributeListSyntax> attributeLists,
//   SyntaxTokenList modifiers,
//   TypeSyntax returnType,
//   ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier,
//   SyntaxToken identifier,
//   TypeParameterListSyntax typeParameterList,
//   ParameterListSyntax parameterList,
//   SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses,
//   BlockSyntax body,
//   ArrowExpressionClauseSyntax expressionBody,
//   SyntaxToken semicolonToken);
