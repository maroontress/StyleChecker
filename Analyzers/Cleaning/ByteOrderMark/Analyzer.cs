namespace Analyzers.Cleaning.ByteOrderMark;

using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Analyzers.Settings;
using Maroontress.Roastery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using R = Resources;

/// <summary>
/// ByteOrderMark (BOM) analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = nameof(ByteOrderMark);

    private const string Category = Categories.Cleaning;

    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        ConfigBank.LoadRootConfig(context, StartAction);
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

    private static void CheckCustomFiles(
        CompilationAnalysisContext context, ConfigPod pod)
    {
        static Regex NewRegex(string p)
        {
            const RegexOptions options = RegexOptions.CultureInvariant
                | RegexOptions.Singleline;
            return new Regex(p, options);
        }

        var config = pod.RootConfig.ByteOrderMark;
        var maxDepth = config.GetMaxDepth();
        var globs = config.GetGlobs();
        if (!globs.Any())
        {
            return;
        }

        var baseDir = Path.GetDirectoryName(pod.Path)
            .OrElseIfEmpty(".");
        var pattern = Globs.ToPattern(globs);
        var regex = NewRegex(pattern);
        var prefix = baseDir + Path.DirectorySeparatorChar;
        var allFiles = PathFinder.GetFiles(baseDir, maxDepth)
            .Where(f => f.StartsWith(prefix, StringComparison.Ordinal))
            .Select(f => f.Substring(prefix.Length)
                .Replace(Path.DirectorySeparatorChar, '/'))
            .Where(f => regex.IsMatch(f))
            .Select(f => f.Replace('/', Path.DirectorySeparatorChar));

        foreach (var file in allFiles)
        {
            void Report(DiagnosticDescriptor d)
                => context.ReportDiagnostic(
                    Diagnostic.Create(d, Location.None, file));

            var path = Path.Combine(baseDir, file);
            ReportIfFileStartsWithUtf8Bom(path, Report);
        }
    }

    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        var tree = context.Tree;
        var encoding = tree.Encoding;
        var path = tree.FilePath;

        if (encoding is not null
            && !encoding.Equals(Encoding.UTF8))
        {
            return;
        }

        Location Where()
            => Location.Create(tree, new TextSpan(0, 0));

        void Report(DiagnosticDescriptor d)
            => context.ReportDiagnostic(
                Diagnostic.Create(d, Where(), path));

        ReportIfFileStartsWithUtf8Bom(path, Report);
    }

    private static void ReportIfFileStartsWithUtf8Bom(
        string path, Action<DiagnosticDescriptor> action)
    {
        if (BomKit.StartsWithBom(path))
        {
            action(Rule);
        }
    }

    private void StartAction(
        CompilationStartAnalysisContext context, ConfigPod pod)
    {
        context.RegisterSyntaxTreeAction(AnalyzeTree);
        context.RegisterCompilationEndAction(
            c => CheckCustomFiles(c, pod));
    }
}
