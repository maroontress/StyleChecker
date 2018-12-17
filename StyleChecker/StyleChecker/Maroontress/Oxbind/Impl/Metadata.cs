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
        /// The map of the attribute name to the <see cref="Task{T}"/> object.
        /// </summary>
        private readonly IReadOnlyDictionary<string, Task<string>>
            attributeTaskMap;

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
            attributeTaskMap = AttributeTaskMap.Of(elementClass);
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
        public string ElementName { get; }

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
            Readers.ConfirmLocalName(@in, ElementName);
            return CreateInstance(@in, getMetadata);
        }

        /// <summary>
        /// Creates a new instance bound to the next XML element in the
        /// specified XML reader.
        /// </summary>
        /// <param name="in">
        /// The XML stream reader.
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
                var key = @in.Name;
                var value = @in.Value;
                DispatchAttribute(instance, key, value);
            });
            @in.MoveToElement();
            if (@in.IsEmptyElement)
            {
                using (var sub = @in.ReadSubtree())
                {
                    sub.Read();
                    sub.Read();
                    HandleComponents(instance, @sub, getMetadata);
                }
            }
            else
            {
                @in.Read();
                HandleComponents(instance, @in, getMetadata);

                var nodeType = Readers.SkipCharacters(@in);
                Readers.ConfirmNodeType(@in, nodeType, XmlNodeType.EndElement);
                Readers.ConfirmLocalName(@in, ElementName);
            }
            @in.Read();
            return instance;
        }

        /// <summary>
        /// Handles the component of the specified instance with the specified
        /// XML reader.
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
        protected abstract void HandleComponents(
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
        private static string GetElementName(Type clazz)
        {
            var a = clazz.GetTypeInfo()
                .GetCustomAttribute<ForElementAttribute>();
            Debug.Assert(a != null, "no ForElement attribute");
            return a.Name;
        }

        /// <summary>
        /// Performs the delegate <see cref="Task{T}"/> associated with the
        /// specified key that represents the attribute name.
        /// </summary>
        /// <param name="instance">
        /// The instance of the <see cref="ElementClass"/>, that is
        /// the first argument for the delegate.
        /// </param>
        /// <param name="key">
        /// The attribute name.
        /// </param>
        /// <param name="value">
        /// The second argument for the delegate.
        /// </param>
        private void DispatchAttribute(
            object instance, string key, string value)
        {
            if (!attributeTaskMap.TryGetValue(key, out var task))
            {
                return;
            }
            task(instance, value);
        }
    }
}
