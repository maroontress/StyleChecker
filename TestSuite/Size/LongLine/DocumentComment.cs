#pragma warning disable CS0219

namespace Application
{
    public sealed class Code
    {
//      8      16      24      32      40      48      56      64      72
//      |       |       |       |       |       |       |       |       |
        /// <summary>
        ///                                                     This line is too long.
//@                                                                                   ^80
        ///                                               Second long line is ignored.
        /// </summary>
        public void NG()
        {
// ignored other than the first long line.
            int                                                         alpha = 1;
        }
    }
}
