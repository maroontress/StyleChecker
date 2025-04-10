#nullable enable
namespace Application;

using System;

public sealed class GoodTypes
{
    // Renaming NotT to T causes a warning CS0693 at the line /*💀*/
    public sealed class C2<NotT>
    {
        public sealed class Inner
        {
            /*💀*/ public static class U<T>
            {
            }
        }

        /*💀*/ public void S<T>()
        {
        }
    }

    public sealed class C3<T>
    {
        public sealed class Inner
        {
            // Renaming NotT to T causes a warning CS0693 at the line /*💀*/
            /*💀*/ public static class U<NotT>
            {
            }
        }
    }

    public sealed class C4
    {
        // Renaming NotT to T causes an error CS0694 at the line /*💀*/
        /*💀*/ public sealed class T<NotT>
        {
        }
    }

    public sealed class C5_Method
    {
        // Renaming NotT to T causes an error CS0102 at the line /*💀*/
        public sealed class U<NotT>
        {
            // A MemberDeclaration with the name "T" exists in this scope.
            /*💀*/ public void T()
            {
            }
        }
    }

    public sealed class C5_Field
    {
        // Renaming NotT to T causes an error CS0102 at the line /*💀*/
        public sealed class U<NotT>
        {
            /*💀*/ public int T;
        }
    }

    public sealed class C5_Property
    {
        // Renaming NotT to T causes an error CS0102 at the line /*💀*/
        public sealed class U<NotT>
        {
            /*💀*/ public int T { get; set; }
        }
    }

    public sealed class C5_Event
    {
        // Renaming NotT to T causes an error CS0102 at the line /*💀*/
        public sealed class U<NotT>
        {
            /*💀*/ public event EventHandler? T;

            public void OnEvent(EventArgs e)
            {
                // The next line causes an error CS0119 after renaming NotT to T.
                T?.Invoke(this, e);
            }
        }
    }

    public sealed class C5_Delegate
    {
        // Renaming NotT to T causes an error CS0102 at the line /*💀*/
        public sealed class U<NotT>
        {
            /*💀*/ public delegate int T(int x, int y);
        }
    }

    public sealed class C5_Type
    {
        // Renaming NotT to T causes an error CS0102 at the line /*💀*/
        public sealed class U<NotT>
        {
            /*💀*/ public struct T
            {
            }
        }
    }

    public sealed class C6
    {
        public sealed class Inner
        {
            // Renaming NotT to T causes a change the meaning of T in U<NotT>: C6.T -> T in U<T>
            public static class U<NotT>
            {
                public static T? Default { get; }
            }
        }

        public sealed class T
        {
        }
    }
}
