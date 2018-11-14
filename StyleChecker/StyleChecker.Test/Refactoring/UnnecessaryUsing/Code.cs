namespace Application
{
    using System.IO;

    public sealed class Code
    {
        public void Declarations()
        {
            using (var s = new MemoryStream())
//@         ^s
            {
            }

            using (Stream s = new MemoryStream(),
//@         ^s
                t = new MemoryStream())
            {
            }

            using (Stream s = new MemoryStream(),
//@         ^s
                t = MyStream(s))
            {
            }

            using (Stream s = MyStream(),
//@         ^t
                t = new MemoryStream())
            {
            }

            using (Stream s = MyStream(),
//@         ^t
                t = new MemoryStream(),
                u = MyStream(t))
            {
            }

            using (Stream s = new MemoryStream(),
//@         ^s
                t = MyStream(),
                u = new MemoryStream())
            {
            }

            using (Stream s = new MemoryStream(),
//@         ^s
                t = MyStream(),
                u = new MemoryStream(),
                v = MyStream())
            {
            }
        }

        public void Nesting()
        {
            using (var s = new MemoryStream())
//@         ^s
            {
                using (var t = new MemoryStream())
//@             ^t
                {
                }
            }

            using (var s = new MemoryStream())
//@         ^s
            {
                using (var t = MyStream(s))
                {
                }
            }

            using (var s = MyStream())
            {
                using (var t = new MemoryStream())
//@             ^t
                {
                }
            }

            using (var s = new MemoryStream())
//@         ^s
            {
                using (var t = MyStream(s))
                {
                    using (var u = new MemoryStream())
//@                 ^u
                    {
                    }
                }
            }
        }

        public Stream MyStream()
        {
            return new MemoryStream();
        }

        public Stream MyStream(Stream s)
        {
            return new MemoryStream();
        }
    }
}
