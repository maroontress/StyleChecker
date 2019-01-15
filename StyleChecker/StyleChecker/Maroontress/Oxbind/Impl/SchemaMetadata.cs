namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
        /// The immutable map that wraps a <see cref="ChildReflectorMap"/>
        /// object.
        /// </summary>
        private readonly IReadOnlyDictionary<Type, Reflector<object>>
            childReflectorMap;

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
            childReflectorMap = ChildReflectorMap.Of(clazz);
        }

        /// <inheritdoc/>
        protected override void HandleComponentsWithContent(
            object instance,
            XmlReader @in,
            Func<Type, Metadata> getMetadata)
        {
            HandleAction((t, r) => t.ApplyWithContent(
                @in, getMetadata, r, o => r.Inject(instance, o)));
        }

        /// <inheritdoc/>
        protected override void HandleComponentsWithEmptyElement(
            object instance,
            XmlReader @in,
            Func<Type, Metadata> getMetadata)
        {
            HandleAction((t, r) => t.ApplyWithEmptyElement(
                @in, getMetadata, r, o => r.Inject(instance, o)));
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
            var fieldTypeInfo = field.FieldType.GetTypeInfo();
            var valueTypeInfo = typeof(T).GetTypeInfo();
            Debug.Assert(
                fieldTypeInfo.IsAssignableFrom(valueTypeInfo),
                $"{typeof(T).FullName}");
            return field.GetValue(null) as T;
        }

        private void HandleAction(Action<SchemaType, Reflector<object>> action)
        {
            foreach (var t in schema.Types())
            {
                action(t, childReflectorMap[t.PlaceholderType]);
            }
        }
    }
}
