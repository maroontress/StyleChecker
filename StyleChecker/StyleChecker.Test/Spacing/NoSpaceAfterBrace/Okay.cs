namespace Application
{
    using System;

    public sealed class Okay
    {
        public void Basic()
        {
            if (true)
            {
            }
            var array = new[] { "" };
            var array2 = new[] {
                "" };
            var array3 = new[]
            {
                "",
            };
            var array4 = new[] { new[] { "" }, };
            Action doNothing = () => {};
            if (array is { Length: 0 } z)
            {
            }
            if (array is {} e)
            {
            }

            static void M(string[] a)
            {
            }

            M(new[] { "" });

            _ = new[] { "" }.ToString();
            _ = new[] { "" }?.ToString();
            _ = new[] { "" }!;
            _ = new[] { "" }[0];
        }
    }
}
