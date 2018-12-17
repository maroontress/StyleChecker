namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using Maroontress.Util;

    /// <summary>
    /// Metadata of the classes that have a <c>static</c> and <c>readonly</c>
    /// <see cref="Schema"/> field annotated with the <see
    /// cref="ElementSchemaAttribute"/>.
    /// </summary>
    public sealed class SchemaMetadata : Metadata
    {
        /// <summary>
        /// The <see cref="Schema"/> object.
        /// </summary>
        private readonly Schema schema;

        /// <summary>
        /// The immutable map that wraps a {@code ChildTaskMap} object.
        /// </summary>
        private readonly IReadOnlyDictionary<Type, Task<object>> childTaskMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaMetadata"/>
        /// class.
        /// </summary>
        /// <param name="clazz">
        /// The class annotated with <see cref="ForElementAttribute"/>.
        /// </param>
        public SchemaMetadata(Type clazz)
            : base(clazz)
        {
            schema = SchemaOf(clazz);
            childTaskMap = ChildTaskMap.Of(clazz);
        }

        /// <summary>
        /// Dispatches the specified value to the specified instance.
        /// </summary>
        /// <param name="instance">
        /// The instance that receives the value.
        /// </param>
        /// <param name="placeholderType">
        /// The placeholder type bound to the class of the value.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void DispatchChild(
            object instance,
            Type placeholderType,
            object value)
        {
            var t = childTaskMap[placeholderType];
            if (t == null)
            {
                return;
            }
            t(instance, value);
        }

        /// <inheritdoc/>
        protected override void HandleComponents(
            object instance,
            XmlReader @in,
            Func<Type, Metadata> getMetadata)
        {
            foreach (var t in schema.Types())
            {
                t.Apply(this, instance, @in, getMetadata);
            }
        }

        /// <summary>
        /// Returns the <see cref="Schema"/> object of the specified class.
        /// </summary>
        /// <param name="clazz">
        /// The class containing the <c>static</c> and <c>readonly</c>
        /// <see cref="Schema"/> fields annotated with
        /// <see cref="ElementSchemaAttribute"/>.
        /// </param>
        /// <returns>
        /// The <see cref="ElementSchemaAttribute"/> object of the first
        /// <see cref="Schema"/> field,
        /// or <see cref="Schema.Empty"/>
        /// if the class contains no <see cref="Schema"/> fields.
        /// </returns>
        private static Schema SchemaOf(Type clazz)
        {
            return Classes.GetStaticFields<ElementSchemaAttribute>(clazz)
                .Select(f => ValueOf<Schema>(f))
                .FirstOrDefault() ?? Schema.Empty;
        }

        /// <summary>
        /// Returns the value of the specified <c>static</c> field.
        /// </summary>
        /// <typeparam name="T">
        /// The class of the value that <paramref name="field"/> has.
        /// </typeparam>
        /// <param name="field">
        /// The <see cref="FieldInfo"/> object representing a <c>static</c>
        /// field.
        /// </param>
        /// <returns>
        /// The value of the <paramref name="field"/>.
        /// </returns>
        private static T ValueOf<T>(FieldInfo field)
            where T : class
        {
            if (!field.FieldType.GetTypeInfo()
                .IsAssignableFrom(typeof(T).GetTypeInfo()))
            {
                throw new BindingException(typeof(T).FullName);
            }
            return field.GetValue(null) as T;
        }
    }
}
