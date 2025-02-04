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
        var model = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        IOperation? ToOperation(SyntaxNode s)
            => model.GetOperation(s, cancellationToken);

        return s =>
        {
            if (s.UsingKeyword != default
                || GetLastDeclarator(s, ToOperation) is not {} o
                || s.NextNode() is not IfStatementSyntax nextNode
                || IsIfNullCheck(model, nextNode) is not {} symbol
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
            LocalDeclarationStatementSyntax s,
            Func<SyntaxNode, IOperation?> toOperation)
        /*
            The 'variables.Count' is greater than zero if there are no
            compilation errors, but check to be sure.
        */
        => (s.Declaration.Variables is { Count: > 0 } variables
                && toOperation(variables.Last())
                    is IVariableDeclaratorOperation o)
            ? o : null;

    private static ILocalSymbol? IsIfNullCheck(
        SemanticModel model, IfStatementSyntax node)
    {
        return (NullChecks.IsNullOrNonNullCheck(node) is not {} name
                || model.GetOperation(name) is not ILocalReferenceOperation o)
            ? null
            : o.Local;
    }
}
