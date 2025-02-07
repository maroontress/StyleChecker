#nullable enable
namespace Application;

public sealed class BadMembers
{
    public sealed class T
    {
        public static void M<NotT>()
        //@                  ^NotT
        {
        }
    }
}
