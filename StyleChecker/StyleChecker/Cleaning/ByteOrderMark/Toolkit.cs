namespace StyleChecker.Cleaning.ByteOrderMark
{
    /// <summary>
    /// Provides abstraction of <see cref="System.IO.DirectoryInfo"/> and <see
    /// cref="System.IO.FileInfo"/> classes.
    /// </summary>
    public abstract class Toolkit
    {
        /// <summary>
        /// Gets or sets the singleton instance.
        /// </summary>
        public static Toolkit TheInstance { get; set; }
            = new DefaultToolkit();

        /// <summary>
        /// Gets a new <see cref="DirectoryAct"/> of the specified path.
        /// </summary>
        /// <param name="path">
        /// The path on which to create the <see cref="DirectoryAct"/>.
        /// </param>
        /// <returns>
        /// The new <see cref="DirectoryAct"/> object.
        /// </returns>
        public abstract DirectoryAct GetDirectoryAct(string path);
    }
}
