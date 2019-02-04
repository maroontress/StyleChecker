namespace StyleChecker.Test.Cleaning.RedundantTypedArrayCreation
{
    public sealed class Code
    {
        public void One()
        {
            var all = new System.String[] { "a", };
            //@           ^System.String
        }

        public void Two()
        {
            var all = new string[] { "a", "b", };
            //@           ^string
        }

        public void Three()
        {
            var all = new string[] { "a", "b", "c", };
            //@           ^string
        }

        public void StringAndNull()
        {
            var all = new string[] { "a", null, };
            //@           ^string
        }

        public void NullCastedToString()
        {
            var all = new string[] { (string)null, };
            //@           ^string
        }

        public void ObjectAndInt32()
        {
            var all = new object[] { (object)"a", 1, };
            //@           ^object
        }

        public void StringInt32AndObject()
        {
            var all = new object[] { "a", 1, (object)2, };
            //@           ^object
        }

        public T[] TypeParam<T>(T first)
        {
            return new T[] { first };
            //@        ^T
        }
    }
}
