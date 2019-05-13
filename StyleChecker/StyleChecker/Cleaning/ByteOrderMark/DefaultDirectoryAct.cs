namespace StyleChecker.Cleaning.ByteOrderMark
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The default implementation of <see cref="DirectoryAct"/> interface.
    /// </summary>
    public sealed class DefaultDirectoryAct : DirectoryAct
    {
        private readonly DirectoryInfo impl;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDirectoryAct"/>
        /// class.
        /// </summary>
        /// <param name="path">
        /// The path on which to create the <see cref="DirectoryAct"/>.
        /// </param>
        public DefaultDirectoryAct(string path)
            => impl = new DirectoryInfo(path);

        private DefaultDirectoryAct(DirectoryInfo info)
            => impl = info;

        /// <inheritdoc/>
        public string Name
            => impl.Name;

        /// <inheritdoc/>
        public FileAttributes Attributes
            => impl.Attributes;

        /// <inheritdoc/>
        public IEnumerable<DirectoryAct> GetDirectories()
        {
            return impl.EnumerateDirectories()
                .Select(d => new DefaultDirectoryAct(d));
        }

        /// <inheritdoc/>
        public IEnumerable<FileAct> GetFiles()
        {
            return impl.EnumerateFiles()
                .Select(f => new DefaultFileAct(f));
        }
    }
}
