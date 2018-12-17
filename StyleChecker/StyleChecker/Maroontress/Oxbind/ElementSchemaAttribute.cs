namespace Maroontress.Oxbind
{
    using System;

    /// <summary>
    /// Marks a <c>static</c> field that defines the schema of child elements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This annotation must mark a <c>static</c> and <c>readonly</c> field
    /// whose type is <see cref="Schema"/>. And the class that has the
    /// <c>static</c> field must be annotated with <see
    /// cref="ForElementAttribute"/>.
    /// </para>
    /// <para>
    /// If there is a <c>static</c> field annotated with <see
    /// cref="ElementSchemaAttribute"/> in a class, there must be no other
    /// <c>static</c> fields annotated with it, and be no instance fields
    /// annotated with <see cref="ForTextAttribute"/> in that class.
    /// </para>
    /// </remarks>
    /// <seealso cref="Schema"/>
    /// <seealso cref="ForTextAttribute"/>
    /// <seealso cref="ForElementAttribute"/>
    [AttributeUsage(
        AttributeTargets.Field,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class ElementSchemaAttribute : Attribute
    {
    }
}
