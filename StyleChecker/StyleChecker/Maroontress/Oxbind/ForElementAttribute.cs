namespace Maroontress.Oxbind
{
    using System;
    using System.Xml;

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
        /// <param name="name">
        /// The element name.
        /// </param>
        /// <param name="ns">
        /// The namespace URI.
        /// </param>
        public ForElementAttribute(string name, string ns = "")
            => QName = new XmlQualifiedName(name, ns);

        /// <summary>
        /// Gets the qualified name of the element.
        /// </summary>
        public XmlQualifiedName QName { get; }
    }
}
