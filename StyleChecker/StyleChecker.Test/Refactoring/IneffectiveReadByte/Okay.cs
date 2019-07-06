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

        public void NoDeclaration(byte[] array, BinaryReader reader)
        {
            int i = 0;
            for (; i < 1; ++i)
            {
                array[i] = reader.ReadByte();
            }
        }

        public void NoIncrementor(byte[] array, BinaryReader reader)
        {
            for (int i = 0; i < 1;)
            {
                array[i] = reader.ReadByte();
            }
        }

        public void NoCondition(byte[] array, BinaryReader reader)
        {
            for (int i = 0;; ++i)
            {
                array[i] = reader.ReadByte();
            }
        }

        public void RightOperandOfConditionIsNotConstant(
            byte[] array, BinaryReader reader)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                array[i] = reader.ReadByte();
            }
        }

        public void ConditionIsConstant(byte[] array, BinaryReader reader)
        {
            for (int i = 0; true; ++i)
            {
                array[i] = reader.ReadByte();
            }
        }

        public void ConditionIsUnaryOperator(byte[] array, BinaryReader reader)
        {
            bool b = false;
            for (int i = 0; !b; ++i)
            {
                array[i] = reader.ReadByte();
            }
        }
    }
}
