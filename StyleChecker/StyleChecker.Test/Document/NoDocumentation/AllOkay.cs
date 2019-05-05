namespace Application
{
    [AllOkay]
    public sealed class Code<T>
    {
        public Code(int size)
        {
            Field = new T[size];
        }

        ~Code()
        {
        }

        public T[] Field;

        public int Property { get; set; }

        public T this[int index]
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
        {
            var handler = Handler;

            if (handler != null)
            {
                handler(this, m);
            }
        }

        public override bool Equals(object o)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Code<T> a, Code<T> b)
        {
            return false;
        }

        public static bool operator !=(Code<T> a, Code<T> b)
        {
            return false;
        }

        public static explicit operator Code<T>(int i)
        {
            return new Code<T>(i);
        }

        public static implicit operator string[] (Code<T> c)
        {
            return new string[c.Field.Length];
        }

        public interface ICode
        {
        }

        public struct Struct
        {
        }

        public enum Enum
        {
            Constant
        }

        public delegate int Delegate(int value);

        public event System.EventHandler<string> CustomHandler
        {
            add
            {
            }
            remove
            {
            }
        }

        public event System.EventHandler<string> Handler;
    }

    /// <summary>
    /// Allows no documentation comment.
    /// </summary>
    public sealed class AllOkayAttribute : System.Attribute
    {
    }
}
