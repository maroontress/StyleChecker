#pragma warning disable CS8619

namespace StyleChecker.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Provides a way to extract the source and its expected diagnostics from
    /// another source where the beliefs have been embedded.
    /// </summary>
    public static class Beliefs
    {
        private static readonly string NewLine = Environment.NewLine;

        /// <summary>
        /// Extracts the source and its expected diagnostics.
        /// </summary>
        /// <param name="encodedSource">
        /// The encoded source where the beliefs have been embedded.
        /// </param>
        /// <param name="excludeIds">
        /// The diagnostics IDs that are ignored.
        /// </param>
        /// <param name="toResult">
        /// The function that returns the expected diagnostic result with the
        /// specified belief.
        /// </param>
        /// <returns>
        /// The decoded source and its expected diagnostics.
        /// </returns>
        public static (string source, Result[] expected) Decode(
            string encodedSource,
            IEnumerable<string> excludeIds,
            Func<Belief, Result> toResult)
        {
            static Belief ToBelief(Diagnostic d)
            {
                var s = d.Location
                    .GetLineSpan()
                    .StartLinePosition;
                var row = s.Line + 1;
                var column = s.Character + 1;
                return new Belief(row, column, d.GetMessage());
            }

            var rawBeliefs = NewDiagnostics(encodedSource, excludeIds)
                .Select(ToBelief)
                .ToArray();
            if (rawBeliefs.Length is 0)
            {
                throw new CompilationException("No Beliefs extracted.");
            }
            var rawLines = encodedSource.Split(NewLine);

            var (lines, beliefs) = Format(rawLines, rawBeliefs);
            var source = string.Join(NewLine, lines);
            var expected = beliefs
                .Select(toResult)
                .ToArray();
            return (source, expected);
        }

        private static IEnumerable<Diagnostic> NewDiagnostics(
            string source, IEnumerable<string> excludeIds)
        {
            var analyzer = new BeliefExtractor();
            var atmosphere = Atmosphere.Default
                .WithExcludeIds(excludeIds);
            var documents = Projects.Of(atmosphere, source)
                .Documents;
            return Diagnostics
                .GetSorted(analyzer, documents, atmosphere);
        }

        private static (IEnumerable<string> lines, IEnumerable<Belief> beliefs)
            Format(IEnumerable<string> lines, IEnumerable<Belief> beliefs)
        {
            /*
                        _Before_            _After_

                lines:
                      | ...                 ...
                r1 - 1| ____a b             ____a b
                r1 + 0| //@ ^m1a            null
                r1 + 1| //@   ^m1b          null
                      | ...                 ...
                r2 - 1| ____a b             ____a b
                r2 + 0| //@ ^m2a            null
                r2 + 1| //@   ^m2b          null
                      | ...                 ...

                beliefs:
                      | ...                 ...
                k1    | (r1, ?, m1a)        (r1 - o1, ?, m1a)
                k1 + 1| (r1, ?, m1b)        (r1 - o1, ?, m1b)
                      | ...                 ...
                k2    | (r2, ?, m2a)        (r2 - o2, ?, m2a)
                k2 + 1| (r2, ?, m2b)        (r2 - o2, ?, m2b)
                      | ...                 ...

                # o2 = o1 + count(r1)
            */
            var map = new Dictionary<int, List<Belief>>();
            foreach (var b in beliefs)
            {
                if (map.TryGetValue(b.Row, out var list))
                {
                    list.Add(b);
                }
                else
                {
                    map[b.Row] = new List<Belief> { b };
                }
            }
            var sum = 0;
            string?[] newArray = lines.ToArray();
            var newList = new List<Belief>();
            foreach (var (row, list) in map)
            {
                var m = list.Count;
                Array.Fill(newArray, null, row, m);
                newList.AddRange(list.Select(b => b.WithRow(b.Row - sum)));
                sum += m;
            }
            return (newArray.OfType<string>(), newList);
        }
    }
}
