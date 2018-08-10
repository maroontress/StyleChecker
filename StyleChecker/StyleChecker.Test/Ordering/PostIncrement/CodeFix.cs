namespace Application
{
    using System;

    public sealed class Code
    {
        public int alpha = 1;
        public int beta = 2;

        public void OK()
        {
            ++alpha;
            --beta;
            for (int k = 0; k++ < 10; ++k)
            {
            }
            if (alpha-- > 0)
            {
            }
            while (beta++)
            {
            }
            Func<int> a = () => alpha--;
            Func<int> b = () =>
            {
                return alpha--;
            };
        }

        public void Note()
        {
            // Following lines should be reported as the side effect is
            // ignored, not by this analyzer.
            Func<int, int> c = i => i++;
            Func<int> d = () =>
            {
                var beta = 3;
                return beta++;
            };
            alpha = alpha++;
        }

        public void NG()
        {
            ++alpha;
            --beta;
            for (int k = 0; k < 10; ++k)
            {
            }
        }
    }
}
