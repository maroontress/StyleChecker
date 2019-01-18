namespace Maroontress.Oxbind
{
    using System;

    /// <summary>
    /// Marks an instance method to be notified with the XML element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="Oxbinder{T}"/> finds the XML child element, it
    /// creates the object bound to the element, and invokes the annotated
    /// method corresponding to the child element, with that object as a single
    /// parameter.
    /// </para>
    /// <para>
    /// This annotation must mark an instance method, of which the return type
    /// must be <c>void</c>, and that must have a single
    /// parameter.  The type of the parameter must be the class annotated
    /// with <see cref="ForElementAttribute"/> and also be one of the classes
    /// included in the <see cref="Schema"/> object, which is the value of the
    /// field annotated with <see cref="ElementSchemaAttribute"/> in that
    /// class. And the class that has the instance method must be annotated
    /// with <see cref="ForElementAttribute"/>.
    /// </para>
    /// <para>
    /// Each class of the instance field marked with the
    /// <see cref="ForChildAttribute"/>
    /// and of the single parameter of the method marked with <see
    /// cref="FromChildAttribute"/> must be unique in one class. For example,
    /// in a class, if an instance method that returns <c>void</c> and has the
    /// single parameter whose type is the <c>Director</c> class is annotated
    /// with <see cref="FromChildAttribute"/>, there must be no other methods
    /// annotated with <see cref="FromChildAttribute"/> of which the type of
    /// the single parameter is <c>Director</c> and also be no fields annotated
    /// with <see cref="ForChildAttribute"/> whose type is the <c>Director</c>
    /// class.
    /// </para>
    /// </remarks>
    /// <seealso cref="ForChildAttribute"/>
    /// <seealso cref="ForElementAttribute"/>
    /// <seealso cref="ElementSchemaAttribute"/>
    /// <seealso cref="Schema"/>
    [AttributeUsage(
        AttributeTargets.Method,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class FromChildAttribute : Attribute
    {
    }
}
