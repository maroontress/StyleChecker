#pragma warning disable RS1008

namespace StyleChecker.Refactoring.NotOneShotInitialization;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Maroontress.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using StyleChecker.Refactoring;
using Enumerables = Maroontress.Util.Enumerables;
using R = Resources;

/// <summary>
/// NotOneShotInitialization analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "NotOneShotInitialization";

    private const string Category = Categories.Refactoring;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    private static readonly ImmutableHashSet<ILocalSymbol>
        NoLocalSymbols = [];

    private static readonly IEnumerable<(ILocalSymbol, SyntaxToken)>
        EmptyLocalSymbolTokens = [];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(AnalyzeModel);
    }

    private static DiagnosticDescriptor NewRule()
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        return new DiagnosticDescriptor(
            DiagnosticId,
            localize(nameof(R.Title)),
            localize(nameof(R.MessageFormat)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        AnalyzeModel(context, context.SemanticModel);
    }

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context, SemanticModel model)
    {
        var root = context.GetCompilationUnitRoot();

        static (List<SyntaxNode> Children, int Index)?
            GetChildIndex(SyntaxNode child)
        {
            if (child.Parent is not {} parent)
            {
                return null;
            }
            var children = parent.ChildNodes()
                .ToList();
            if (children.Count is 0)
            {
                return null;
            }
            var index = children.FindIndex(c => ReferenceEquals(c, child));
            return (index is -1)
                ? null
                : (children, index);
        }

        static SyntaxNode? GetPreviousNode(SyntaxNode child)
        {
            if (GetChildIndex(child) is not {} childIndex)
            {
                return null;
            }
            var (children, i) = childIndex;
            return (i is 0)
                ? null
                : children[i - 1];
        }

        static SyntaxNode? GetNextNode(SyntaxNode child)
        {
            if (GetChildIndex(child) is not {} childIndex)
            {
                return null;
            }
            var (children, i) = childIndex;
            return (i == children.Count - 1)
                ? null
                : children[i + 1];
        }

        static IdentifierNameSyntax? ToIdentifier(ExpressionSyntax s)
        {
            return (s is IdentifierNameSyntax i) ? i : null;
        }

        static IdentifierNameSyntax? GetTheLikeOfAssignment(SyntaxNode n)
        {
            if (n is not ExpressionStatementSyntax expr)
            {
                return null;
            }
            var i = expr.Expression switch
            {
                AssignmentExpressionSyntax a
                    => a.Left,
                PrefixUnaryExpressionSyntax u when u.IsKindOneOf(
                        SyntaxKind.PreIncrementExpression,
                        SyntaxKind.PreDecrementExpression)
                    => u.Operand,
                PostfixUnaryExpressionSyntax u when u.IsKindOneOf(
                        SyntaxKind.PostIncrementExpression,
                        SyntaxKind.PostDecrementExpression)
                    => u.Operand,
                _ => null,
            };
            return (i is null) ? null : ToIdentifier(i);
        }

        static bool ContainsDefaultSection(SwitchSectionSyntax s)
        {
            var labels = s.Labels;
            return labels.OfType<DefaultSwitchLabelSyntax>()
                .Any();
        }

        static bool ContainsBreakOnly(SwitchSectionSyntax s)
        {
            var statements = s.Statements;
            return statements.Count is 1
                && statements[0] is BreakStatementSyntax;
        }

        static bool IsNextAlso<T>(T node)
            where T : SyntaxNode
        {
            var next = GetNextNode(node);
            return next is T;
        }

        static List<T> NewList<T>(T t)
            where T : SyntaxNode
        {
            var list = new List<T>()
            {
                t,
            };
            var n = GetPreviousNode(t);
            while (n is T e)
            {
                list.Add(e);
                n = GetPreviousNode(n);
            }
            return list;
        }

        static IEnumerable<List<T>> SplitRow<T>(IEnumerable<T> g)
            where T : SyntaxNode
        {
            return g.Where(s => !IsNextAlso(s))
                    .Select(s => NewList(s));
        }

        ILocalSymbol? ToAssignmentLocalSymbol(SyntaxNode n)
        {
            return GetTheLikeOfAssignment(n) is {} i
                    && model.GetOperation(i) is ILocalReferenceOperation o
                ? o.Local
                : null;
        }

        IImmutableSet<ILocalSymbol> ToIfAssignSet(IfStatementSyntax node)
        {
            var thenNode = node.Statement;
            var childNodes = Enumerables.Of<SyntaxNode>(thenNode);
            while (thenNode is BlockSyntax block)
            {
                if (block.Statements
                    .FirstOrDefault() is not {} first)
                {
                    return NoLocalSymbols;
                }
                childNodes = thenNode.ChildNodes();
                thenNode = first;
            }
            var childSymbols = childNodes.Select(ToAssignmentLocalSymbol)
                .ToList();
            return childSymbols.Any(n => n is null)
                ? NoLocalSymbols
                : childSymbols.FilterNonNullReference()
                    .ToRigidSet();
        }

        IImmutableSet<ILocalSymbol> ToAssignBreakSet(SwitchSectionSyntax s)
        {
            var statements = s.Statements;
            var n = statements.Count;
            if (n < 2
                || statements[n - 1] is not BreakStatementSyntax)
            {
                return NoLocalSymbols;
            }
            var symbols = statements.Take(n - 1)
                .Select(ToAssignmentLocalSymbol)
                .ToList();
            return symbols.Any(a => a is null)
                ? NoLocalSymbols
                : symbols.FilterNonNullReference()
                    .ToRigidSet();
        }

        IImmutableSet<ILocalSymbol> ToSwitchAssignSet(
            SwitchStatementSyntax node)
        {
            var allSets = node.Sections
                .Where(s => !ContainsDefaultSection(s))
                .Select(ToAssignBreakSet)
                .ToList();
            if (allSets.Count is 0
                || allSets.Any(s => !s.Any()))
            {
                return NoLocalSymbols;
            }
            var firstSet = allSets[0];
            if (!allSets.Skip(1)
                .All(s => s.SetEquals(firstSet)))
            {
                return NoLocalSymbols;
            }
            var defaultSection = node.Sections
                .FirstOrDefault(ContainsDefaultSection);
            return defaultSection is null
                    || ContainsBreakOnly(defaultSection)
                ? firstSet
                : NoLocalSymbols;
        }

        IImmutableSet<ILocalSymbol> ToAssignSet(SyntaxNode node)
            => node switch
            {
                IfStatementSyntax n => ToIfAssignSet(n),
                SwitchStatementSyntax n => ToSwitchAssignSet(n),
                _ => NoLocalSymbols,
            };

        ILocalSymbol? ToDeclarationLocalSymbol(VariableDeclaratorSyntax node)
            => model.GetOperation(node) is IVariableDeclaratorOperation d
                ? d.Symbol
                : null;

        IEnumerable<(ILocalSymbol Symbol, SyntaxToken Token)>
                ToLocalSymbolToken(VariableDeclaratorSyntax node)
            => ToDeclarationLocalSymbol(node) is {} symbol
                ? ImmutableArray.Create((symbol, node.Identifier))
                : EmptyLocalSymbolTokens;

        IEnumerable<(ILocalSymbol Symbol, SyntaxToken Token)>
                ToDeclaredSymbolTokens(LocalDeclarationStatementSyntax d)
            => d.Declaration
                .Variables
                .Where(v => v.Initializer is not null)
                .SelectMany(ToLocalSymbolToken);

        IEnumerable<SyntaxToken> ToTokens(
            IReadOnlyList<LocalDeclarationStatementSyntax> list)
        {
            var last = list[0];
            if (GetNextNode(last) is not {} next)
            {
                return [];
            }
            var assignSet = ToAssignSet(next);
            if (!assignSet.Any())
            {
                return [];
            }
            var map = list.SelectMany(ToDeclaredSymbolTokens)
                .ToRigidMap(p => p.Symbol, p => p.Token);
            var declaredSymbols = map.Keys;
            return !assignSet.IsSubsetOf(declaredSymbols)
                ? []
                : assignSet.Select(a => map[a]);
        }

        var all = root.DescendantNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .GroupBy(s => s.Parent)
            .SelectMany(g => SplitRow(g))
            .SelectMany(ToTokens)
            .ToList();
        foreach (var token in all)
        {
            var diagnostic = Diagnostic.Create(
                Rule, token.GetLocation(), token);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
/*
    var a = 0, b = 1, c = 2;
    if (...) {
        b = 3;
    }
*/
/*
    var v = 0;
    switch (...)
    {
        case ...:
            v = 1;
            break;
        case ...:
            v = 2;
            break;
        // [
        default:
            break;
        // ]
    }
*/
/*
    ---
    LocalDeclarationStatementSyntax
    ...
        => C<IVariableDeclaratorOperation.Symbol>
    (A|B)
    ---
    (A) IfStatementSyntax
        ---
        ExpressionStatementSyntax
            AssignmentExpressionSyntax
        ...
            => C<ILocalReferenceOperation.Local>
        ---
    ---
    (B) SwitchStatementSyntax
        ---
        SwitchSectionSyntax
            (DefaultSwitchLabelSyntax|CaseSwitchLabelSyntax)+
            ---
            ExpressionStatementSyntax
                AssignmentExpressionSyntax
            ...
                => C<ILocalReferenceOperation.Local>
            BreakStatementSyntax
            ---
            BreakStatementSyntax
            ---
        ---
    ---
*/
