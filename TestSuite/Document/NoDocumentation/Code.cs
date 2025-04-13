namespace Application
{
    public sealed class Code<T>
    //@                 ^Code
    {
        public Code(int size)
        //@    ^Code
        {
            Field = new T[size];
        }

        ~Code()
     //@ ^Code
        {
        }

        public T[] Field;
        //@        ^Field

        public int Property { get; set; }
        //@        ^Property

        public T this[int index]
        //@      ^this
        {
            get
            {
                return Field[index];
            }
            set
            {
                Field[index] = value;
            }
        }

        public void Method(string m)
        //@         ^Method
        {
            var handler = Handler;

            if (handler != null)
            {
                handler(this, m);
            }
        }

        public override bool Equals(object o)
        //@                  ^Equals
        {
            return false;
        }

        public override int GetHashCode()
        //@                 ^GetHashCode
        {
            return 0;
        }

        public static bool operator ==(Code<T> a, Code<T> b)
        //@                         ^==
        {
            return false;
        }

        public static bool operator !=(Code<T> a, Code<T> b)
        //@                         ^!=
        {
            return false;
        }

        public static explicit operator Code<T>(int i)
        //@           ^explicit
        {
            return new Code<T>(i);
        }

        public static implicit operator string[](Code<T> c)
        //@           ^implicit
        {
            return new string[c.Field.Length];
        }

        public interface ICode
        //@              ^ICode
        {
        }

        public struct Struct
        //@           ^Struct
        {
        }

        public enum Enum
        //@         ^Enum
        {
            Constant
        //@ ^Constant
        }

        public delegate int Delegate(int value);
        //@                 ^Delegate

        public event System.EventHandler<string> CustomHandler
        //@                                      ^CustomHandler
        {
            add
            {
            }
            remove
            {
            }
        }

        public event System.EventHandler<string> Handler;
        //@                                      ^Handler
    }
}
