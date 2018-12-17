namespace Maroontress.Oxbind
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Maroontress.Oxbind.Impl;

    /// <summary>
    /// A component of <see cref="Schema"/> that represents one of the child
    /// elements, which stands for the class corresponding to the child
    /// element, and how the child element occurs inside the parent.
    /// </summary>
    /// <remarks>
    /// The <see cref="SchemaType"/> object is immutable, and has a class
    /// annotated with <see cref="ForElementAttribute"/> corresponding to the
    /// child element. It must be created with one of <see
    /// cref="Mandatory.Of{T}()"/>, <see cref="Optional.Of{T}()"/> and <see
    /// cref="Multiple.Of{T}()"/>.
    /// </remarks>
    /// <seealso cref="Mandatory.Of{T}()"/>
    /// <seealso cref="Multiple.Of{T}()"/>
    /// <seealso cref="Optional.Of{T}()"/>
    public abstract class SchemaType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaType"/> class.
        /// </summary>
        /// <param name="type">
        /// The class corresponding to the child element.
        /// </param>
        /// <param name="isMandatory">
        /// Whether the child element always occurs inside the parent.
        /// </param>
        /// <param name="isMultiple">
        /// Whether the child element occurs zero or multiple times.
        /// </param>
        protected SchemaType(Type type, bool isMandatory, bool isMultiple)
        {
            ElementType = type;
            IsMandatory = isMandatory;
            PlaceholderType = isMultiple
                ? typeof(IEnumerable<>).MakeGenericType(type)
                : type;
        }

        /// <summary>
        /// Gets the class corresponding to the child element.
        /// </summary>
        /// <value>
        /// The class corresponding to the child element.
        /// </value>
        public Type ElementType { get; }

        /// <summary>
        /// Gets the placeholder class corresponding to the child element.
        /// </summary>
        /// <value>
        /// The placeholder class corresponding to the child element.
        /// </value>
        public Type PlaceholderType { get; }

        /// <summary>
        /// Gets a value indicating whether the child element always occurs
        /// inside the parent.
        /// </summary>
        /// <value>
        /// <c>true</c> if the child element always occurs inside the parent.
        /// </value>
        public bool IsMandatory { get; }

        /// <summary>
        /// Applies the XML stream into the specified object.
        /// </summary>
        /// <param name="metadata">
        /// The metadata of the <paramref name="instance"/>'s class.
        /// </param>
        /// <param name="instance">
        /// The object to be injected.
        /// </param>
        /// <param name="input">
        /// The <see cref="XmlReader"/> object.
        /// </param>
        /// <param name="getMetadata">
        /// The function that returns the metadata corresponding to the
        /// specified class.
        /// </param>
        public abstract void Apply(
            SchemaMetadata metadata,
            object instance,
            XmlReader input,
            Func<Type, Metadata> getMetadata);
    }
}
