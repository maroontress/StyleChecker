namespace Maroontress.Oxbind
{
    using System;

    /// <summary>
    /// Indicates that an error has occurred while creating a new instance with
    /// the XML reader.
    /// </summary>
    public sealed class BindingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingException"/>
        /// class.
        /// </summary>
        public BindingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingException"/>
        /// class, with the specified detail message.
        /// </summary>
        /// <param name="message">The detail message.</param>
        public BindingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingException"/>
        /// class, with the specified detail message and cause.
        /// </summary>
        /// <param name="message">The detail message.</param>
        /// <param name="innerException">The cause.</param>
        public BindingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
