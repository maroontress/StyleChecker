namespace Maroontress.Oxbind
{
    /// <summary>
    /// The event of the desereialized value.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value.
    /// </typeparam>
    public interface BindEvent<T>
    {
        /// <summary>
        /// Gets the deserialized value.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Gets the line number where this event occurs in the XML docuemnt.
        /// </summary>
        int Line { get; }

        /// <summary>
        /// Gets the column number where this event occurs in the XML document.
        /// </summary>
        int Column { get; }
    }
}
