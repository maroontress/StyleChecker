#pragma warning disable CS0219

namespace Application
{
    public sealed class Code
    {
        public void OK()
        {
            int alpha = 1;
        }

        public void NG()
        {
//      8      16      24      32      40      48      56      64      72
//      |       |       |       |       |       |       |       |       |
            int                                                         beta = 2;
// ignored other than the first long line.
            int                                                         alpha = 1;
        }
    }
}
