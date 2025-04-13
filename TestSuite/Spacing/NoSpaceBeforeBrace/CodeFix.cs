#pragma warning disable CS0162

namespace Application
{
    using System;

    public sealed class Code
    {
        public void NG()
        {
            string[] array = { "" };

            Action doNothing = () => { return; };

            if (array is { Length: 0 } z)
            {
            }

            if (true) { return; } else {}

            do {} while (false);

            ; {}
        }
    }
}
