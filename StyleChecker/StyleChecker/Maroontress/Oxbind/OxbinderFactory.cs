namespace Maroontress.Oxbind
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Maroontress.Oxbind.Impl;
    using Maroontress.Util;
    using Maroontress.Util.Graph;

    /// <summary>
    /// The factory of the <see cref="Oxbinder{T}"/> object.
    /// </summary>
    public sealed class OxbinderFactory
    {
        /// <summary>
        /// The cache of the metadata.
        /// </summary>
        private readonly InternMap<Type, Metadata> metadataCache;

        /// <summary>
        /// The cache of the validated classes.
        /// </summary>
        private readonly Traversal<Type> validationTraversal;

        /// <summary>
        /// The cache of the classes that have been checked for DAG.
        /// </summary>
        private readonly DagChecker<Type> dagChecker;

        /// <summary>
        /// Initializes a new instance of the <see cref="OxbinderFactory"/>
        /// class.
        /// </summary>
        public OxbinderFactory()
        {
            metadataCache = new InternMap<Type, Metadata>();
            validationTraversal = new Traversal<Type>(type =>
            {
                var v = new Validator(type);
                if (!v.IsValid)
                {
                    var log = string.Join(
                        Environment.NewLine, v.GetMessages());
                    throw new BindException(
                        $"{type.Name} has failed to validate annotations: "
                            + $"{log}");
                }
                return v.GetSchemaClasses();
            });
            dagChecker = new DagChecker<Type>(Validator.GetDependencies);
        }

        /// <summary>
        /// Creates an <see cref="Oxbinder{T}"/> object for the specified
        /// class.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the class annotated with <see
        /// cref="ForElementAttribute"/> that stands for the root element.
        /// </typeparam>
        /// <returns>
        /// The <see cref="Oxbinder{T}"/> object to create new objects of the
        /// specified class with XML streams.
        /// </returns>
        /// <seealso cref="Oxbinder{T}.NewInstance(TextReader)"/>
        public Oxbinder<T> Of<T>()
        {
            return new OxBinderImpl<T>(GetSharedMetadata);
        }

        /// <summary>
        /// Returns the shared <see cref="Metadata"/> object for the specified
        /// class.
        /// </summary>
        /// <param name="type">The class corresponding to the element.</param>
        /// <returns>
        /// The <see cref="Metadata"/> object for the specified class.
        /// </returns>
        private Metadata GetSharedMetadata(Type type)
        {
            return metadataCache.Intern(type, () =>
            {
                validationTraversal.Visit(type);
                try
                {
                    dagChecker.Check(type);
                }
                catch (CircularDependencyException e)
                {
                    throw new BindException(
                        $"{type.Name} has circular dependency.", e);
                }
                var fields
                    = Classes.GetInstanceFields<ForTextAttribute>(type);
                var methods
                    = Classes.GetInstanceMethods<FromTextAttribute>(type);
                if (fields.Any() && methods.Any())
                {
                    Debug.Fail(fields.First() + " and " + methods.First());
                }
                return fields.Any() ? new TextMetadata(type, fields)
                    : methods.Any() ? new TextMetadata(type, methods)
                    : new SchemaMetadata(type) as Metadata;
            });
        }
    }
}
