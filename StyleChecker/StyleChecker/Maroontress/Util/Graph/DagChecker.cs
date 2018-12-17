namespace Maroontress.Util.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A checker that checks a directed acyclic graph (DAG), is a directed
    /// graph with no directed cycles.
    /// </summary>
    /// <remarks>
    /// For more information about DAG, see
    /// <a href="http://en.wikipedia.org/wiki/Directed_acyclic_graph">
    /// Directed acyclic graph</a>.
    /// </remarks>
    /// <typeparam name="T">The type of the node of DAG.</typeparam>
    public sealed class DagChecker<T>
    {
        /// <summary>
        /// The node set for the breadcrumb.
        /// </summary>
        private readonly LinkedHashSet<T> set = new LinkedHashSet<T>();

        /// <summary>
        /// The node set containing the nodes that have no circular
        /// dependencies.
        /// </summary>
        private readonly ISet<T> checkedSet;

        /// <summary>
        /// The function that returns the dependencies of the specified node.
        /// </summary>
        private readonly Func<T, ISet<T>> getDependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="DagChecker{T}"/>
        /// class.
        /// </summary>
        /// <param name="getDependencies">
        /// The function that returns the dependencies of the specified node.
        /// </param>
        public DagChecker(Func<T, ISet<T>> getDependencies)
        {
            this.getDependencies = getDependencies;
            checkedSet = new HashSet<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DagChecker{T}"/>
        /// class, with the specified node set containing the nodes that have
        /// no circular dependencies.
        ///
        /// The <see cref="Check(T)"/> method may add the nodes to the
        /// specified node set.
        /// </summary>
        /// <param name="getDependencies">
        /// The function that returns the dependencies of the specified node.
        /// </param>
        /// <param name="checkedSet">
        /// The node set containing the nodes that have no circular
        /// dependencies.
        /// </param>
        public DagChecker(Func<T, ISet<T>> getDependencies, ISet<T> checkedSet)
        {
            this.getDependencies = getDependencies;
            this.checkedSet = checkedSet;
        }

        /// <summary>
        /// Checks recursively that the node has a circular dependency.
        /// </summary>
        /// <param name="node">
        /// The node of the DAG.
        /// </param>
        /// <exception cref="CircularDependencyException">
        /// if the circular dependency has occurred.
        /// </exception>
        public void Check(T node)
        {
            if (!set.Add(node))
            {
                var list = new List<T>(set)
                {
                    node,
                };
                var index = list.IndexOf(node);
                var loop = list.GetRange(index, list.Count - index);
                throw CircularDependencyException.Of(loop);
            }
            try
            {
                var dependencies = getDependencies(node)
                    .Where(d => !checkedSet.Contains(d));
                foreach (var d in dependencies)
                {
                    Check(d);
                }
                checkedSet.Add(node);
            }
            finally
            {
                set.Remove(node);
            }
        }
    }
}
