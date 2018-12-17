namespace Maroontress.Oxbind
{
    using System;

    /// <summary>
    /// Marks an instance field to be bound with the XML attribute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="Oxbinder{T}"/> finds the XML attribute, it populates
    /// the annotated field corresponding to the attribute name, with that
    /// value.
    /// </para>
    /// <para>
    /// This annotation must mark an instance field whose type is <see
    /// cref="string"/>. And the class that has the instance field must be
    /// annotated with <see cref="ForElementAttribute"/>.
    /// </para>
    /// <para>
    /// Each attribute name of the annotation <see
    /// cref="ForAttributeAttribute"/> and <see cref="FromAttributeAttribute"/>
    /// must be unique in one class.
    /// For example, in a class, if an instance field is annotated with
    /// <c>[ForAttribute("name")]</c>, there must be no other fields annotated
    /// with <c>[ForAttribute("name")]</c> and also be no methods annotated
    /// with <c>[FromAttribute("name")]</c>.
    /// </para>
    /// </remarks>
    /// <seealso cref="FromAttributeAttribute"/>
    /// <seealso cref="ForElementAttribute"/>
    [AttributeUsage(
        AttributeTargets.Field,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class ForAttributeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="ForAttributeAttribute"/> class.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        public ForAttributeAttribute(string name) => Name = name;

        /// <summary>
        /// Gets the attribute name.
        /// </summary>
        public string Name { get; }
    }
}
