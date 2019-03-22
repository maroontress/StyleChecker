namespace StyleChecker.Cleaning.ByteOrderMark
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
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
            ConfigBank.LoadRootConfig(context, p => pod = p);
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(StartAction);
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
            var config = pod.RootConfig.ByteOrderMark;
            var allFiles = config.GetPaths();
            foreach (var file in allFiles)
            {
                void Report(DiagnosticDescriptor d)
                    => context.ReportDiagnostic(
                        Diagnostic.Create(d, Location.None, file));

                ReportIfFileStartsWithUtf8Bom(file, Report);
            }
        }

        private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
        {
            var tree = context.Tree;
            var encoding = tree.Encoding;
            var path = tree.FilePath;

            if (encoding != null
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
            using (var stream = new FileStream(path, FileMode.Open))
            {
                ReadFully(stream, array, 0, array.Length);
            }
            return Enumerable.SequenceEqual(array, Utf8ByteOrderMark);
        }

        private void StartAction(CompilationStartAnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(AnalyzeTree);
            context.RegisterCompilationEndAction(
                c => CheckCustomFiles(c, pod));
        }
    }
}
