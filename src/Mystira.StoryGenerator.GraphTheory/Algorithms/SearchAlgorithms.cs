using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.GraphTheory.Algorithms;

public static class SearchAlgorithms
{
    /// <summary>
    /// <para>
    /// Performs a breadth-first traversal starting from the specified nodes.
    /// </para>
    /// <para>
    /// Each node is visited at most once. The traversal order is determined
    /// by a standard FIFO queue over discovered nodes.
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
    /// The directed graph to traverse.
    /// </para>
    /// </param>
    /// <param name="startNodes">
    /// <para>
    /// The starting nodes for the traversal. Nodes that do not exist in the
    /// graph are ignored.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A sequence of nodes in breadth-first order, without repeats.
    /// </para>
    /// </returns>
    public static IEnumerable<TNode> BreadthFirst<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph,
        IEnumerable<TNode> startNodes)
        where TNode : notnull
    {
        var visited = new HashSet<TNode>();
        var queue = new Queue<TNode>();

        foreach (var start in startNodes)
        {
            if (visited.Add(start))
                queue.Enqueue(start);
        }

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            yield return node;

            foreach (var succ in graph.GetSuccessors(node))
            {
                if (visited.Add(succ))
                    queue.Enqueue(succ);
            }
        }
    }

    /// <summary>
    /// <para>
    /// Performs a depth-first traversal starting from the specified nodes.
    /// </para>
    /// <para>
    /// Each node is visited at most once. The traversal order is determined
    /// by a LIFO stack over discovered nodes, yielding a depth-first ordering.
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
    /// The directed graph to traverse.
    /// </para>
    /// </param>
    /// <param name="startNodes">
    /// <para>
    /// The starting nodes for the traversal. Nodes that do not exist in the
    /// graph are ignored.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A sequence of nodes in depth-first order, without repeats.
    /// </para>
    /// </returns>
    public static IEnumerable<TNode> DepthFirst<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph,
        IEnumerable<TNode> startNodes)
        where TNode : notnull
    {
        var visited = new HashSet<TNode>();
        var stack = new Stack<TNode>();

        foreach (var start in startNodes)
        {
            if (visited.Add(start))
                stack.Push(start);
        }

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;

            foreach (var succ in graph.GetSuccessors(node))
            {
                if (visited.Add(succ))
                    stack.Push(succ);
            }
        }
    }
}
