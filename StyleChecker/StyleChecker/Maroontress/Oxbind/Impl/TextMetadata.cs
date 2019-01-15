namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            Debug.Assert(size == 1, $"{size}");
            var textField = list.First();
            var type = textField.FieldType;
            var sugarcoater = StringSugarcoaters.Of(type);
            Reflector = new Reflector<string>(
                textField.SetValue, type, sugarcoater);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMetadata"/> class.
        /// </summary>
        /// <param name="clazz">
        /// The class annotated with <see cref="ForElementAttribute"/>.
        /// </param>
        /// <param name="list">
        /// The list of the instance method marked with the annotation <see
        /// cref="FromTextAttribute"/>. The return type of the method must be
        /// <see cref="void"/> and the method has the single parameter whose
        /// type is <see cref="string"/>.
        /// </param>
        public TextMetadata(
            Type clazz, IEnumerable<MethodInfo> list)
            : base(clazz)
        {
            var size = list.Count();
            Debug.Assert(size == 1, $"{size}");
            var textMethod = list.First();
            var returnType = textMethod.ReturnType;
            Debug.Assert(returnType.Equals(Types.Void), $"{returnType}");
            var parameters = textMethod.GetParameters();
            var length = parameters.Length;
            Debug.Assert(length == 1, $"{length}");
            var type = parameters[0].ParameterType;
            var sugarcoater = StringSugarcoaters.Of(type);
            Reflector = new Reflector<string>(
                (i, t) => textMethod.Invoke(i, new object[] { t }),
                type,
                sugarcoater);
        }

        private Reflector<string> Reflector { get; }

        /// <inheritdoc/>
        protected override void HandleComponentsWithContent(
            object instance,
            XmlReader @in,
            [Unused] Func<Type, Metadata> getMetadata)
        {
            var info = Readers.ToXmlLineInfo(@in);
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
            Reflector.Inject(
                instance, Reflector.Sugarcoater(info, b.ToString()));
        }

        /// <inheritdoc/>
        protected override void HandleComponentsWithEmptyElement(
            object instance,
            XmlReader @in,
            [Unused] Func<Type, Metadata> getMetadata)
        {
            // treats '<element/>' as well as '<element></element>'.
            var info = Readers.ToXmlLineInfo(@in);
            Reflector.Inject(
                instance, Reflector.Sugarcoater(info, string.Empty));
        }
    }
}
