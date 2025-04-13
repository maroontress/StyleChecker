namespace StyleChecker.Analyzers.Naming.Underscore;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Analyzers;
using R = Resources;

/// <summary>
/// Underscore analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "Underscore";

    private const string Category = Categories.Naming;
    private static readonly DiagnosticDescriptor IsRule = NewIsRule();
    private static readonly DiagnosticDescriptor IncludeRule
        = NewIncludeRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [IncludeRule, IsRule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static DiagnosticDescriptor NewIncludeRule()
        => NewRule(nameof(R.IncludeMessageFormat));

    private static DiagnosticDescriptor NewIsRule()
        => NewRule(nameof(R.IsMessageFormat));

    private static DiagnosticDescriptor NewRule(string messageFormat)
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        return new DiagnosticDescriptor(
            DiagnosticId,
            localize(nameof(R.Title)),
            localize(messageFormat),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static void AnalyzeSyntaxTree(
        SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetCompilationUnitRoot(
            context.CancellationToken);

        static bool ContainsUndersore(SyntaxToken t)
            => t.Text.IndexOf('_') is not -1;

        var all = LocalVariables.DeclarationTokens(root)
            .Concat(LocalVariables.OutVariableTokens(root))
            .Concat(LocalVariables.PatternMatchingTokens(root))
            .Concat(LocalVariables.ParameterTokens(root))
            .Concat(LocalVariables.FunctionTokens(root))
            .Concat(LocalVariables.CatchTokens(root))
            .Concat(LocalVariables.ForEachTokens(root))
            .Where(ContainsUndersore)
            .ToList();
        foreach (var token in all)
        {
            var diagnostic = Diagnostic.Create(
                token.ValueText is "_"
                    ? IsRule
                    : IncludeRule,
                token.GetLocation(),
                token);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
