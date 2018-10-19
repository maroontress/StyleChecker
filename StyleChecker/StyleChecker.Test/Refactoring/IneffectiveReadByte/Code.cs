namespace Application
{
    using System.IO;

    public sealed class Code
    {
        public byte[] byteArray;
        public byte[] ByteArrayProperty { get; private set; } = new byte[1];
        public BinaryReader binaryReader;

        public void Field()
        {
            for (int i = 0; i < 10; ++i)
            {
                byteArray[i] = binaryReader.ReadByte();
            }
        }

        public void Local()
        {
            var stream = new MemoryStream();
            var reader = new BinaryReader(stream);
            var buffer = new byte[4];
            for (var i = 0; i < 4; ++i)
            {
                buffer[i] = reader.ReadByte();
            }
        }

        public void AutoProperty()
        {
            for (int i = 0; i < 1; ++i)
            {
                ByteArrayProperty[i] = binaryReader.ReadByte();
            }
        }
    }
}
