#nullable enable
namespace Application;

public sealed class BadTypes
{
    public sealed class C1<NotT>
    //@                    ^NotT
    {
        public sealed class Inner
        {
            // In this scope, "T" becomes "C1<?>.Inner.T", so it is all right.
            public static T Default = new();

            public class T
            {
            }
        }
    }

    public sealed class C2
    {
        public sealed class T
        {
            public sealed class Inner
            {
                public static class U<NotT>
                //@                   ^NotT
                {
                }
            }

            public static class U<NotT>
            //@                   ^NotT
            {
            }
        }
    }
}
