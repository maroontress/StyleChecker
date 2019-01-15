namespace Maroontress.Oxbind
{
    using System;
    using System.Xml;
    using Maroontress.Oxbind.Impl;
    using StyleChecker.Annotations;

    /// <summary>
    /// The factory of the <see cref="SchemaType"/> object that represents
    /// the child element that occurs once or not at all.
    /// </summary>
    public static class Optional
    {
        /// <summary>
        /// Creates the <see cref="SchemaType"/> object that represents
        /// the child element corresponding to the specified class
        /// that occurs once or not at all.
        /// </summary>
        /// <remarks>
        /// The <see cref="SchemaType"/> object for the specified class
        /// that this method returns
        /// must not follow consecutively one that
        /// <see cref="Multiple.Of{T}()"/>
        /// with the same class returns,
        /// in the parameters of the
        /// <see cref="Schema.Of(SchemaType[])"/>.
        /// For example,
        /// <c>Schema.Of(Multiple.Of&lt;Movie&gt;(),
        /// Optional.Of&lt;Movie&gt;())</c> is invalid.
        /// </remarks>
        /// <typeparam name="T">
        /// The class annotated with <see cref="ForElementAttribute"/>,
        /// representing the child element that occurs once or not at all.
        /// </typeparam>
        /// <returns>
        /// The <see cref="SchemaType"/> object.
        /// </returns>
        public static SchemaType Of<T>()
        {
            return new OptionalSchemaType(typeof(T));
        }

        private sealed class OptionalSchemaType : SchemaType
        {
            public OptionalSchemaType(Type type)
                : base(type, false, false)
            {
            }

            /// <inheritdoc/>
            public override void ApplyWithContent(
                XmlReader input,
                Func<Type, Metadata> getMetadata,
                Reflector<object> reflector,
                Action<object> setChildValue)
            {
                var m = getMetadata(ElementType);
                var nodeType = Readers.SkipCharacters(input);
                if (nodeType != XmlNodeType.Element)
                {
                    return;
                }
                if (!Readers.Equals(input, m.ElementName))
                {
                    return;
                }
                var info = Readers.ToXmlLineInfo(input);
                var child = m.CreateInstance(input, getMetadata);
                setChildValue(reflector.Sugarcoater(info, child));
            }

            public override void ApplyWithEmptyElement(
                [Unused] XmlReader input,
                [Unused] Func<Type, Metadata> getMetadata,
                [Unused] Reflector<object> reflector,
                [Unused] Action<object> setChildValue)
            {
            }
        }
    }
}
