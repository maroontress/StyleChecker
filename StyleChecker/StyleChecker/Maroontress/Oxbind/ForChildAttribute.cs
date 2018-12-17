namespace Maroontress.Oxbind
{
    using System;

    /// <summary>
    /// Marks an instance field to be bound with the XML element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="Oxbinder{T}"/> finds the XML child element, it
    /// creates the object bound to the element, and then populates the
    /// annotated field with the object.
    /// </para>
    /// <para>
    /// This annotation must mark an instance field. And the class that has the
    /// instance field must be annotated with <see
    /// cref="ForElementAttribute"/>.
    /// </para>
    /// <para>
    /// The type of the field must be the class annotated with <see
    /// cref="ForElementAttribute"/> and also be one of the classes included in
    /// the <see cref="Schema"/> object, which is the value of the field
    /// annotated with <see cref="ElementSchemaAttribute"/> in that class.
    /// </para>
    /// <para>
    /// Each class of the instance field marked with the <see
    /// cref="ForChildAttribute"/> and of the single parameter of the method
    /// marked with <see cref="FromChildAttribute"/> must be unique in one
    /// class. For example, in a class, if an instance field whose type is the
    /// <c>Director</c> class is annotated with <see
    /// cref="ForChildAttribute"/>, there must be no other fields annotated
    /// with
    /// <see cref="ForChildAttribute"/>
    /// whose type is the <c>Director</c> class and also be no methods
    /// annotated with <see cref="FromChildAttribute"/> of which the type of
    /// the single parameter is <c>Director</c>.
    /// </para>
    /// </remarks>
    /// <seealso cref="FromChildAttribute"/>
    /// <seealso cref="ForElementAttribute"/>
    /// <seealso cref="ElementSchemaAttribute"/>
    /// <seealso cref="Schema"/>
    [AttributeUsage(
        AttributeTargets.Field,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class ForChildAttribute : Attribute
    {
    }
}
