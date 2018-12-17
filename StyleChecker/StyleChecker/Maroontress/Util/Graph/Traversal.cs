namespace Maroontress.Util.Graph
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a way of graph traversal to visit all the reachable nodes from
    /// the specified node.
    /// </summary>
    /// <remarks>
    /// For more information, see
    /// <a href="https://en.wikipedia.org/wiki/Graph_traversal">
    /// Graph traversal</a>:
    ///
    /// <para>
    /// <blockquote>
    /// <para>Unlike tree traversal, graph traversal may require that some
    /// nodes be visited more than once, since it is not necessarily known
    /// before transitioning to a node that it has already been explored. As
    /// graphs become more dense, this redundancy becomes more prevalent,
    /// causing computation time to increase; as graphs become more sparse, the
    /// opposite holds true.</para>
    ///
    /// <para>Thus, it is usually necessary to remember which nodes have
    /// already been explored by the algorithm, so that nodes are revisited as
    /// infrequently as possible (or in the worst case, to prevent the
    /// traversal from continuing indefinitely). This may be accomplished by
    /// associating each node of the graph with a "color" or "visitation" state
    /// during the traversal, which is then checked and updated as the
    /// algorithm visits each node. If the node has already been visited, it is
    /// ignored and the path is pursued no further; otherwise, the algorithm
    /// checks/updates the node and continues down its current path.</para>
    /// </blockquote>
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The type of the node.
    /// </typeparam>
    public sealed class Traversal<T>
    {
        /// <summary>
        /// The set of the visiting node.
        /// </summary>
        private readonly ISet<T> visitingSet = new HashSet<T>();

        /// <summary>
        /// The function that returns the dependencies of the specified node.
        /// </summary>
        private readonly Func<T, IEnumerable<T>> getDependencies;

        /// <summary>
        /// Initializes a new instance of the <see cref="Traversal{T}"/> class.
        /// </summary>
        /// <param name="getDependencies">
        /// The function that returns the dependencies of the specified node.
        /// </param>
        public Traversal(Func<T, IEnumerable<T>> getDependencies)
        {
            this.getDependencies = getDependencies;
        }

        /// <summary>
        /// Visits all the reachable nodes from the specified node.
        /// </summary>
        /// <param name="node">
        /// The node where to start visiting.
        /// </param>
        public void Visit(T node)
        {
            IfNotVisited(getDependencies(node));
        }

        /// <summary>
        /// Visits recursively all the specified nodes that have never been
        /// visited.
        ///
        /// At first, calls the {@code getDependencies} function with
        /// the specified node. And then calls recursively the function
        /// with those nodes that have never been visited.
        /// </summary>
        /// <param name="all">
        /// The reachable nodes.
        /// </param>
        private void IfNotVisited(IEnumerable<T> all)
        {
            lock (visitingSet)
            {
                foreach (var node in all)
                {
                    if (visitingSet.Add(node))
                    {
                        Visit(node);
                    }
                }
            }
        }
    }
}
