namespace Maroontress.Oxbind
{
    using System;

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindException"/>
        /// class, with the specified detail message.
        /// </summary>
        /// <param name="message">The detail message.</param>
        public BindException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindException"/>
        /// class, with the specified detail message and cause.
        /// </summary>
        /// <param name="message">The detail message.</param>
        /// <param name="innerException">The cause.</param>
        public BindException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
