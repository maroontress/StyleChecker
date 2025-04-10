#nullable enable
namespace Application;

public sealed class GoodMembers
{
    public sealed class M1<T>
    {
        // Renaming NotT to T causes a warning CS0693 at the line /*💀*/
        /*💀*/ public static void M<NotT>()
        {
        }
    }

    public sealed class M2<T>
    {
        public sealed class Inner
        {
            // Renaming NotT to T causes a warning CS0693 at the line /*💀*/
            /*💀*/ public static void M<NotT>()
            {
            }
        }
    }

    public sealed class M3
    {
        // Renaming NotT to T causes an error CS0412 at the line /*💀*/
        public static int M<NotT>()
        {
            /*💀*/ var T = 0;
            return T;
        }
    }

    public sealed class M4
    {
        // Renaming NotT to T causes an error CS0412 at the line /*💀*/
        public static void M<NotT>()
        {
            /*💀*/ static void T()
            {
            }

            T();
        }
    }
}
