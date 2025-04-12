namespace CodeFixes.Refactoring.NullCheckAfterDeclaration;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analyzers;
using Analyzers.Refactoring.NullCheckAfterDeclaration;
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

    private static int IsExprPrecedence { get; }
        = OperatorPrecedences.Of(SyntaxKind.IsPatternExpression);

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
                c => Refactor(document, root, node, nextNode, c),
            equivalenceKey: title);
        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> Refactor(
        Document document,
        SyntaxNode root,
        LocalDeclarationStatementSyntax declNode,
        IfStatementSyntax ifNode,
        CancellationToken cancellationToken)
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

        static bool AreSameType(
            Symbolizer s, ExpressionSyntax left, ExpressionSyntax right)
        {
            var all = new[] { left, right }
                .Select(s.ToTypeInfo)
                .Select(i => i.Type)
                .ToList();
            return SymbolEqualityComparer.Default.Equals(all[0], all[1]);
        }

        static TypeSyntax? ToTypePattern(TypeSyntax declarationType)
            => declarationType.IsVar
                ? null
                : declarationType.WithoutTrivia();

        static IsPatternSpec NewConditionalExpression(
            Symbolizer s,
            TypeSyntax declarationType,
            ExpressionSyntax expr,
            ConditionalExpressionSyntax condExpr)
        {
            var returnExpr = Expressions.ParenthesizeIfNeeded(expr);
            var newExpr = AreSameType(s, condExpr.WhenTrue, condExpr.WhenFalse)
                ? (ExpressionSyntax)returnExpr
                : SyntaxFactory.CastExpression(declarationType, returnExpr);
            return new(newExpr, ToTypePattern(declarationType));
        }

        static SyntaxTriviaList ToOpenParenTrivias(
                ParenthesizedExpressionSyntax n)
            => n.OpenParenToken.TrailingTrivia;

        static SyntaxTriviaList ToCloseParenTrivias(
                ParenthesizedExpressionSyntax n)
            => n.CloseParenToken.TrailingTrivia;

        static (SyntaxTriviaList Left, SyntaxTriviaList Right)
            PeeledTrivias(ExpressionSyntax expr)
        {
            if (expr is not ParenthesizedExpressionSyntax parenExpr)
            {
                var empty = SyntaxTriviaList.Empty;
                return (empty, empty);
            }
            var left = ToOpenParenTrivias(parenExpr);
            var right = ToCloseParenTrivias(parenExpr);
            var next = parenExpr.Expression;
            while (next is ParenthesizedExpressionSyntax parentExpr)
            {
                left = left.AddRange(ToOpenParenTrivias(parenExpr));
                right = right.AddRange(ToCloseParenTrivias(parenExpr));
                next = parentExpr.Expression;
            }
            return (left, right);
        }

        static ExpressionSyntax UnpeelTrivia(
            ExpressionSyntax expr, ExpressionSyntax coreExpr)
        {
            var (left, right) = PeeledTrivias(expr);
            var newTrailingTrivia = coreExpr.GetTrailingTrivia()
                .AddRange(left)
                .AddRange(right);
            return coreExpr.WithTrailingTrivia(newTrailingTrivia);
        }

        static IsPatternSpec NewAsOrOtherExpression(
            Symbolizer s,
            TypeSyntax declarationType,
            ExpressionSyntax expr,
            ExpressionSyntax coreExpr)
        {
            return (coreExpr is BinaryExpressionSyntax binaryExpr
                && binaryExpr.IsKind(SyntaxKind.AsExpression)
                && binaryExpr.Right is TypeSyntax rightType
                && (declarationType.IsVar
                    || AreSameType(s, declarationType, rightType)))
                /*
                    It does not matter if binaryExpr.Left is a conditional
                    expression.
                */
                ? new(
                    UnpeelTrivia(expr, binaryExpr.Left),
                    rightType.WithLeadingTrivia(
                        binaryExpr.OperatorToken.TrailingTrivia))
                : new(expr, ToTypePattern(declarationType));
        }

        static IsPatternSpec NewIsPatternSpec(
            Symbolizer s,
            TypeSyntax declarationType,
            ExpressionSyntax expr)
        {
            var coreExpr = Expressions.Peel(expr);
            if (coreExpr is ConditionalExpressionSyntax condExpr
                && !declarationType.IsVar)
            {
                return NewConditionalExpression(
                    s, declarationType, expr, condExpr);
            }
            var (newCoreExpr, typePattern)
                = NewAsOrOtherExpression(s, declarationType, expr, coreExpr);
            var precedence = OperatorPrecedences.Of(newCoreExpr.Kind());
            var newExpr = (precedence >= IsExprPrecedence)
                ? SyntaxFactory.ParenthesizedExpression(newCoreExpr)
                : newCoreExpr;
            return new(newExpr, typePattern);
        }

        if (await document.GetSemanticModelAsync(cancellationToken)
            is not {} model)
        {
            return document;
        }
        var symbolizer = new Symbolizer(model, cancellationToken);
        var declaration = declNode.Declaration;
        if (declaration.Variables is not { Count: > 0 } variables
            || NullChecks.ClassifyNullCheck(ifNode) is not {} isNullCheck)
        {
            return document;
        }
        var lastVariable = variables.Last();
        if (lastVariable.Initializer is not {} initializer)
        {
            return document;
        }
        var patternTrivias = ifNode.Condition
            .DescendantTrivia()
            .ToSyntaxTriviaList();
        var beforeValueTrivia = initializer.EqualsToken
            .TrailingTrivia;
        var expr = initializer.Value;
        var declarationType = declaration.Type;
        var newSpec = NewIsPatternSpec(symbolizer, declarationType, expr);
        var newExpr = newSpec.Expression
            .WithLeadingTrivia(beforeValueTrivia);
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
            return document;
        }
        var firstRoot = (variables.Count is 1)
            ? trackedRoot.RemoveNode(trackedDeclNode, RemoveNodeOptions)
            : trackedRoot.ReplaceNode(
                trackedDeclNode, RemoveLastDeclarator(declNode));
        if (firstRoot is null
            || firstRoot.GetCurrentNode(ifNode) is not {} trackedIfNode)
        {
            return document;
        }
        var recursivePattern = newSpec.TypePattern is not {} typePattern
            ? NewRecursivePattern()
            : NewTypePattern(typePattern);
        var declarationPattern = recursivePattern.WithDesignation(
            SyntaxFactory.SingleVariableDesignation(newIdentifier));
        var pattern = isNullCheck
            ? NewNotPattern(declarationPattern)
            : declarationPattern;
        var isPattern = SyntaxFactory.IsPatternExpression(newExpr, pattern);
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
