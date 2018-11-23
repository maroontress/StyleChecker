namespace Application
{
    using System.IO;

    public sealed class Code
    {
        public void Declarations()
        {
            {
                var s = new MemoryStream();
                {
                }
            }

            {
                Stream s = new MemoryStream(), t = new MemoryStream();
                {
                }
            }

            {
                Stream s = new MemoryStream();
                using (Stream t = MyStream(s))
                {
                }
            }

            using (Stream s = MyStream())
            {
                Stream t = new MemoryStream();
                {
                }
            }

            using (Stream s = MyStream())
            {
                Stream t = new MemoryStream();
                using (Stream u = MyStream(t))
                {
                }
            }

            {
                Stream s = new MemoryStream();
                using (Stream t = MyStream())
                {
                    Stream u = new MemoryStream();
                    {
                    }
                }
            }

            {
                Stream s = new MemoryStream();
                using (Stream t = MyStream())
                {
                    Stream u = new MemoryStream();
                    using (Stream v = MyStream())
                    {
                    }
                }
            }
        }

        public void Nesting()
        {
            {
                var s = new MemoryStream();
                {
                    var t = new MemoryStream();
                    {
                    }
                }
            }

            {
                var s = new MemoryStream();
                {
                    using (var t = MyStream(s))
                    {
                    }
                }
            }

            using (var s = MyStream())
            {
                var t = new MemoryStream();
                {
                }
            }

            {
                var s = new MemoryStream();
                {
                    using (var t = MyStream(s))
                    {
                        var u = new MemoryStream();
                        {
                        }
                    }
                }
            }
        }

        public void VerbatimSymbol()
        {
            var @s = new MemoryStream();
            {
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
