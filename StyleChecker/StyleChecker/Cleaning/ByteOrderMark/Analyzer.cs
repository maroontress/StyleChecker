namespace StyleChecker.Cleaning.ByteOrderMark
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;
    using StyleChecker.Settings;
    using R = Resources;

    /// <summary>
    /// ByteOrderMark (BOM) analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = nameof(ByteOrderMark);

        private const string Category = Categories.Cleaning;

        private static readonly ImmutableArray<byte> Utf8ByteOrderMark
            = ImmutableArray.Create<byte>(0xef, 0xbb, 0xbf);

        private static readonly DiagnosticDescriptor Rule = NewRule();

        private static readonly DiagnosticDescriptor NotFoundRule
            = NewNotFoundRule();

        private ConfigPod pod;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule, NotFoundRule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            void EndAction(CompilationAnalysisContext c)
                => CheckCustomFiles(c, pod);

            void StartAction(CompilationStartAnalysisContext c, ConfigPod p)
            {
                pod = p;
                c.RegisterSyntaxTreeAction(AnalyzeTree);
                c.RegisterCompilationEndAction(EndAction);
            }

            ConfigBank.LoadRootConfig(context, StartAction);
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
        }

        private static DiagnosticDescriptor NewRule()
           => NewRule(
               nameof(R.MessageFormat), DiagnosticSeverity.Warning);

        private static DiagnosticDescriptor NewNotFoundRule()
            => NewRule(
               nameof(R.NotFoundMessageFormat), DiagnosticSeverity.Error);

        private static DiagnosticDescriptor NewRule(
            string format, DiagnosticSeverity severity)
        {
            var localize = Localizers.Of<R>(R.ResourceManager);
            return new DiagnosticDescriptor(
                DiagnosticId,
                localize(nameof(R.Title)),
                localize(format),
                Category,
                severity,
                isEnabledByDefault: true,
                description: localize(nameof(R.Description)),
                helpLinkUri: HelpLink.ToUri(DiagnosticId));
        }

        private static void CheckCustomFiles(
            CompilationAnalysisContext context, ConfigPod pod)
        {
            Regex NewRegex(string p)
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

            var baseDir = Path.GetDirectoryName(pod.Path);
            if (baseDir.Length == 0)
            {
                baseDir = ".";
            }
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

            if (!(encoding is null)
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
            try
            {
                if (!StartsWithUtf8Bom(path))
                {
                    return;
                }
                action(Rule);
            }
            catch (EndOfStreamException)
            {
                return;
            }
            catch (DirectoryNotFoundException)
            {
                action(NotFoundRule);
            }
            catch (FileNotFoundException)
            {
                action(NotFoundRule);
            }
        }

        private static bool StartsWithUtf8Bom(string path)
        {
            void ReadFully(Stream s, byte[] a, int o, int n)
            {
                var offset = o;
                var length = n;
                while (length > 0)
                {
                    var size = s.Read(a, offset, length);
                    if (size == 0)
                    {
                        throw new EndOfStreamException();
                    }
                    offset += size;
                    length -= size;
                }
            }

            var array = new byte[Utf8ByteOrderMark.Length];
            using (var stream = File.OpenRead(path))
            {
                ReadFully(stream, array, 0, array.Length);
            }
            return array.SequenceEqual(Utf8ByteOrderMark);
        }
    }
}
