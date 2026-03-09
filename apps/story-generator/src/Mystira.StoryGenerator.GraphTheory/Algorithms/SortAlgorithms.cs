using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.GraphTheory.Algorithms;

public static class SortAlgorithms
{
    /// <summary>
    /// <para>
    /// Computes a topological ordering of the nodes in a directed acyclic graph
    /// (DAG) using Kahn's algorithm.
    /// </para>
    /// <para>
    /// If the graph contains one or more cycles, this method throws an
    /// <see cref="InvalidOperationException"/>.
    /// </para>
    /// </summary>
    /// <typeparam name="TNode">
    /// <para>
    /// The node identifier type of the underlying graph.
    /// </para>
    /// </typeparam>
    /// <typeparam name="TEdgeLabel">
    /// <para>
    /// The edge label type of the underlying graph.
    /// </para>
    /// </typeparam>
    /// <param name="graph">
    /// <para>
    /// The directed graph to sort topologically.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A list of nodes in topological order, such that every edge goes from
    /// an earlier node to a later node in the sequence.
    /// </para>
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <para>
    /// Thrown if the graph contains at least one directed cycle and therefore
    /// cannot be topologically sorted.
    /// </para>
    /// </exception>
    public static IReadOnlyList<TNode> TopologicalSort<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph)
        where TNode : notnull
    {
        // Kahn's algorithm
        var inDegree = graph.Nodes.ToDictionary(n => n, n => graph.InDegree(n));
        var queue = new Queue<TNode>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var result = new List<TNode>(graph.Nodes.Count);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            result.Add(node);

            foreach (var succ in graph.GetSuccessors(node))
            {
                if (--inDegree[succ] == 0)
                    queue.Enqueue(succ);
            }
        }

        if (result.Count != graph.Nodes.Count)
        {
            throw new InvalidOperationException("Graph contains at least one cycle.");
        }

        return result;
    }

    public static bool HasCycle<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph)
        where TNode : notnull
    {
        try
        {
            graph.TopologicalSort();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }
}
