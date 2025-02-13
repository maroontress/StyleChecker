namespace StyleChecker.Refactoring.NullCheckAfterDeclaration;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using R = Resources;

/// <summary>
/// NullCheckAfterDeclaration analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "NullCheckAfterDeclaration";

    private const string Category = Categories.Refactoring;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    private static Func<ISymbol, ISymbol, bool> SymbolEquals { get; }
        = SymbolEqualityComparer.Default.Equals;

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
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        var token = context.CancellationToken;
        var model = context.SemanticModel;
        var toImplicitTypeDiagnostics = NewDiagnosticsFactory(context);
        var root = model.SyntaxTree
            .GetCompilationUnitRoot(token);
        var diagnostics = root.DescendantNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .SelectMany(toImplicitTypeDiagnostics)
            .ToList();
        foreach (var d in diagnostics)
        {
            context.ReportDiagnostic(d);
        }
    }

    private static Func<LocalDeclarationStatementSyntax,
            IEnumerable<Diagnostic>>
        NewDiagnosticsFactory(SemanticModelAnalysisContext context)
    {
        var symbolizer = context.GetSymbolizer();
        return s =>
        {
            if (s.UsingKeyword != default
                || GetLastDeclarator(symbolizer, s) is not {} o
                || s.NextNode() is not IfStatementSyntax nextNode
                || IsIfNullCheck(symbolizer, nextNode) is not {} symbol
                || !SymbolEquals(o.Symbol, symbol))
            {
                return [];
            }
            var id = symbol.ToString();
            var location = o.Syntax.GetLocation();
            var d = Diagnostic.Create(Rule, location, id);
            return [d];
        };
    }

    private static IVariableDeclaratorOperation? GetLastDeclarator(
            ISymbolizer symbolizer, LocalDeclarationStatementSyntax s)
        /*
            The 'variables.Count' is greater than zero if there are no
            compilation errors, but check to be sure.
        */
        => (s.Declaration.Variables is { Count: > 0 } variables
            && symbolizer.GetOperation(variables.Last())
                is IVariableDeclaratorOperation o
            && o.Initializer?.Value is {} initializerValue
            && initializerValue.Type is {} valueType
            && valueType.IsReferenceType
            && !FlowStateIsNotNull(
                symbolizer.ToTypeInfo(initializerValue.Syntax))
            && IsTherePlaceWhereItIsReadAndMaybeNull(symbolizer, s, o.Symbol))
            ? o : null;

    private static ILocalSymbol? IsIfNullCheck(
        ISymbolizer symbolizer, IfStatementSyntax node)
    {
        return (NullChecks.IsNullOrNonNullCheck(node) is not {} name
                || symbolizer.GetOperation(name)
                    is not ILocalReferenceOperation o)
            ? null
            : o.Local;
    }

    private static bool IsTherePlaceWhereItIsReadAndMaybeNull(
        ISymbolizer symbolizer,
        LocalDeclarationStatementSyntax d,
        ILocalSymbol s)
    {
        static Func<IdentifierNameSyntax, bool> ToPredicate(
            ISymbolizer symbolizer, ILocalSymbol s)
        {
            var id = s.Name;
            return n => n.Identifier.ValueText == id
                && GetContainingStatementOrExpression(n) is {} node
                && symbolizer.ToDataFlowAnalysis(node)
                    .ReadInside
                    .Contains(s)
                && !FlowStateIsNotNull(symbolizer.ToTypeInfo(n));
        }

        var isReadAndMaybeNull = ToPredicate(symbolizer, s);
        return d.Parent is BlockSyntax block
            && !block.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(isReadAndMaybeNull)
                /* Skip the first element in the if statement. */
                .Skip(1)
                .Any();
    }

    private static bool FlowStateIsNotNull(TypeInfo typeInfo)
        => typeInfo is { Nullability.FlowState: NullableFlowState.NotNull };

    private static SyntaxNode? GetContainingStatementOrExpression(
        SyntaxNode node)
    {
        var n = node.Parent;
        while (n is not (null or StatementSyntax or ExpressionSyntax))
        {
            n = n.Parent;
        }
        return n;
    }
}
