namespace StyleChecker.Test.Framework;

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
        SupportedDiagnostics => [Rule];

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

    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        static int GetLine(SyntaxTrivia trivia)
            => trivia.GetLocation()
                .GetLineSpan()
                .StartLinePosition
                .Line;

        static int GetCount(string s, char c)
        {
            var k = 0;
            var n = s.Length;
            while (k < n && s[k] == c)
            {
                ++k;
            }
            return k;
        }

        static KeyValuePair<string, string?> ToPair(string key, string? value)
            => KeyValuePair.Create(key, value);

        const string prefix = "//@";
        var root = context.Tree.GetCompilationUnitRoot(
            context.CancellationToken);
        var expectations = root.DescendantTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia))
            .Select(t => (Trivia: t, Comment: t.ToString()))
            .Where(p => p.Comment.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        List<(SyntaxTrivia, string, int)> ConcatLines()
        {
            var lines = expectations.Select(e => GetLine(e.Trivia))
                .ToList();
            var n = lines.Count;
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
            return [.. expectations.Zip(
                lines, (i, w) => (i.Trivia, i.Comment, w))];
        }

        foreach (var (trivia, comment, line) in ConcatLines())
        {
            var where = trivia.GetLocation();
            if (comment == prefix)
            {
                throw new Exception($"{where}: invalid syntax");
            }

            (int Offset, int BodyOffset) GetOffset()
            {
                var bodyOffset = prefix.Length;
                var c = comment[bodyOffset];
                var leftSideOffset = "012".IndexOf(c);
                if (leftSideOffset is not -1)
                {
                    return (leftSideOffset, bodyOffset);
                }
                var offset = comment.IndexOf('^');
                if (offset is -1)
                {
                    throw new Exception($"{where}: invalid char '{c}'; "
                        + "must be followed by [012] or contain '^'");
                }
                return (offset, offset);
            }

            var (offset, bodyOffset) = GetOffset();
            var hatsFollowedByBody = comment[(bodyOffset + 1)..];
            var hatDelta = GetCount(hatsFollowedByBody, '^');
            var body = hatsFollowedByBody[hatDelta..];
            var lineSpan = where.GetLineSpan();
            var start = lineSpan.StartLinePosition;
            var column = start.Character + offset;
            var delta = 1 + hatDelta;
            var targetStart = new LinePosition(line - delta, column);

            Location? FirstOne<T>(
                IEnumerable<T> tree, Func<T, Location?> toLocation)
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
            if (Array.Find(allLocations, a => a is not null)
                is not {} location)
            {
                throw new NullReferenceException(
                    $"{where}: no location matched");
            }

            var parameters = ImmutableDictionary.CreateRange(
                [ToPair("delta", delta.ToString())]);
            var diagnostic = Diagnostic.Create(
                Rule, location, parameters, body);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
