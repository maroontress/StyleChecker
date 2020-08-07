#pragma warning disable RS1008

namespace StyleChecker.Refactoring.NotOneShotInitialization
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
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
            NoLocalSymbols = ImmutableHashSet.Create<ILocalSymbol>();

        private static readonly IEnumerable<(ILocalSymbol, SyntaxToken)>
            EmptyLocalSymbolTokens
                = Enumerable.Empty<(ILocalSymbol, SyntaxToken)>();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        private static void AnalyzeModel(
            SemanticModelAnalysisContext context)
        {
            var root = context.GetCompilationUnitRoot();
            var model = context.SemanticModel;

            static (SyntaxNode[] Children, int? Index)
                GetChildIndex(SyntaxNode child)
            {
                var parent = child.Parent;
                if (parent is null)
                {
                    return (Array.Empty<SyntaxNode>(), null);
                }
                var children = parent.ChildNodes()
                    .ToArray();
                if (children.Length is 0)
                {
                    return (Array.Empty<SyntaxNode>(), null);
                }
                var index = Array.FindIndex(
                    children, c => ReferenceEquals(c, child));
                var i = (index is -1) ? null : (int?)index;
                return (children, i);
            }

            static SyntaxNode? GetPreviousNode(SyntaxNode child)
            {
                var (children, i) = GetChildIndex(child);
                return (i is null || i.Value is 0)
                    ? null : children[i.Value - 1];
            }

            static SyntaxNode? GetNextNode(SyntaxNode child)
            {
                var (children, i) = GetChildIndex(child);
                return (i is null || i.Value == children.Length - 1)
                    ? null : children[i.Value + 1];
            }

            static AssignmentExpressionSyntax?
                GetSimpleAssignmentNode(SyntaxNode n)
            {
                if (!(n is ExpressionStatementSyntax expr))
                {
                    return null;
                }
                if (!(expr.Expression
                    is AssignmentExpressionSyntax a))
                {
                    return null;
                }
                return a;
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

            static IEnumerable<List<T>> SplitRow<T>(IGrouping<SyntaxNode, T> g)
                where T : SyntaxNode
            {
                return g.Where(s => !IsNextAlso(s))
                        .Select(s => NewList(s));
            }

            ILocalSymbol? ToAssignmentLocalSymbol(SyntaxNode n)
            {
                var a = GetSimpleAssignmentNode(n);
                return !(a is null)
                        && a.Left is IdentifierNameSyntax i
                        && model.GetOperation(i) is ILocalReferenceOperation o
                    ? o.Local
                    : null;
            }

            ImmutableHashSet<ILocalSymbol> ToIfAssignSet(
                IfStatementSyntax node)
            {
                var thenNode = node.Statement;
                var childNodes = Enumerables.Of<SyntaxNode>(thenNode);
                while (thenNode is BlockSyntax block)
                {
                    var first = block.Statements
                        .FirstOrDefault();
                    if (first is null)
                    {
                        return NoLocalSymbols;
                    }
                    childNodes = thenNode.ChildNodes();
                    thenNode = first;
                }
                var childSymbols = childNodes.Select(
                        ToAssignmentLocalSymbol)
                    .ToArray();
                return childSymbols.Any(n => n is null)
                    ? NoLocalSymbols
                    : childSymbols.OfType<ILocalSymbol>()
                        .ToImmutableHashSet();
            }

            ImmutableHashSet<ILocalSymbol> ToAssignBreakSet(
                SwitchSectionSyntax s)
            {
                var statements = s.Statements;
                var n = statements.Count;
                if (n < 2)
                {
                    return NoLocalSymbols;
                }
                var last = statements[n - 1];
                if (!(last is BreakStatementSyntax))
                {
                    return NoLocalSymbols;
                }
                var symbols = statements.Take(n - 1)
                    .Select(ToAssignmentLocalSymbol)
                    .ToArray();
                return symbols.Any(a => a is null)
                    ? NoLocalSymbols
                    : symbols.OfType<ILocalSymbol>()
                        .ToImmutableHashSet();
            }

            ImmutableHashSet<ILocalSymbol> ToSwitchAssignSet(
                SwitchStatementSyntax node)
            {
                var allSets = node.Sections
                    .Where(s => !ContainsDefaultSection(s))
                    .Select(ToAssignBreakSet)
                    .ToArray();
                if (allSets.Length is 0
                    || allSets.Any(s => s.IsEmpty))
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
                    .Where(s => ContainsDefaultSection(s))
                    .FirstOrDefault();
                return defaultSection is null
                        || ContainsBreakOnly(defaultSection)
                    ? firstSet
                    : NoLocalSymbols;
            }

            ImmutableHashSet<ILocalSymbol> ToAssignSet(SyntaxNode node)
            {
                if (node is IfStatementSyntax ifNode)
                {
                    return ToIfAssignSet(ifNode);
                }
                if (node is SwitchStatementSyntax switchNode)
                {
                    return ToSwitchAssignSet(switchNode);
                }
                return NoLocalSymbols;
            }

            ILocalSymbol? ToDeclarationLocalSymbol(
                VariableDeclaratorSyntax node)
            {
                var o = model.GetOperation(node);
                return o is IVariableDeclaratorOperation d
                    ? d.Symbol
                    : null;
            }

            IEnumerable<(ILocalSymbol Symbol, SyntaxToken Token)>
                ToLocalSymbolToken(VariableDeclaratorSyntax node)
            {
                var symbol = ToDeclarationLocalSymbol(node);
                return symbol is null
                    ? EmptyLocalSymbolTokens
                    : ImmutableArray.Create((symbol, node.Identifier));
            }

            IEnumerable<(ILocalSymbol Symbol, SyntaxToken Token)>
                ToDeclaredSymbolTokens(LocalDeclarationStatementSyntax d)
            {
                var variables = d.Declaration.Variables;
                return variables.Where(v => !(v.Initializer is null))
                    .SelectMany(ToLocalSymbolToken);
            }

            var all = root.DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .GroupBy(s => s.Parent)
                .SelectMany(g => SplitRow(g));
            foreach (var list in all)
            {
                var last = list[0];
                var next = GetNextNode(last);
                if (next is null)
                {
                    continue;
                }
                var assignSet = ToAssignSet(next);
                if (assignSet.IsEmpty)
                {
                    continue;
                }
                var map = list.SelectMany(ToDeclaredSymbolTokens)
                    .ToImmutableDictionary(p => p.Symbol, p => p.Token);
                var declaredSymbols = map.Keys;
                if (!assignSet.IsSubsetOf(declaredSymbols))
                {
                    continue;
                }
                foreach (var a in assignSet)
                {
                    var token = map[a];
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        token.GetLocation(),
                        token);
                    context.ReportDiagnostic(diagnostic);
                }
            }
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
