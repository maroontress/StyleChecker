namespace Maroontress.Oxbind.Impl
{
    using System.Xml;

    /// <summary>
    /// The default implementation of <see cref="IXmlLineInfo"/> interface.
    /// </summary>
    public sealed class DefaultXmlLineInfo : IXmlLineInfo
    {
        private readonly bool hasLineInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultXmlLineInfo"/>
        /// class with the specified <see cref="IXmlLineInfo"/>.
        /// </summary>
        /// <param name="info">
        /// The location information to be copied.
        /// </param>
        public DefaultXmlLineInfo(IXmlLineInfo info)
        {
            LineNumber = info.LineNumber;
            LinePosition = info.LinePosition;
            hasLineInfo = info.HasLineInfo();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultXmlLineInfo"/>
        /// class.
        /// </summary>
        private DefaultXmlLineInfo()
        {
            LineNumber = 0;
            LinePosition = 0;
            hasLineInfo = false;
        }

        /// <summary>
        /// Gets the <see cref="IXmlLineInfo"/> representing no line
        /// information.
        /// </summary>
        public static IXmlLineInfo NoLineInfo { get; }
            = new DefaultXmlLineInfo();

        /// <inheritdoc/>
        public int LineNumber { get; }

        /// <inheritdoc/>
        public int LinePosition { get; }

        /// <inheritdoc/>
        public bool HasLineInfo() => hasLineInfo;
    }
}
