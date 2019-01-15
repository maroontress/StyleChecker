namespace Maroontress.Oxbind
{
    using System;

    /// <summary>
    /// Marks an instance method to be notified with the text inside the XML
    /// element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="Oxbinder{T}"/> finds the XML elements containing a
    /// text node, it invokes the annotated method with the value that the text
    /// node contains.
    /// </para>
    /// <para>
    /// This annotation must mark an instance method, of which the return type
    /// must be <c>void</c>, and that must have a single parameter whose type
    /// is <see cref="string"/>. And the class that has the instance method
    /// must be annotated with <see cref="ForElementAttribute"/>.
    /// </para>
    /// <para>
    /// If there is an instance method annotated with <see
    /// cref="FromTextAttribute"/> in a class, the class must not have other
    /// instance methods annotated with it, instance fields annotated with <see
    /// cref="ForTextAttribute"/>, and <c>static</c> and <c>final</c> fields
    /// annotated with <see cref="ElementSchemaAttribute"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Schema"/>
    /// <seealso cref="ForElementAttribute"/>
    /// <seealso cref="ElementSchemaAttribute"/>
    [AttributeUsage(
        AttributeTargets.Method,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class FromTextAttribute : Attribute
    {
    }
}
