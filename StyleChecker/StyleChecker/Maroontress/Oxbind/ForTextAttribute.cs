namespace Maroontress.Oxbind
{
    using System;

    /// <summary>
    /// Marks an instance field to be bound with the text inside the XML
    /// element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="Oxbinder{T}"/> finds the XML elements containing a
    /// text node, it populate the annotated field with the value that the text
    /// node contains.
    /// </para>
    /// <para>
    /// This annotation must mark an instance field whose type is <see
    /// cref="string"/>. And the class that has the instance field must be
    /// annotated with <see cref="ForElementAttribute"/>.
    /// </para>
    /// <para>
    /// If there is an instance field annotated with <see
    /// cref="ForTextAttribute"/> in a class, the class must not have other
    /// instance fields annotated with it, instance methods annotated with <see
    /// cref="FromTextAttribute"/>, and be no <c>static</c> and <c>final</c>
    /// fields annotated with <see cref="ElementSchemaAttribute"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Schema"/>
    /// <seealso cref="ForElementAttribute"/>
    /// <seealso cref="ElementSchemaAttribute"/>
    [AttributeUsage(
        AttributeTargets.Field,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class ForTextAttribute : Attribute
    {
    }
}
