namespace StyleChecker.Refactoring.NullCheckAfterDeclaration;

using System;
using System.Collections.Frozen;
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

    private static FrozenSet<SyntaxKind> ForbiddenExpressions { get; } = new[]
    {
        SyntaxKind.ImplicitObjectCreationExpression,
        SyntaxKind.ImplicitStackAllocArrayCreationExpression,
        SyntaxKind.CollectionExpression,
        SyntaxKind.DefaultLiteralExpression,
        SyntaxKind.NullLiteralExpression,
    }.ToFrozenSet();

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
                || s.NextNode() is not IfStatementSyntax ifNode
                || IsIfNullCheck(symbolizer, ifNode) is not {} symbol
                || !SymbolEquals(o.Symbol, symbol)
                || !IsAlwaysToBeAssigned(symbolizer, symbol, ifNode))
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
                && !RequiresInference(initializerValue)
                && !DeterminesNonNull(symbolizer, initializerValue))
            ? o : null;

    private static bool DeterminesNonNull(
        ISymbolizer symbolizer, IOperation initializerValue)
    {
        var node = initializerValue.Syntax;
        return FlowStateIsNotNull(symbolizer.ToTypeInfo(node));
    }

    private static bool RequiresInference(IOperation initializerValue)
    {
        return initializerValue.Syntax is ExpressionSyntax node
            && ForbiddenExpressions.Contains(Expressions.Peel(node).Kind());
    }

    private static ILocalSymbol? IsIfNullCheck(
        ISymbolizer symbolizer, IfStatementSyntax node)
    {
        return (NullChecks.IsNullOrNonNullCheck(node) is not {} name
                || symbolizer.GetOperation(name)
                    is not ILocalReferenceOperation o)
            ? null
            : o.Local;
    }

    private static bool IsAlwaysToBeAssigned(
        ISymbolizer symbolizer,
        ILocalSymbol s,
        IfStatementSyntax ifNode)
    {
        if (NullChecks.ClassifyNullCheck(ifNode) is not {} ifNodeType)
        {
            return false;
        }

        bool IsUsedOutsideIf() => symbolizer.ToDataFlowAnalysis(ifNode)
            .ReadOutside
            .Contains(s);

        if ((ifNodeType
            ? ifNode.Statement
            : ifNode.Else?.Statement) is not {} nullClause)
        {
            /*
                if (... is not null)
                {
                    ...HERE...
                }
                // No else clause
            */

            /*
                isUsedOutsideIf
                true            => false
                false           => true
            */
            return !IsUsedOutsideIf();
        }
        /*
            if (... is null)
            {
                ...nullClause...
            }
            // No else clause

            or

            if (... is not null)
            {
                ...nonNullClause...
            }
            else
            {
                ...nullClause...
            }
        */

        bool IsEndPointReachable()
            => symbolizer.ToControlFlowAnalysis(nullClause)
                .EndPointIsReachable;

        var flowAnalysis = symbolizer.ToDataFlowAnalysis(nullClause);
        var isUsedInsideClause = flowAnalysis.ReadInside
            .Contains(s);
        var isAlwaysAssigned = flowAnalysis.AlwaysAssigned
            .Contains(s);
        /*
            isUsedInsideClause isAlwaysAssigned isUsedOutsideIf isEndPointReachable
            true               false            -               -                   => false
            false              false            true            true                => false
            false              false            true            false               => true
            false              true             true            -                   => true
            true               true             -               -                   => true
            false              -                false           -                   => true
        */
        return isUsedInsideClause
            ? isAlwaysAssigned
            : !IsUsedOutsideIf() || isAlwaysAssigned || !IsEndPointReachable();
    }

    private static bool FlowStateIsNotNull(TypeInfo typeInfo)
        => typeInfo is { Nullability.FlowState: NullableFlowState.NotNull };
}
