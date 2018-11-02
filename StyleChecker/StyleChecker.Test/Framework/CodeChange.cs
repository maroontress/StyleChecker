namespace StyleChecker.Test.Framework
{
    /// <summary>
    /// Represents two codes.
    /// </summary>
    public sealed class CodeChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeChange"/> class.
        /// </summary>
        /// <param name="before">The code before change.</param>
        /// <param name="after">The code after change.</param>
        public CodeChange(string before, string after)
        {
            Before = before;
            After = after;
        }

        /// <summary>
        /// Gets the code before change.
        /// </summary>
        public string Before { get; private set; }

        /// <summary>
        /// Gets the code after change.
        /// </summary>
        public string After { get; private set; }
    }
}
