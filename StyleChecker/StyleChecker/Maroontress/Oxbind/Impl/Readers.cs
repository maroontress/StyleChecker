namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Xml;

    /// <summary>
    /// Reads and checks XML reader.
    /// </summary>
    public static class Readers
    {
        private static readonly IXmlLineInfo NoLineInfo
            = new DefaultXmlLineInfo();

        /// <summary>
        /// Does nothing if there are more parsing events, or throws
        /// <see cref="BindException"/> otherwise.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        public static void ConfirmNext(XmlReader @in)
        {
            if (!@in.EOF)
            {
                return;
            }
            throw new BindException("unexpected end of stream.");
        }

        /// <summary>
        /// Skips the text and checks End Of Stream in the specified XML
        /// reader. If the specified XML reader does not reach at the end of
        /// the stream, throws <see cref="BindException"/>.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        public static void ConfirmEndOfStream(XmlReader @in)
        {
            while (IsCharacters(@in.NodeType))
            {
                @in.Read();
            }
            if (@in.EOF)
            {
                return;
            }
            throw new BindException("expected end of stream");
        }

        /// <summary>
        /// Does nothing if the element of the specified local name starts at
        /// the specified XML reader, or throws <see cref="BindException"/>
        /// otherwise.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="expectedName">
        /// The local name that is expected.
        /// </param>
        public static void ConfirmStartElement(
            XmlReader @in, XmlQualifiedName expectedName)
        {
            ConfirmElement(@in, expectedName, XmlNodeType.Element, "start");
        }

        /// <summary>
        /// Does nothing if the element of the specified local name ends at
        /// the specified XML reader, or throws <see cref="BindException"/>
        /// otherwise.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="expectedName">
        /// The local name that is expected.
        /// </param>
        public static void ConfirmEndElement(
            XmlReader @in, XmlQualifiedName expectedName)
        {
            ConfirmElement(@in, expectedName, XmlNodeType.EndElement, "end");
        }

        /// <summary>
        /// Skips the text in the specified XML reader.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <returns>
        /// The current node type other than the text.
        /// </returns>
        public static XmlNodeType SkipCharacters(XmlReader @in)
        {
            for (;;)
            {
                ConfirmNext(@in);
                var nodeType = @in.NodeType;
                if (!IsCharacters(nodeType))
                {
                    return nodeType;
                }
                @in.Read();
            }
        }

        /// <summary>
        /// Returns the new <see cref="BindEvent{T}"/> object of the specified
        /// value and the specified location information.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the event value.
        /// </typeparam>
        /// <param name="info">
        /// The location information.
        /// </param>
        /// <param name="value">
        /// The value of the event.
        /// </param>
        /// <returns>
        /// The new bind event.
        /// </returns>
        public static BindEvent<T> NewEvent<T>(IXmlLineInfo info, T value)
            => new BindEventImpl<T>(value, info);

        /// <summary>
        /// Returns the new <see cref="BindEvent{T}"/> object of the specified
        /// value and the specified location information.
        /// </summary>
        /// <param name="info">
        /// The location information.
        /// </param>
        /// <param name="value">
        /// The value of the event.
        /// </param>
        /// <returns>
        /// The new bind event.
        /// </returns>
        public static object NewEventObject(
            IXmlLineInfo info, object value)
        {
            var type = value.GetType();
            var eventType = Types.BindEventImplT.MakeGenericType(type);
            return Activator.CreateInstance(
                eventType, new object[] { value, info });
        }

        /// <summary>
        /// Gets the new <see cref="IXmlLineInfo"/> object associated with the
        /// specified XML reader.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <returns>
        /// The <see cref="IXmlLineInfo"/> object which is immutable.
        /// </returns>
        public static IXmlLineInfo ToXmlLineInfo(XmlReader @in)
            => @in is IXmlLineInfo info
                ? new DefaultXmlLineInfo(info)
                : NoLineInfo;

        /// <summary>
        /// Gets the <see cref="IXmlLineInfo"/> object associated with the
        /// specified XML reader.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <returns>
        /// The <see cref="IXmlLineInfo"/> object that the XML reader object is
        /// casted to, or the default <see cref="IXmlLineInfo"/> object if it
        /// is failed to cast.
        /// </returns>
        public static IXmlLineInfo AsXmlLineInfo(XmlReader @in)
            => @in is IXmlLineInfo info
                ? info
                : NoLineInfo;

        /// <summary>
        /// Creates a new <see cref="BindException"/> representing
        /// the lack of the mandatory child element in the empty element.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="expectedChildElementName">
        /// The name of the element that is expected as the child element.
        /// </param>
        /// <returns>
        /// The new <see cref="BindException"/>.
        /// </returns>
        public static BindException NewBindExceptionDueToEmptyElement(
            XmlReader @in, XmlQualifiedName expectedChildElementName)
        {
            var actualNodeType = @in.NodeType;
            var actualName = NewQName(@in);
            var actualElement
                = $"{actualNodeType} of the element '{actualName}'";
            var hint = $"it is expected that the element '{actualName}' "
                + $"contains the child element '{expectedChildElementName}'";
            var reason = $"element is empty: {actualElement} ({hint})";
            var info = ToXmlLineInfo(@in);
            var line = info.LineNumber;
            var column = info.LinePosition;
            var m = $"{line}:{column}: {reason}";
            return new BindException(m);
        }

        /// <summary>
        /// Gets a new <see cref="XmlQualifiedName"/> of the specified
        /// <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <returns>
        /// The new qualified name that the specified XML reader represents.
        /// </returns>
        public static XmlQualifiedName NewQName(XmlReader @in)
            => new XmlQualifiedName(@in.LocalName, @in.NamespaceURI);

        /// <summary>
        /// Gets whether the name that the specifed XML reader represents
        /// equals to the specified qualified name.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="qName">
        /// The qualified name.
        /// </param>
        /// <returns>
        /// <c>true</c> if the name that the XML reader represents equals
        /// to the qualified name, <c>false</c> otherwise.
        /// </returns>
        public static bool Equals(XmlReader @in, XmlQualifiedName qName)
            => @in.LocalName.Equals(qName.Name)
                && @in.NamespaceURI.Equals(qName.Namespace);

        /// <summary>
        /// Throws a <see cref="BindException"/>.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="hint">
        /// The hint message.
        /// </param>
        private static void UnexpectedNodeType(XmlReader @in, string hint)
        {
            var actual = @in.NodeType;
            var actualNodeType = actual.ToString();
            if (actual == XmlNodeType.Element
                || actual == XmlNodeType.EndElement)
            {
                @in.MoveToElement();
                actualNodeType += $" of the element '{@in.LocalName}'";
            }
            var reason = $"unexpected node type: {actualNodeType} ({hint})";
            var info = ToXmlLineInfo(@in);
            var line = info.LineNumber;
            var column = info.LinePosition;
            var m = $"{line}:{column}: {reason}";
            throw new BindException(m);
        }

        /// <summary>
        /// Does nothing if the node type of the he specified XML reader is
        /// equal to the specified node type, and the local name is equal to
        /// the specified local name, or throws <see cref="BindException"/>
        /// otherwise.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="expectedName">
        /// The local name that is expected.
        /// </param>
        /// <param name="expectedNodeType">
        /// <see cref="XmlNodeType.Element"/> or <see
        /// cref="XmlNodeType.EndElement"/>.
        /// </param>
        /// <param name="hint">
        /// The hint of the exception (<c>"start"</c> or <c>"end"</c>).
        /// </param>
        private static void ConfirmElement(
            XmlReader @in,
            XmlQualifiedName expectedName,
            XmlNodeType expectedNodeType,
            string hint)
        {
            var actualNodeType = @in.NodeType;
            if (actualNodeType == expectedNodeType
                && Equals(@in, expectedName))
            {
                return;
            }
            UnexpectedNodeType(
                @in,
                $"it is expected that the element '{expectedName}' {hint}s");
        }

        /// <summary>
        /// Gets whether the specified node type represents characters or
        /// white spaces.
        /// </summary>
        /// <param name="nodeType">
        /// The node type to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if the node type represents characters or white
        /// spaces.
        /// </returns>
        private static bool IsCharacters(XmlNodeType nodeType)
            => nodeType == XmlNodeType.Text
                || nodeType == XmlNodeType.Whitespace;
    }
}
