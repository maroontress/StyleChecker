namespace StyleChecker.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Maroontress.Oxbind;

    /// <summary>
    /// The configuration data of DiscardingReturnValue analyzer.
    /// </summary>
    [ForElement("DiscardingReturnValue", Namespace)]
    public sealed class DiscardingReturnValueConfig : AbstractConfig
    {
        [ElementSchema]
        private static readonly Schema TheSchema = Schema.Of(
            Multiple.Of<Method>());

        [field: ForChild]
        private IEnumerable<Method> MethodElements { get; }
            = Array.Empty<Method>();

        /// <summary>
        /// Gets the signatures of the methods whose return value must not be
        /// discarded.
        /// </summary>
        /// <returns>
        /// The signatures of the methods.
        /// </returns>
        public IEnumerable<string> GetMethodSignatures()
        {
            return MethodElements.Select(e => e.Id);
        }

        /// <inheritdoc/>
        public override IEnumerable<(int, int, string)> Validate()
            => NoError;

        /// <summary>
        /// Represents the method whose return value must not be discarded.
        /// </summary>
        [ForElement("method", Namespace)]
        public sealed class Method
        {
            /// <summary>
            /// Gets the signature of the method.
            /// </summary>
            [field: ForAttribute("id")]
            public string Id { get; }
        }
    }
}
