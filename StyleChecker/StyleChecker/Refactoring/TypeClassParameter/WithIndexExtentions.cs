namespace StyleChecker.Refactoring.TypeClassParameter
{
    using System.Collections;
    using System.Collections.Generic;

    public static class WithIndexExtentions
    {
        public static IEnumerable<(int index, T value)>
            WithIndex<T>(this IEnumerable<T> all)
        {
            return new Impl<T>(all);
        }

        private class Impl<T> : IEnumerable<(int, T)>
        {
            public Impl(IEnumerable<T> all)
            {
                All = all;
            }

            public IEnumerable<T> All { get; }

            public IEnumerator GetEnumerator()
            {
                var n = All.GetEnumerator();
                for (var k = 0; n.MoveNext(); ++k)
                {
                    yield return (k, n.Current);
                }
            }

            IEnumerator<(int, T)> IEnumerable<(int, T)>.GetEnumerator()
            {
                var n = All.GetEnumerator();
                for (var k = 0; n.MoveNext(); ++k)
                {
                    yield return (k, n.Current);
                }
            }
        }
    }
}
