namespace Maroontress.Oxbind
{
    using System;
    using System.Xml;
    using Maroontress.Oxbind.Impl;
    using StyleChecker.Annotations;

    /// <summary>
    /// The factory of the <see cref="SchemaType"/> object that represents
    /// the child element that occurs exactly one times.
    /// </summary>
    public static class Mandatory
    {
        /// <summary>
        /// Creates the <see cref="SchemaType"/> object that represents
        /// the child element corresponding to the specified class
        /// that occurs exactly one times.
        /// </summary>
        /// <remarks>
        /// The <see cref="SchemaType"/> object for the specified class
        /// that this method returns
        /// must not follow consecutively one that
        /// <see cref="Multiple.Of{T}()"/> or
        /// <see cref="Multiple.Of{T}()"/> with the same class returns,
        /// in the parameters of the
        /// <see cref="Schema.Of(SchemaType[])"/>.
        /// For example, both <c>Schema.Of(Optional.Of&lt;Movie&gt;(),
        /// Mandatory.Of&lt;Movie&gt;())</c> and
        /// <c>Schema.Of(Multiple.Of&lt;Movie&gt;(),
        /// Mandatory.Of&lt;Movie&gt;())</c> are invalid.
        /// </remarks>
        /// <typeparam name="T">
        /// The class annotated with <see cref="ForElementAttribute"/>,
        /// representing the child element that occurs exactly one times.
        /// </typeparam>
        /// <returns>
        /// The <see cref="SchemaType"/> object.
        /// </returns>
        public static SchemaType Of<T>()
        {
            return new MandatorySchemaType(typeof(T));
        }

        private sealed class MandatorySchemaType : SchemaType
        {
            public MandatorySchemaType(Type type)
                : base(type, true, false)
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
                _ = Readers.SkipCharacters(input);
                Readers.ConfirmStartElement(input, m.ElementName);
                var info = Readers.ToXmlLineInfo(input);
                var child = m.CreateInstance(input, getMetadata);
                setChildValue(reflector.Sugarcoater(info, child));
            }

            /// <inheritdoc/>
            public override void ApplyWithEmptyElement(
                XmlReader input,
                Func<Type, Metadata> getMetadata,
                [Unused] Reflector<object> reflector,
                [Unused] Action<object> setChildValue)
            {
                var m = getMetadata(ElementType);
                throw Readers.NewBindExceptionDueToEmptyElement(
                    input, m.ElementName);
            }
        }
    }
}
