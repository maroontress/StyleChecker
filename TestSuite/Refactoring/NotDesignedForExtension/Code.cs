namespace StyleChecker.Test.Refactoring.NotDesignedForExtension
{
    public abstract class Code
    {
        public abstract int Property { get; }

        public virtual int Value => 0;
        //@                ^p,Value

        public abstract void Go();

        public virtual int Method()
        //@                ^m,Method
        {
            return 0;
        }
    }

    public class ChildCode : Code
    {
        public override int Property => 1;
        //@                 ^p,Property

        public override int Value => 2;
        //@                 ^p,Value

        public override void Go()
        //@                  ^m,Go
        {
            Method();
        }

        public override int Method()
        //@                 ^m,Method
        {
            return 1;
        }
    }
}
