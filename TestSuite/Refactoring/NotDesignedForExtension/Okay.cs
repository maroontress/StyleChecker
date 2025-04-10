namespace StyleChecker.Test.Refactoring.NotDesignedForExtension
{
    using System;

    public abstract class Code
    {
        public abstract int Property { get; }

        public abstract void Go();

        public virtual void Method()
        {
        }
    }

    public class ChildCode : Code
    {
        public sealed override int Property => 1;

        public sealed override void Go()
        {
            Console.WriteLine($"{nameof(ChildCode)}.{nameof(Go)}");
        }

        public sealed override void Method()
        {
            Console.WriteLine($"{nameof(ChildCode)}.{nameof(Method)}");
        }
    }

    public sealed class SealedChildCode : Code
    {
        public override int Property => 1;

        public override void Go()
        {
            Console.WriteLine($"{nameof(ChildCode)}.{nameof(Go)}");
        }

        public override void Method()
        {
            Console.WriteLine($"{nameof(ChildCode)}.{nameof(Method)}");
        }
    }
}
