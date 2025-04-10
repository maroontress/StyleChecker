namespace Application
{
    [Okay]
    public sealed class Code<T>
    {
        [Okay]
        public Code(int size)
        {
            Field = new T[size];
        }

        [Okay]
        ~Code()
        {
        }

        [Okay]
        public T[] Field;

        [Okay]
        public int Property { get; set; }

        [Okay]
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

        [Okay]
        public void Method(string m)
        {
            var handler = Handler;

            if (handler != null)
            {
                handler(this, m);
            }
        }

        [Okay]
        public override bool Equals(object o)
        {
            return false;
        }

        [Okay]
        public override int GetHashCode()
        {
            return 0;
        }

        [Okay]
        public static bool operator ==(Code<T> a, Code<T> b)
        {
            return false;
        }

        [Okay]
        public static bool operator !=(Code<T> a, Code<T> b)
        {
            return false;
        }

        [Okay]
        public static explicit operator Code<T>(int i)
        {
            return new Code<T>(i);
        }

        [Okay]
        public static implicit operator string[] (Code<T> c)
        {
            return new string[c.Field.Length];
        }

        [Okay]
        public interface ICode
        {
        }

        [Okay]
        public struct Struct
        {
        }

        [Okay]
        public enum Enum
        {
            [Okay]
            Constant
        }

        [Okay]
        public delegate int Delegate(int value);

        [Okay]
        public event System.EventHandler<string> CustomHandler
        {
            add
            {
            }
            remove
            {
            }
        }

        [Okay]
        public event System.EventHandler<string> Handler;
    }

    /// <summary>
    /// Allows no documentation comment.
    /// </summary>
    public sealed class OkayAttribute : System.Attribute
    {
    }
}
