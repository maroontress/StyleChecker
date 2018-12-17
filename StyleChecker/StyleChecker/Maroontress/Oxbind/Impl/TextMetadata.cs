namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using StyleChecker.Annotations;

    /// <summary>
    /// Metadata of the classes that have a single <see cref="string"/> field
    /// annotated with the <see cref="ForTextAttribute"/>.
    /// </summary>
    public sealed class TextMetadata : Metadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextMetadata"/> class.
        /// </summary>
        /// <param name="clazz">
        /// The class annotated with <see cref="ForElementAttribute"/>.
        /// </param>
        /// <param name="list">
        /// The list of the instance field marked with the annotation
        /// <see cref="ForTextAttribute"/>. The type of the field must be
        /// <see cref="string"/>.
        /// </param>
        public TextMetadata(
            Type clazz, IEnumerable<FieldInfo> list)
            : base(clazz)
        {
            var size = list.Count();
            if (size != 1)
            {
                throw new BindingException(size.ToString());
            }
            TextField = list.First();
            if (!TextField.FieldType.Equals(typeof(string)))
            {
                throw new BindingException(TextField.FieldType.ToString());
            }
        }

        /// <summary>
        /// Gets the property that stores a text value inside the XML element.
        /// </summary>
        private FieldInfo TextField { get; }

        /// <inheritdoc/>
        protected override void HandleComponents(
            object instance,
            XmlReader @in,
            [Unused] Func<Type, Metadata> getMetadata)
        {
            var b = new StringBuilder();
            for (;;)
            {
                Readers.ConfirmNext(@in);
                var nodeType = @in.NodeType;
                if (nodeType != XmlNodeType.Text)
                {
                    break;
                }
                b.Append(@in.Value);
                @in.Read();
            }
            TextField.SetValue(instance, b.ToString());
        }
    }
}
