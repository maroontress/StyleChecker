namespace StyleChecker.Test.Naming.ThoughtlessName
{
    using System;
    using System.IO;
    using System.Text;

    public sealed class Okay
    {
        public void OK()
        {
            var b = new StringBuilder();
            var stream = new MemoryStream();
        }

        public void ForEach()
        {
            var all = new[] { "a", "b", "c", };
            foreach (var e in all)
            {
                Console.WriteLine(e);
            }
        }

        public void Catch()
        {
            try
            {
                Console.WriteLine("a");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                Console.WriteLine("a");
            }
            catch (Exception)
            {
                Console.WriteLine("Exception thrown");
            }
        }
    }
}
