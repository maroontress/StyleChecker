#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.IneffectiveReadByte
{
    using System.IO;

    public sealed class Okay
    {
        public void NoInitializer(byte[] array, BinaryReader reader)
        {
            int k = 0;
            for (int i; (i = k) < 1; ++k)
            {
                array[i] = reader.ReadByte();
            }
        }
    }
}
