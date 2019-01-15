namespace Maroontress.Oxbind
{
    using System;
    using System.Xml;

    /// <summary>
    /// Marks an instance method to be notified with the XML attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="Oxbinder{T}"/> finds the XML attribute, it invokes
    /// the annotated method corresponding to the attribute name, with that
    /// value as a single parameter. This annotation must mark an instance
    /// method, of which the return type must be <c>void</c>, and that must
    /// have a single parameter whose type is <see cref="string"/>. And the
    /// class that has the instance method must be annotated with <see
    /// cref="ForElementAttribute"/>.
    /// </para>
    /// <para>
    /// Each attribute name of the annotation <see
    /// cref="ForAttributeAttribute"/> and <see cref="FromAttributeAttribute"/>
    /// must be unique in one class. For example, in a class, if an instance
    /// method is annotated with <c>[FromAttribute("name")]</c>, there must be
    /// no other methods annotated with <c>[FromAttribute("name")]</c> and also
    /// be no fields annotated with <c>[ForAttribute("name")]</c>.
    /// </para>
    /// </remarks>
    /// <seealso cref="ForElementAttribute"/>
    /// <seealso cref="ForAttributeAttribute"/>
    [AttributeUsage(
        AttributeTargets.Method,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class FromAttributeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="FromAttributeAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The attribute name.
        /// </param>
        /// <param name="ns">
        /// The namespace URI.
        /// </param>
        public FromAttributeAttribute(string name, string ns = "")
            => QName = new XmlQualifiedName(name, ns);

        /// <summary>
        /// Gets the qualified name of the attribute.
        /// </summary>
        public XmlQualifiedName QName { get; }
    }
}
