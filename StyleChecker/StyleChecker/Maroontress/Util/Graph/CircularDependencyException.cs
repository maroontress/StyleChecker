namespace Maroontress.Util.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Thrown when a circular dependency has occurred in the DAG.
    /// </summary>
    /// <see cref="DagChecker{T}"/>
    public sealed class CircularDependencyException
        : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="CircularDependencyException"/> class.
        /// </summary>
        public CircularDependencyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="CircularDependencyException"/> class,
        /// with the specified detail message.
        /// </summary>
        /// <param name="message">
        /// A detail message.
        /// </param>
        public CircularDependencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="CircularDependencyException"/> class.
        /// </summary>
        /// <param name="message">
        /// A detail message.
        /// </param>
        /// <param name="innerException">
        /// The cause.
        /// </param>
        public CircularDependencyException(
            string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see
        /// cref="CircularDependencyException"/> class,
        /// with the list representing a circular dependency.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the node.
        /// </typeparam>
        /// <param name="list">
        /// The list of nodes. The first node is equals to the
        /// last node. The <i>k</i>th node depends (<i>k</i> + 1)th node.
        /// </param>
        /// <returns>The new instance.</returns>
        public static CircularDependencyException Of<T>(IEnumerable<T> list)
            => new CircularDependencyException(
                string.Join(" -> ", list.Select(o => o.ToString())));
    }
}
