namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    using System.Collections.Immutable;
    using System.IO;
    using StyleChecker.Annotations;

    public sealed class Code
    {
        private byte[] array = new byte[4];

        public void Read(Stream stream)
        {
            stream.Read(array, 0, array.Length);
//@         ^System.IO.Stream.${Read}
        }

        public void Read(BinaryReader reader)
        {
            reader.Read(array, 0, array.Length);
//@         ^System.IO.BinaryReader.${Read}
        }

        public void DoNotIgonore(Stream stream)
        {
            Read(stream, array);
//@         ^StyleChecker.Test.Refactoring.DiscardingReturnValue.Code.Read(System.IO.Stream, byte[])
        }

        [return: DoNotIgnore]
        public int Read(Stream stream, byte[] buffer)
        {
            return stream.Read(buffer, 0, buffer.Length);
        }

        public void StringClass()
        {
            var s = "Hello, World.";
            s.IndexOf('W');
//@         ^string.IndexOf(char)
            s.ToLower();
//@         ^string.ToLower()
        }

        public void TypeClass()
        {
            var s = "Hello, World.";
            var t = s.GetType();
            t.GetMembers();
//@         ^System.Type.GetMembers()
        }

        public void ImmutableArrayClass()
        {
            ImmutableArray.Create("a");
//@         ^System.Collections.Immutable.ImmutableArray.Create<T>(T)
            var b = ImmutableArray.Create("b");
            b.Add("c");
//@         ^System.Collections.Immutable.ImmutableArray<T>.Add(T)
        }
    }
}
