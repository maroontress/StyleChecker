namespace StyleChecker.Refactoring.UnnecessaryUsing;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using R = Resources;

/// <summary>
/// UnnecessaryUsing analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "UnnecessaryUsing";

    private const string Category = Categories.Refactoring;

    private static readonly DiagnosticDescriptor Rule = NewRule();

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

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context)
    {
        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;

        IEnumerable<ISymbol> ToSymbols(
            VariableDeclaratorSyntax v, Func<string, bool> matches)
        {
            return (model.GetOperation(v, cancellationToken)
                    is not IVariableDeclaratorOperation declaratorOperation
                    || v.Initializer is not {} initialzer
                    || model.GetOperation(initialzer.Value, cancellationToken)
                        is not {} operation
                    || operation.Type is not {} type
                    || !matches(TypeSymbols.GetFullName(type)))
                ? []
                : [declaratorOperation.Symbol];
        }

        IEnumerable<Diagnostic> ToDiagnostics(UsingStatementSyntax s)
        {
            var location = s.GetLocation();
            return (s.Declaration is not {} declaration)
                ? []
                : declaration.Variables
                    .SelectMany(v => ToSymbols(v, Classes.DisposesNothing))
                    .Take(1)
                    .Select(s => Diagnostic.Create(Rule, location, s.Name));
        }

        var root = model.SyntaxTree
            .GetCompilationUnitRoot(cancellationToken);
        var all = root.DescendantNodes()
            .OfType<UsingStatementSyntax>()
            .SelectMany(ToDiagnostics)
            .ToList();
        foreach (var d in all)
        {
            context.ReportDiagnostic(d);
        }
    }
}
