namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BeliefExtractor : DiagnosticAnalyzer
    {
        private const string DiagnosticId = "ExpectationsExtractor";
        private const string Category = "Preprocessing";
        private static readonly DiagnosticDescriptor Rule = NewRule();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static DiagnosticDescriptor NewRule()
        {
            return new DiagnosticDescriptor(
                DiagnosticId,
                "Extracts comments starting with //@",
                "{0}",
                Category,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: "No description.");
        }

        private void AnalyzeSyntaxTree(
            SyntaxTreeAnalysisContext context)
        {
            const string prefix = "//@";
            var root = context.Tree.GetCompilationUnitRoot(
                context.CancellationToken);
            var all = root.DescendantTrivia()
                .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia))
                .Select(t => (trivia: t, comment: t.ToString()));
            var expectations = all
                .Where(p => p.comment.StartsWith(prefix))
                .ToArray();

            int GetLine(SyntaxTrivia trivia)
            {
                return trivia.GetLocation()
                    .GetLineSpan()
                    .StartLinePosition
                    .Line;
            }

            List<(SyntaxTrivia, string, int)> ConcatLines()
            {
                var lines = expectations.Select(e => GetLine(e.trivia))
                    .ToArray();
                var n = lines.Length;
                for (var k = 0; k < n;)
                {
                    var offset = lines[k];
                    var j = 1;
                    for (; k + j < n; ++j)
                    {
                        if (lines[k + j] != offset + j)
                        {
                            break;
                        }
                        lines[k + j] = offset;
                    }
                    k += j;
                }
                var list = new List<(SyntaxTrivia, string, int)>();
                for (var k = 0; k < n; ++k)
                {
                    var (trivia, comment) = expectations[k];
                    list.Add((trivia, comment, lines[k]));
                }
                return list;
            }

            foreach (var (trivia, comment, line) in ConcatLines())
            {
                var where = trivia.GetLocation();
                if (comment == prefix)
                {
                    throw new Exception($"{where}: invalid syntax.");
                }
                var offset = comment.IndexOf('^');
                var bodyOffset = offset;
                if (offset == -1)
                {
                    var c = comment[prefix.Length];
                    if (c == ' ')
                    {
                        throw new Exception($"{where}: '^' not found.");
                    }
                    if ("012".IndexOf(c) == -1)
                    {
                        throw new Exception($"{where}: invalid char '{c}'");
                    }
                    offset = c - '0';
                    bodyOffset = prefix.Length;
                }
                var body = comment.Substring(bodyOffset + 1);
                var lineSpan = where.GetLineSpan();
                var start = lineSpan.StartLinePosition;
                var column = start.Character + offset;

                var targetStart = new LinePosition(line - 1, column);

                Location FirstOne<T>(
                    IEnumerable<T> tree,
                    Func<T, Location> toLocation)
                {
                    return tree.Select(a => toLocation(a))
                        .FirstOrDefault(a => a.GetLineSpan()
                            .StartLinePosition == targetStart);
                }
                var allLocations = new[]
                {
                    FirstOne(
                        root.DescendantNodesAndTokens(),
                        a => a.GetLocation()),
                    FirstOne(
                        root.DescendantTrivia(),
                        a => a.GetLocation()),
                    FirstOne(
                        root.DescendantTokens(descendIntoTrivia: true),
                        a => a.GetLocation()),
                };
                var location = allLocations
                    .FirstOrDefault(a => a != null);
                if (location == null)
                {
                    throw new Exception($"{where}: no location matched.");
                }

                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    body);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
