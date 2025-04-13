namespace StyleChecker.Analyzers.Settings.InvalidConfig;

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using Maroontress.Oxbind;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using StyleChecker.Analyzers;
using StyleChecker.Analyzers.Config;
using StyleChecker.Analyzers.Settings;
using R = Resources;

/// <summary>
/// InvalidConfig analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "InvalidConfig";

    private const string Category = Categories.Settings;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(StartAction);
    }

    private static void StartAction(
        CompilationStartAnalysisContext context)
    {
        var pod = ConfigBank.LoadRootConfig(context);
        context.RegisterCompilationEndAction(c => DiagnosticConfig(c, pod));
    }

    private static DiagnosticDescriptor NewRule()
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        return new DiagnosticDescriptor(
            DiagnosticId,
            localize(nameof(R.Title)),
            localize(nameof(R.MessageFormat)),
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static void DiagnosticConfig(
        CompilationAnalysisContext context, ConfigPod pod)
    {
        static Func<LinePosition, Location> LocationSupplier(ConfigPod pod)
                => start => pod.Path is not {} path
            ? Location.None
            : Location.Create(
                path,
                new TextSpan(0, 0),
                new LinePositionSpan(start, start));

        var toLocation = LocationSupplier(pod);
        var newLocation = (int row, int col)
            => toLocation(new LinePosition(row, col));
        var toDiagnostic = (WhereWhy e) => Diagnostic.Create(
            Rule, newLocation(e.Line - 1, e.Column - 1), e.Message);
        var toXmlExceptionDiagnostic = (XmlException e) =>
        {
            var row = e.LineNumber - 1;
            var col = e.LinePosition - 1;
            return Diagnostic.Create(Rule, newLocation(row, col), e.Message);
        };
        var toBindExceptionDiagnostic = (BindException e) =>
        {
            var info = e.LineInfo;
            var localtion = info.HasLineInfo()
                ? newLocation(info.LineNumber - 1, info.LinePosition - 1)
                : newLocation(0, 0);
            return Diagnostic.Create(Rule, localtion, e.Message);
        };
        var toExceptionDiagnostic = (Exception e)
            => Diagnostic.Create(Rule, newLocation(0, 0), e.ToString());

        if (pod.Exception is {} anyException)
        {
            var d = anyException switch
            {
                XmlException xmlException
                    => toXmlExceptionDiagnostic(xmlException),
                BindException bindException
                    => toBindExceptionDiagnostic(bindException),
                _ => toExceptionDiagnostic(anyException),
            };
            context.ReportDiagnostic(d);
            return;
        }

        var all = pod.RootConfig
            .Validate()
            .Select(e => toDiagnostic(e))
            .ToList();
        foreach (var d in all)
        {
            context.ReportDiagnostic(d);
        }
    }
}
