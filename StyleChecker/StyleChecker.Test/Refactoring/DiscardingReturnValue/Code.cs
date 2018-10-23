namespace StyleChecker.Test.Refactoring.DiscardingReturnValue
{
    using System.IO;

    public sealed class Code
    {
        byte[] array = new byte[4];

        public void Read(Stream stream)
        {
            stream.Read(array, 0, array.Length);
        }

        public void Read(BinaryReader reader)
        {
            reader.Read(array, 0, array.Length);
        }
    }
}
