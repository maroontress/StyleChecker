namespace Maroontress.Oxbind
{
    using System.IO;

    /// <summary>
    /// Provides a way to create new instances from text readers.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the instance to create.
    /// </typeparam>
    public interface Oxbinder<T>
    {
        /// <summary>
        /// Creates a new instance from the specified text reader.
        /// </summary>
        /// <remarks>
        /// This method throws <see cref="BindingException" /> if there are
        /// invalid annotations in the class representing the XML root element
        /// (that was specified with <see cref="OxbinderFactory.Of()" /> when
        /// this <c>Oxbinder</c> was created), or in the classes representing
        /// the descendants of that root element.
        /// </remarks>
        /// <param name="reader">
        /// The text reader that provides the XML stream.
        /// </param>
        /// <returns>
        /// A new instance.
        /// </returns>
        T NewInstance(TextReader reader);
    }
}
