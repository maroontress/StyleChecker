namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    using System;

    public sealed class Code
    {
        public void ArgumentSyntax()
        {
            int GetLength(string[] array)
            {
                return array.Length;
            }
            var length = GetLength(new string[] { "a" });
            //@                        ^string[]|[]
        }

        public void One()
        {
            var all = new System.String[] { "a", };
            //@           ^System.String[]|[]
        }

        public void Two()
        {
            var all = new string[] { "a", "b", };
            //@           ^string[]|[]
        }

        public void Three()
        {
            var all = new string[] { "a", "b", "c", };
            //@           ^string[]|[]
        }

        public void StringAndNull()
        {
            var all = new string[] { "a", null, };
            //@           ^string[]|[]
        }

        public void NullCastedToString()
        {
            var all = new string[] { (string)null, };
            //@           ^string[]|[]
        }

        public void ObjectAndInt32()
        {
            var all = new object[] { (object)"a", 1, };
            //@           ^object[]|[]
        }

        public void StringInt32AndObject()
        {
            var all = new object[] { "a", 1, (object)2, };
            //@           ^object[]|[]
        }

        public T[] TypeParam<T>(T first)
        {
            return new T[] { first };
            //@        ^T[]|[]
        }

        public void TwoDimension()
        {
            var all = new string[,] { { "a", "b", }, { "c", "d", }, };
            //@           ^string[,]|[,]
        }

        public void NestedArray()
        {
            var all = new string[][]
            //@           ^string[][]|[]
            {
                new[] { "a", "b" },
                new[] { "c", "d" },
            };
        }

        public void MethodReferencesAndDeleagte()
        {
            var all = new Action[]
            //@           ^Action[]|[]
            {
                (Action)One,
                Two,
            };
        }

        public void MethodReferencesAndNull()
        {
            var all = new Action[]
            //@           ^Action[]|[]
            {
                One,
                (Action)null,
            };
        }
    }
}
