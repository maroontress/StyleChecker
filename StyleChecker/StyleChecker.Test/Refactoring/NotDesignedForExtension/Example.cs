namespace StyleChecker.Test.Refactoring.NotDesignedForExtension
{
    public sealed class Example
    {
        public class BaseClass
        {
            // A virtual method must be empty or be changed to be abstract.
            public virtual void Method()
            //@                 ^m,Method
            {
                DoSomething();
            }

            private void DoSomething()
            {
            }
        }

        public class DerivedClass : BaseClass
        {
            // An overriding method must be sealed or empty.
            public override void Method()
            //@                  ^m,Method
            {
                PerformAnotherAction();
            }

            private void PerformAnotherAction()
            {
            }
        }
    }
}
