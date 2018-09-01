#pragma warning disable CS0162
#pragma warning disable CS0219

namespace Application
{
    using System;

    public sealed class Code
    {
        public void OK()
        {
            for (int k = 0;;)
            {
            }
            for (int k = 0; k < 0;)
            {
            }
            for (int k = 0; k < 10; ++k)
            {
            }
            for (; false;)
            {
            }
            for (;;)
            {
            }
            for (var k = 0;
                k< 10;
                ++k)
            {
            }
            Console.WriteLine(""); /**/
            Console.WriteLine(""); //
        }

        public void NG()
        {
            Console.WriteLine(""); /**/
            Console.WriteLine(""); //
            for (int k = 0; k < 10;)
            {
            }
            for (int k = 0; k < 10; ++k)
            {
            }
            for (; false;)
            {
            }
        }
    }
}
