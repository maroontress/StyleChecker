namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Maroontress.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Extracts <see cref="Belief"/>s embedded in a C# source code.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BeliefExtractor : DiagnosticAnalyzer
    {
        /// <summary>
        /// The identifier of this analyzer.
        /// </summary>
        public const string DiagnosticId = "ExpectationsExtractor";

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
                .Where(p => p.comment.StartsWith(
                    prefix, StringComparison.Ordinal))
                .ToArray();

            static int GetLine(SyntaxTrivia trivia)
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

            static int GetCountAt(string c)
            {
                var k = prefix.Length;
                var count = 1;
                var n = c.Length;
                while (k < n && c[k] is '@')
                {
                    ++count;
                    ++k;
                }
                return count;
            }

            foreach (var (trivia, comment, line) in ConcatLines())
            {
                var where = trivia.GetLocation();
                if (comment == prefix)
                {
                    throw new Exception($"{where}: invalid syntax.");
                }
                var delta = GetCountAt(comment);

                (int Offset, int BodyOffset) GetOffset()
                {
                    var offset = comment.IndexOf('^');
                    if (!(offset is -1))
                    {
                        return (offset, offset);
                    }
                    var bodyOffset = prefix.Length + delta - 1;
                    var c = comment[bodyOffset];
                    if (c is ' ')
                    {
                        throw new Exception($"{where}: '^' not found.");
                    }
                    if ("012".IndexOf(c) is -1)
                    {
                        throw new Exception($"{where}: invalid char '{c}'");
                    }
                    return (c - '0', bodyOffset);
                }

                var (offset, bodyOffset) = GetOffset();
                var body = comment.Substring(bodyOffset + 1);
                var lineSpan = where.GetLineSpan();
                var start = lineSpan.StartLinePosition;
                var column = start.Character + offset;

                var targetStart = new LinePosition(line - delta, column);

                Location? FirstOne<T>(
                    IEnumerable<T> tree,
                    Func<T, Location?> toLocation)
                {
                    return tree.Select(a => toLocation(a))
                        .FilterNonNullReference()
                        .FirstOrDefault(a => a.GetLineSpan()
                            .StartLinePosition == targetStart);
                }
                var allLocations = new[]
                {
                    FirstOne(
                        root.DescendantNodesAndTokens(),
                        a => a.GetLocation()),
                    FirstOne(
                        root.DescendantTrivia(descendIntoTrivia: true),
                        a => a.GetLocation()),
                    FirstOne(
                        root.DescendantTokens(descendIntoTrivia: true),
                        a => a.GetLocation()),
                };
                var location = Array.Find(allLocations, a => !(a is null));
                if (location is null)
                {
                    throw new NullReferenceException(
                        $"{where}: no location matched.");
                }

                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    new Dictionary<string, string>()
                    {
                        ["delta"] = delta.ToString(),
                    }.ToImmutableDictionary(),
                    body);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
