namespace StyleChecker.Refactoring.NullCheckAfterDeclaration;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using R = Resources;

/// <summary>
/// NullCheckAfterDeclaration CodeFix provider.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
[Shared]
public sealed class CodeFixer : AbstractCodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds
        => [Analyzer.DiagnosticId];

    private static SyntaxRemoveOptions RemoveNodeOptions { get; }
        = SyntaxRemoveOptions.KeepExteriorTrivia
            | SyntaxRemoveOptions.KeepDirectives
            | SyntaxRemoveOptions.KeepEndOfLine;

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        var title = localize(nameof(R.FixTitle))
            .ToString(CompilerCulture);

        var document = context.Document;
        if (await document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false) is not {} root)
        {
            return;
        }
        var diagnostic = context.Diagnostics[0];
        var span = diagnostic.Location.SourceSpan;
        if (root.FindNodeOfType<VariableDeclaratorSyntax>(span)
            is not {} declaratorNode
            || declaratorNode.Parent is not VariableDeclarationSyntax declNode
            || declNode.Parent is not LocalDeclarationStatementSyntax node
            || node.NextNode() is not IfStatementSyntax nextNode)
        {
            return;
        }
        var action = CodeAction.Create(
            title: title,
            createChangedDocument:
                c => Task.FromResult(
                    Refactor(document, root, node, nextNode) ?? document),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static SyntaxNode AddUsing(LocalDeclarationStatementSyntax node)
        => node.WithoutLeadingTrivia()
            .WithUsingKeyword(SyntaxFactory.Token(SyntaxKind.UsingKeyword))
            .WithLeadingTrivia(node.GetLeadingTrivia())
            .WithAdditionalAnnotations(Formatter.Annotation);

    private static Document? Refactor(
        Document document,
        SyntaxNode root,
        LocalDeclarationStatementSyntax declNode,
        IfStatementSyntax ifNode)
    {
        static PatternSyntax NewNotPattern(PatternSyntax s)
            => SyntaxFactory.UnaryPattern(
                SyntaxFactory.Token(SyntaxKind.NotKeyword), s);

        static RecursivePatternSyntax NewTypePattern(TypeSyntax type)
            => SyntaxFactory.RecursivePattern()
                .WithType(type is NullableTypeSyntax nullable
                    ? nullable.ElementType
                    : type);

        static RecursivePatternSyntax NewRecursivePattern()
            => SyntaxFactory.RecursivePattern()
                .WithPropertyPatternClause(
                    SyntaxFactory.PropertyPatternClause());

        var declaration = declNode.Declaration;
        if (declaration.Variables is not { Count: > 0 } variables
            || NullChecks.ClassifyNullCheck(ifNode) is not {} isNullCheck)
        {
            return null;
        }
        var lastVariable = variables.Last();
        if (lastVariable.Initializer is not {} initializer)
        {
            return null;
        }
        var patternTrivias = ifNode.Condition
            .DescendantTrivia()
            .ToSyntaxTriviaList();
        var beforeValueTrivia = initializer.EqualsToken
            .TrailingTrivia;
        var expression = initializer.Value
            .WithLeadingTrivia(beforeValueTrivia);
        var declarationType = declaration.Type;
        var beforeIdTrivia = (variables.Count is 1)
            ? declarationType.GetTrailingTrivia()
            : declaration.ChildTokens().Last()
                .TrailingTrivia;
        var identifier = lastVariable.Identifier;
        var afterIdTrivia = identifier.TrailingTrivia;
        var newIdentifier = identifier.WithLeadingTrivia(beforeIdTrivia)
            .WithTrailingTrivia(afterIdTrivia.Concat(patternTrivias));

        var trackedRoot = root.TrackNodes(declNode, ifNode);
        if (trackedRoot.GetCurrentNode(declNode)
            is not {} trackedDeclNode)
        {
            return null;
        }
        var firstRoot = (variables.Count is 1)
            ? trackedRoot.RemoveNode(trackedDeclNode, RemoveNodeOptions)
            : trackedRoot.ReplaceNode(
                trackedDeclNode, RemoveLastDeclarator(declNode));
        if (firstRoot is null
            || firstRoot.GetCurrentNode(ifNode) is not {} trackedIfNode)
        {
            return null;
        }
        var recursivePattern = declarationType.IsVar
            ? NewRecursivePattern()
            : NewTypePattern(declarationType.WithoutLeadingTrivia());
        var declarationPattern = recursivePattern.WithDesignation(
            SyntaxFactory.SingleVariableDesignation(newIdentifier));
        var pattern = isNullCheck
            ? NewNotPattern(declarationPattern)
            : declarationPattern;
        var isPattern = SyntaxFactory.IsPatternExpression(expression, pattern);
        var newIfNode = ifNode.WithCondition(isPattern)
            .WithTriviaFrom(trackedIfNode);
        var secondRoot = firstRoot.ReplaceNode(trackedIfNode, newIfNode)
            .WithAdditionalAnnotations(Formatter.Annotation);
        return document.WithSyntaxRoot(secondRoot);
    }

    private static LocalDeclarationStatementSyntax RemoveLastDeclarator(
        LocalDeclarationStatementSyntax node)
    {
        var declaration = node.Declaration;
        var variables = declaration.Variables;
        var newVariables = variables.RemoveAt(variables.Count - 1);
        var newDeclaration = declaration.WithVariables(newVariables);
        return node.WithDeclaration(newDeclaration);
    }
}
