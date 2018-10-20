namespace Application
{
    using System.IO;

    public sealed class Code
    {
        public void Declarations()
        {
            using (var s = new MemoryStream())
            {
            }

            using (Stream s = new MemoryStream(),
                t = new MemoryStream())
            {
            }

            using (Stream s = new MemoryStream(),
                t = MyStream(s))
            {
            }

            using (Stream s = MyStream(),
                t = new MemoryStream())
            {
            }

            using (Stream s = MyStream(),
                 t = new MemoryStream(),
                 u = MyStream(t))
            {
            }

            using (Stream s = new MemoryStream(),
                 t = MyStream(),
                 u = new MemoryStream())
            {
            }

            using (Stream s = new MemoryStream(),
                 t = MyStream(),
                 u = new MemoryStream(),
                 v = MyStream())
            {
            }
        }

        public void Nesting()
        {
            using (var s = new MemoryStream())
            {
                using (var t = new MemoryStream())
                {
                }
            }

            using (var s = new MemoryStream())
            {
                using (var t = MyStream(s))
                {
                }
            }

            using (var s = MyStream())
            {
                using (var t = new MemoryStream())
                {
                }
            }

            using (var s = new MemoryStream())
            {
                using (var t = MyStream(s))
                {
                    using (var u = new MemoryStream())
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
