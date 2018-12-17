namespace Maroontress.Oxbind
{
    using System;

    /// <summary>
    /// Marks a class to be bound with the XML element.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class ForElementAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForElementAttribute"/>
        /// class.
        /// </summary>
        /// <param name="name">The element name.</param>
        public ForElementAttribute(string name) => Name = name;

        /// <summary>
        /// Gets the element name.
        /// </summary>
        public string Name { get; private set; }
    }
}
