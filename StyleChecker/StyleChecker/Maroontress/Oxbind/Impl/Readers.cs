namespace Maroontress.Oxbind.Impl
{
    using System.Xml;

    /// <summary>
    /// Reads and checks XML reader.
    /// </summary>
    public static class Readers
    {
        /// <summary>
        /// Does nothing if there are more parsing events, or throws
        /// <see cref="BindingException"/> otherwise.
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
            throw new BindingException("unexpected end of stream.");
        }

        /// <summary>
        /// Does nothing if the specified node type is equal to the current
        /// node of the specified XML reader, or throws
        /// <see cref="BindingException"/> otherwise.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="expected">
        /// The expected node type.
        /// </param>
        public static void ConfirmNodeType(
            XmlReader @in, XmlNodeType expected)
        {
            ConfirmNodeType(@in, @in.NodeType, expected);
        }

        /// <summary>
        /// Does nothing if the specified node types are equal, or throws
        /// <see cref="BindingException"/> otherwise.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="actual">
        /// The actual node type.
        /// </param>
        /// <param name="expected">
        /// The expected event type.
        /// </param>
        public static void ConfirmNodeType(
            XmlReader @in, XmlNodeType actual, XmlNodeType expected)
        {
            if (actual == expected)
            {
                return;
            }
            UnexpectedNodeType(@in, $"node type {expected} is expected");
        }

        /// <summary>
        /// Skips the text and checks End Of Stream in the specified XML
        /// reader. If the specified XML reader does not reach at the end of
        /// the stream, throws <see cref="BindingException"/>.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        public static void ConfirmEndOfStream(XmlReader @in)
        {
            while (IsSkipCharacter(@in.NodeType))
            {
                @in.Read();
            }
            if (!@in.EOF)
            {
                throw new BindingException("expected end of stream");
            }
        }

        /// <summary>
        /// Does nothing if the specified local name is equal to the current
        /// local name of the specified XML reader, or throws
        /// <see cref="BindingException"/> otherwise.
        /// </summary>
        /// <param name="in">
        /// The XML reader.
        /// </param>
        /// <param name="expectedName">
        /// The local name that is expected.
        /// </param>
        public static void ConfirmLocalName(
            XmlReader @in, string expectedName)
        {
            var actual = @in.NodeType;
            var actualName = @in.LocalName;
            if ((actual == XmlNodeType.Element
                || actual == XmlNodeType.EndElement)
                    && actualName.Equals(expectedName))
            {
                return;
            }
            UnexpectedNodeType(@in, $"the element {expectedName} is expected");
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
                if (!IsSkipCharacter(nodeType))
                {
                    return nodeType;
                }
                @in.Read();
            }
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
        public static bool IsSkipCharacter(XmlNodeType nodeType)
            => nodeType == XmlNodeType.Text
                || nodeType == XmlNodeType.Whitespace;

        /// <summary>
        /// Throws a <see cref="BindingException"/>.
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
            var actualHint = actual.ToString();
            if (actual == XmlNodeType.Element
                || actual == XmlNodeType.EndElement)
            {
                @in.MoveToElement();
                actualHint += $" of the element {@in.LocalName}";
            }
            var m = $"unexpected node type: {actualHint} ({hint})";
            throw new BindingException(m);
        }
    }
}
