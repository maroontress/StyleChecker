namespace Maroontress.Oxbind.Impl
{
    using System.Xml;

    /// <summary>
    /// The default implementation of the <see cref="BindEvent{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the deserialized event.
    /// </typeparam>
    public class BindEventImpl<T> : BindEvent<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindEventImpl{T}"/>
        /// class.
        /// </summary>
        /// <param name="value">
        /// The deserialized value.
        /// </param>
        /// <param name="info">
        /// The location information of the XML document.
        /// </param>
        public BindEventImpl(T value, IXmlLineInfo info)
        {
            Value = value;
            Line = info.LineNumber;
            Column = info.LinePosition;
        }

        /// <inheritdoc/>
        public T Value { get; }

        /// <inheritdoc/>
        public int Line { get; }

        /// <inheritdoc/>
        public int Column { get; }
    }
}
