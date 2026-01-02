namespace Mystira.Shared.GraphTheory.Algorithms;

/// <summary>
/// Graph sorting algorithms (topological sort, cycle detection).
/// </summary>
public static class SortAlgorithms
{
    /// <summary>
    /// Computes a topological ordering of the nodes in a directed acyclic graph (DAG)
    /// using Kahn's algorithm.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <typeparam name="TEdgeLabel">The edge label type.</typeparam>
    /// <param name="graph">The directed graph to sort topologically.</param>
    /// <returns>A list of nodes in topological order.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the graph contains at least one directed cycle.
    /// </exception>
    public static IReadOnlyList<TNode> TopologicalSort<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph)
        where TNode : notnull
    {
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

    /// <summary>
    /// Determines whether the graph contains at least one cycle.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <typeparam name="TEdgeLabel">The edge label type.</typeparam>
    /// <param name="graph">The directed graph to check.</param>
    /// <returns>True if the graph contains a cycle; otherwise false.</returns>
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
