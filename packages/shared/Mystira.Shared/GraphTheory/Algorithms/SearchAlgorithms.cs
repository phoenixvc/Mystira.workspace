namespace Mystira.Shared.GraphTheory.Algorithms;

/// <summary>
/// Graph traversal algorithms (BFS, DFS).
/// </summary>
public static class SearchAlgorithms
{
    /// <summary>
    /// Performs a breadth-first search starting from a single node with a visitor callback.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <typeparam name="TEdgeLabel">The edge label type.</typeparam>
    /// <param name="graph">The directed graph to traverse.</param>
    /// <param name="start">The starting node for the traversal.</param>
    /// <param name="visitor">A callback invoked for each visited node. Return true to continue, false to stop.</param>
    public static void BreadthFirstSearch<TNode, TEdgeLabel>(
        DirectedGraph<TNode, TEdgeLabel> graph,
        TNode start,
        Func<TNode, bool> visitor)
        where TNode : notnull
    {
        if (!graph.Nodes.Contains(start))
            return;

        var visited = new HashSet<TNode>();
        var queue = new Queue<TNode>();

        if (visited.Add(start))
            queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (!visitor(node))
                return;

            foreach (var succ in graph.GetSuccessors(node))
            {
                if (visited.Add(succ))
                    queue.Enqueue(succ);
            }
        }
    }

    /// <summary>
    /// Performs a depth-first search starting from a single node with a visitor callback.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <typeparam name="TEdgeLabel">The edge label type.</typeparam>
    /// <param name="graph">The directed graph to traverse.</param>
    /// <param name="start">The starting node for the traversal.</param>
    /// <param name="visitor">A callback invoked for each visited node. Return true to continue, false to stop.</param>
    public static void DepthFirstSearch<TNode, TEdgeLabel>(
        DirectedGraph<TNode, TEdgeLabel> graph,
        TNode start,
        Func<TNode, bool> visitor)
        where TNode : notnull
    {
        if (!graph.Nodes.Contains(start))
            return;

        var visited = new HashSet<TNode>();
        var stack = new Stack<TNode>();

        if (visited.Add(start))
            stack.Push(start);

        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!visitor(node))
                return;

            foreach (var succ in graph.GetSuccessors(node))
            {
                if (visited.Add(succ))
                    stack.Push(succ);
            }
        }
    }

    /// <summary>
    /// Performs a breadth-first traversal starting from the specified nodes.
    /// Each node is visited at most once.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <typeparam name="TEdgeLabel">The edge label type.</typeparam>
    /// <param name="graph">The directed graph to traverse.</param>
    /// <param name="startNodes">The starting nodes for the traversal.</param>
    /// <returns>A sequence of nodes in breadth-first order, without repeats.</returns>
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
    /// Performs a depth-first traversal starting from the specified nodes.
    /// Each node is visited at most once.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <typeparam name="TEdgeLabel">The edge label type.</typeparam>
    /// <param name="graph">The directed graph to traverse.</param>
    /// <param name="startNodes">The starting nodes for the traversal.</param>
    /// <returns>A sequence of nodes in depth-first order, without repeats.</returns>
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
