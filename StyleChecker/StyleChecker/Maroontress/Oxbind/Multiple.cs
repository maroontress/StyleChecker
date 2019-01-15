namespace Maroontress.Oxbind
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Maroontress.Oxbind.Impl;
    using StyleChecker.Annotations;

    /// <summary>
    /// The factory of the <see cref="SchemaType"/> object that represents
    /// the child element that consecutively occurs zero or more times.
    /// </summary>
    public static class Multiple
    {
        /// <summary>
        /// Creates the <see cref="SchemaType"/> object that represents
        /// the child element corresponding to the specified class
        /// that consecutively occurs zero or more times.
        /// </summary>
        /// <remarks>
        /// The <see cref="SchemaType"/> object for the specified class
        /// that this method returns
        /// must not follow consecutively one that
        /// <see cref="Optional.Of{T}()"/> or
        /// <see cref="Of{T}()"/> with the same class returns,
        /// in the parameters of the
        /// <see cref="Schema.Of(SchemaType[])"/>.
        /// For example, both
        /// <c>Schema.Of(Optional.Of&lt;Movie&gt;(),
        /// Multiple.Of&lt;Movie&gt;())</c> and
        /// <c>Schema.Of(Multiple.Of&lt;Movie&gt;(),
        /// Multiple.Of&lt;Movie&gt;())</c> are invalid.
        /// </remarks>
        /// <typeparam name="T">
        /// The class annotated with <see cref="ForElementAttribute"/>,
        /// representing the child element that
        /// consecutively occurs zero or more times.
        /// </typeparam>
        /// <returns>
        /// The <see cref="SchemaType"/> object.
        /// </returns>
        public static SchemaType Of<T>()
        {
            return new MultipleSchemaType(typeof(T));
        }

        private sealed class MultipleSchemaType : SchemaType
        {
            public MultipleSchemaType(Type type)
                : base(type, false, true)
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
                var list = new List<object>();
                for (;;)
                {
                    var nodeType = Readers.SkipCharacters(input);
                    if (nodeType != XmlNodeType.Element)
                    {
                        break;
                    }
                    if (!Readers.Equals(input, m.ElementName))
                    {
                        break;
                    }
                    var info = Readers.ToXmlLineInfo(input);
                    var child = m.CreateInstance(input, getMetadata);
                    list.Add(reflector.Sugarcoater(info, child));
                }
                var count = list.Count;
                var array = Array.CreateInstance(reflector.UnitType, count);
                for (var k = 0; k < count; ++k)
                {
                    array.SetValue(list[k], k);
                }
                setChildValue(array);
            }

            public override void ApplyWithEmptyElement(
                [Unused] XmlReader input,
                [Unused] Func<Type, Metadata> getMetadata,
                Reflector<object> reflector,
                Action<object> setChildValue)
            {
                var array = Array.CreateInstance(reflector.UnitType, 0);
                setChildValue(array);
            }
        }
    }
}
