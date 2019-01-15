namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Xml;
    using Maroontress.Util;

    /// <summary>
    /// Metadata binding a class and its members to the XML element and
    /// attributes.
    /// </summary>
    /// <remarks>
    /// <c>Metadata</c> objects are immutable.
    /// </remarks>
    public abstract class Metadata
    {
        /// <summary>
        /// The map of the attribute name to the <see cref="Reflector{T}"/>
        /// object.
        /// </summary>
        private readonly
            IReadOnlyDictionary<XmlQualifiedName, Reflector<string>>
            attributeReflectorMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="Metadata"/> class.
        /// </summary>
        /// <param name="elementClass">
        /// The class annotated with <see cref="ForElementAttribute"/>.
        /// </param>
        protected Metadata(Type elementClass)
        {
            ElementClass = elementClass;
            ElementName = GetElementName(elementClass);
            attributeReflectorMap = AttributeReflectorMap.Of(elementClass);
        }

        /// <summary>
        /// Gets the class representing the XML element, annotated with <see
        /// cref="ForElementAttribute"/> for the class bound to this metadata.
        /// </summary>
        public Type ElementClass { get; }

        /// <summary>
        /// Gets the name of the XML element, which is the value of the
        /// annotation <see cref="ForElementAttribute"/> for the class bound to
        /// this metadata.
        /// </summary>
        public XmlQualifiedName ElementName { get; }

        /// <summary>
        /// Returns a new instance bound to the root XML element that is read
        /// from the specified XML reader.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="getMetadata">
        /// The function that returns the <see cref="Metadata"/>
        /// object associated with its argument of the specified class.
        /// </param>
        /// <returns>
        /// A new instance bound to the root XML element.
        /// </returns>
        public object MandatoryElement(
            XmlReader @in,
            Func<Type, Metadata> getMetadata)
        {
            _ = Readers.SkipCharacters(@in);
            Readers.ConfirmStartElement(@in, ElementName);
            return CreateInstance(@in, getMetadata);
        }

        /// <summary>
        /// Creates a new instance bound to the next XML element in the
        /// specified XML reader.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="getMetadata">
        /// The function that returns the <see cref="Metadata"/> object
        /// associated with its argument of the specified class.
        /// </param>
        /// <returns>
        /// A new instance bound to the next XML element in the specified XML
        /// reader.
        /// </returns>
        public object CreateInstance(
            XmlReader @in, Func<Type, Metadata> getMetadata)
        {
            var instance = Activator.CreateInstance(ElementClass);
            Elements.ForEach(@in.AttributeCount, k =>
            {
                @in.MoveToAttribute(k);
                DispatchAttribute(@in, instance);
            });
            @in.MoveToElement();
            if (@in.IsEmptyElement)
            {
                HandleComponentsWithEmptyElement(instance, @in, getMetadata);
            }
            else
            {
                @in.Read();
                HandleComponentsWithContent(instance, @in, getMetadata);

                _ = Readers.SkipCharacters(@in);
                Readers.ConfirmEndElement(@in, ElementName);
            }
            @in.Read();
            return instance;
        }

        /// <summary>
        /// Handles the component of the specified instance with content of
        /// the element that the specified XML reader provides.
        /// </summary>
        /// <param name="instance">
        /// The instance whose components are handled.
        /// </param>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="getMetadata">
        /// The function that returns the <see cref="Metadata"/>
        /// object associated with its argument of the specified class.
        /// </param>
        protected abstract void HandleComponentsWithContent(
            object instance,
            XmlReader @in,
            Func<Type, Metadata> getMetadata);

        /// <summary>
        /// Handles the component of the specified instance with empty element
        /// that the specified XML reader is providing.
        /// </summary>
        /// <param name="instance">
        /// The instance whose components are handled.
        /// </param>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="getMetadata">
        /// The function that returns the <see cref="Metadata"/>
        /// object associated with its argument of the specified class.
        /// </param>
        protected abstract void HandleComponentsWithEmptyElement(
            object instance,
            XmlReader @in,
            Func<Type, Metadata> getMetadata);

        /// <summary>
        /// Returns the element name bound to the class annotated with
        /// <see cref="ForElementAttribute"/>.
        /// </summary>
        /// <param name="clazz">
        /// The class that must be marked with the annotation
        /// <c>[ForElement]</c>.
        /// </param>
        /// <returns>
        /// The element name.
        /// </returns>
        private static XmlQualifiedName GetElementName(Type clazz)
        {
            var a = clazz.GetTypeInfo()
                .GetCustomAttribute<ForElementAttribute>();
            Debug.Assert(a != null, "no ForElement attribute");
            return a.QName;
        }

        /// <summary>
        /// Performs the delegate <see cref="Reflector{T}"/> associated with
        /// the key that represents the attribute name, with the specified XML
        /// reader and instance.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="instance">
        /// The instance of the <see cref="ElementClass"/>.
        /// </param>
        private void DispatchAttribute(XmlReader @in, object instance)
        {
            var key = Readers.NewQName(@in);
            if (!attributeReflectorMap.TryGetValue(key, out var reflector))
            {
                // just ignore the attribute if it is unknown.
                return;
            }
            var value = @in.Value;
            var info = Readers.AsXmlLineInfo(@in);
            reflector.Inject(instance, reflector.Sugarcoater(info, value));
        }
    }
}
