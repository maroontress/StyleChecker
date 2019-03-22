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
        /// <param name="atmosphere">
        /// The compilation environment.
        /// </param>
        /// <param name="toResult">
        /// The function that returns the expected diagnostic result with the
        /// specified belief.
        /// </param>
        /// <returns>
        /// The decoded source and its expected diagnostics.
        /// </returns>
        public static (string, Result[]) Decode(
            string encodedSource,
            Atmosphere atmosphere,
            Func<Belief, Result> toResult)
        {
            Belief ToBelief(Diagnostic d)
            {
                var s = d.Location
                    .GetLineSpan()
                    .StartLinePosition;
                var row = s.Line + 1;
                var column = s.Character + 1;
                return new Belief(row, column, d.GetMessage());
            }
            var rawBeliefs = NewDiagnostics(encodedSource, atmosphere)
                .Select(ToBelief)
                .ToArray();
            var rawLines = encodedSource.Split(NewLine);

            var (lines, beliefs) = Format(rawLines, rawBeliefs);
            var source = string.Join(NewLine, lines);
            var expected = beliefs
                .Select(toResult)
                .ToArray();
            return (source, expected);
        }

        private static Diagnostic[] NewDiagnostics(
            string source, Atmosphere atmosphere)
        {
            var analyzer = new BeliefExtractor();
            var baseDir = atmosphere.BasePath;
            var documents = Projects.Of(baseDir, Arrays.Create(source))
                .Documents
                .ToArray();
            return Diagnostics
                .GetSorted(analyzer, documents, atmosphere)
                .ToArray();
        }

        private static (IEnumerable<string>, IEnumerable<Belief>) Format(
            IEnumerable<string> lines, IEnumerable<Belief> beliefs)
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
            var newArray = lines.ToArray();
            var newList = new List<Belief>();
            foreach (var (row, list) in map)
            {
                var m = list.Count;
                Array.Fill(newArray, null, row, m);
                newList.AddRange(list.Select(b => b.WithRow(b.Row - sum)));
                sum += m;
            }
            return (newArray.Where(s => !(s is null)), newList);
        }
    }
}
