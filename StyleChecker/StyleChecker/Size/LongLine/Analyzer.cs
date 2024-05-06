namespace StyleChecker.Size.LongLine;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Settings;
using R = Resources;

/// <summary>
/// LongLine analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "LongLine";

    private const string Category = Categories.Size;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        ConfigBank.LoadRootConfig(context, StartAction);
        context.EnableConcurrentExecution();
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

    private static void AnalyzeSyntaxTree(
        SyntaxTreeAnalysisContext context,
        ConfigPod pod)
    {
        var config = pod.RootConfig.LongLine;
        var maxLineLength = config.GetMaxLineLength();
        var isOver = (Location w) => w.GetLineSpan()
            .StartLinePosition
            .Character >= maxLineLength;
        var toDiagnostic = (Location w) => Diagnostic.Create(
            Rule, w, maxLineLength);
        var root = context.Tree
            .GetCompilationUnitRoot(context.CancellationToken);
        var firstTriviaOrEmpty = root.DescendantTrivia()
            .Where(t => t.IsKind(SyntaxKind.EndOfLineTrivia))
            .Select(t => t.GetLocation())
            .Where(isOver)
            .Take(1);
        var firstTokenOrEmpty = root.DescendantTokens(descendIntoTrivia: true)
            .Where(t => t.IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
            .Select(t => t.GetLocation())
            .Where(isOver)
            .Take(1);
        var atMostOne = firstTokenOrEmpty.Concat(firstTriviaOrEmpty)
            .OrderBy(i => i.SourceSpan.Start)
            /*
                Note that this diagnostic only reports the first line in the
                file that exceeds the limit.
            */
            .Take(1)
            .Select(toDiagnostic)
            .ToList();
        foreach (var d in atMostOne)
        {
            context.ReportDiagnostic(d);
        }
    }

    private static void StartAction(
        CompilationStartAnalysisContext context, ConfigPod pod)
    {
        context.RegisterSyntaxTreeAction(c => AnalyzeSyntaxTree(c, pod));
    }
}
