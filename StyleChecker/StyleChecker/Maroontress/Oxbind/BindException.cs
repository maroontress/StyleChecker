namespace Maroontress.Oxbind
{
    using System;
    using System.Xml;
    using Maroontress.Oxbind.Impl;

    /// <summary>
    /// Indicates that an error has occurred while creating a new instance with
    /// the XML reader.
    /// </summary>
    public sealed class BindException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindException"/>
        /// class.
        /// </summary>
        public BindException()
        {
            LineInfo = DefaultXmlLineInfo.NoLineInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindException"/>
        /// class, with the specified detail message.
        /// </summary>
        /// <param name="message">
        /// The detail message.
        /// </param>
        public BindException(string message)
            : base(message)
        {
            LineInfo = DefaultXmlLineInfo.NoLineInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindException"/>
        /// class, with the specified detail message and cause.
        /// </summary>
        /// <param name="message">
        /// The detail message.
        /// </param>
        /// <param name="innerException">
        /// The cause.
        /// </param>
        public BindException(string message, Exception innerException)
            : base(message, innerException)
        {
            LineInfo = DefaultXmlLineInfo.NoLineInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindException"/>
        /// class, with the specified detail message and the specified
        /// location.
        /// </summary>
        /// <param name="message">
        /// The detail message.
        /// </param>
        /// <param name="info">
        /// The line information.
        /// </param>
        public BindException(string message, IXmlLineInfo info)
            : base(message)
        {
            LineInfo = new DefaultXmlLineInfo(info);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindException"/>
        /// class, with the specified detail message and cause.
        /// </summary>
        /// <param name="message">
        /// The detail message.
        /// </param>
        /// <param name="innerException">
        /// The cause.
        /// </param>
        /// <param name="info">
        /// The line information.
        /// </param>
        public BindException(
            string message, Exception innerException, IXmlLineInfo info)
            : base(message, innerException)
        {
            LineInfo = new DefaultXmlLineInfo(info);
        }

        /// <summary>
        /// Gets the line information.
        /// </summary>
        public IXmlLineInfo LineInfo { get; }

        /// <summary>
        /// Gets the message with the location information.
        /// </summary>
        /// <remarks>
        /// This method returns the string with the form
        /// <c>"</c>Line<c>:</c>Column<c>: </c>Message<c>"</c>
        /// if the <see cref="LineInfo"/> has the line information (i.e.
        /// <see cref="IXmlLineInfo.HasLineInfo"/> returns <c>true</c>),
        /// otherwise returns the same as <see cref="Exception.Message"/>.
        /// </remarks>
        /// <returns>
        /// The string representation.
        /// </returns>
        public string GetFullMessage()
        {
            var info = LineInfo;
            return !info.HasLineInfo()
                ? Message
                : $"{info.LineNumber}:{info.LinePosition}: {Message}";
        }
    }
}
