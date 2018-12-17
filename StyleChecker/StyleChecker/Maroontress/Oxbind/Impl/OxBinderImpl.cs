namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// The default implementation of the <see cref="Oxbinder{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the instance to create.
    /// </typeparam>
    public sealed class OxBinderImpl<T> : Oxbinder<T>
    {
        /// <summary>
        /// The class of the instance that the method
        /// <see cref="NewInstance(TextReader)"/> returns.
        /// </summary>
        private readonly Type clazz;

        /// <summary>
        /// The function that returns the <see cref="Metadata"/>
        /// associated with the specified class.
        /// </summary>
        private readonly Func<Type, Metadata> getMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="OxBinderImpl{T}"/>
        /// class.
        /// </summary>
        /// <param name="getMetadata">
        /// The function that returns the <see cref="Metadata"/> associated
        /// with the specified class.
        /// </param>
        public OxBinderImpl(Func<Type, Metadata> getMetadata)
        {
            clazz = typeof(T);
            this.getMetadata = getMetadata;
        }

        /// <inheritdoc/>
        public T NewInstance(TextReader reader)
        {
            var settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
            };
            return NewInstance(XmlReader.Create(reader, settings));
        }

        /// <summary>
        /// Creates a new instance from the specified XML reader.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <returns>
        /// A new instance.
        /// </returns>
        private T NewInstance(XmlReader @in)
        {
            Readers.ConfirmNext(@in);
            @in.Read();
            if (@in.NodeType == XmlNodeType.XmlDeclaration)
            {
                Readers.ConfirmNext(@in);
                @in.Read();
            }
            var nodeType = Readers.SkipCharacters(@in);
            Readers.ConfirmNodeType(@in, nodeType, XmlNodeType.Element);
            var m = getMetadata(clazz);
            var instance = m.MandatoryElement(@in, getMetadata);
            Readers.ConfirmEndOfStream(@in);
            return (T)instance;
        }
    }
}
