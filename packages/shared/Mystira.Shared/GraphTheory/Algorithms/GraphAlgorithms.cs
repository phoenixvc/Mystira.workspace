using Mystira.Shared.GraphTheory.Graph;

namespace Mystira.Shared.GraphTheory.Algorithms;

/// <summary>
/// Extension methods for graph algorithms.
/// </summary>
public static class GraphAlgorithms
{
    /// <summary>
    /// Performs a breadth-first search from a source node.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to search.</param>
    /// <param name="source">Starting node.</param>
    /// <returns>Nodes in BFS order.</returns>
    public static IEnumerable<TNode> BreadthFirstSearch<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph,
        TNode source)
        where TNode : notnull
    {
        if (!graph.ContainsNode(source))
            yield break;

        var visited = new HashSet<TNode>();
        var queue = new Queue<TNode>();

        visited.Add(source);
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var successor in graph.GetSuccessors(current))
            {
                if (visited.Add(successor))
                {
                    queue.Enqueue(successor);
                }
            }
        }
    }

    /// <summary>
    /// Performs a depth-first search from a source node.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to search.</param>
    /// <param name="source">Starting node.</param>
    /// <returns>Nodes in DFS order.</returns>
    public static IEnumerable<TNode> DepthFirstSearch<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph,
        TNode source)
        where TNode : notnull
    {
        if (!graph.ContainsNode(source))
            yield break;

        var visited = new HashSet<TNode>();
        var stack = new Stack<TNode>();

        stack.Push(source);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!visited.Add(current))
                continue;

            yield return current;

            foreach (var successor in graph.GetSuccessors(current).Reverse())
            {
                if (!visited.Contains(successor))
                {
                    stack.Push(successor);
                }
            }
        }
    }

    /// <summary>
    /// Performs a topological sort of the graph.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to sort.</param>
    /// <returns>Nodes in topological order, or null if the graph has cycles.</returns>
    public static IReadOnlyList<TNode>? TopologicalSort<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph)
        where TNode : notnull
    {
        var visited = new HashSet<TNode>();
        var inProgress = new HashSet<TNode>();
        var result = new List<TNode>();

        bool Visit(TNode node)
        {
            if (inProgress.Contains(node))
                return false; // Cycle detected

            if (visited.Contains(node))
                return true;

            inProgress.Add(node);

            foreach (var successor in graph.GetSuccessors(node))
            {
                if (!Visit(successor))
                    return false;
            }

            inProgress.Remove(node);
            visited.Add(node);
            result.Add(node);
            return true;
        }

        foreach (var node in graph.Nodes)
        {
            if (!visited.Contains(node))
            {
                if (!Visit(node))
                    return null;
            }
        }

        result.Reverse();
        return result;
    }

    /// <summary>
    /// Finds all cycles in the graph.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to analyze.</param>
    /// <returns>List of cycles (each cycle is a list of nodes).</returns>
    public static IReadOnlyList<IReadOnlyList<TNode>> FindCycles<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph)
        where TNode : notnull
    {
        var cycles = new List<IReadOnlyList<TNode>>();
        var visited = new HashSet<TNode>();
        var path = new List<TNode>();
        var pathSet = new HashSet<TNode>();

        void FindCyclesFrom(TNode node)
        {
            if (pathSet.Contains(node))
            {
                // Found a cycle
                var cycleStart = path.IndexOf(node);
                var cycle = path.Skip(cycleStart).ToList();
                cycles.Add(cycle);
                return;
            }

            if (visited.Contains(node))
                return;

            visited.Add(node);
            path.Add(node);
            pathSet.Add(node);

            foreach (var successor in graph.GetSuccessors(node))
            {
                FindCyclesFrom(successor);
            }

            path.RemoveAt(path.Count - 1);
            pathSet.Remove(node);
        }

        foreach (var node in graph.Nodes)
        {
            if (!visited.Contains(node))
            {
                FindCyclesFrom(node);
            }
        }

        return cycles;
    }

    /// <summary>
    /// Checks if the graph has any cycles.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to check.</param>
    /// <returns>True if the graph has cycles.</returns>
    public static bool HasCycles<TNode, TLabel>(this DirectedGraph<TNode, TLabel> graph)
        where TNode : notnull =>
        graph.TopologicalSort() == null;

    /// <summary>
    /// Finds all paths from a source to a target node.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to search.</param>
    /// <param name="source">Starting node.</param>
    /// <param name="target">Target node.</param>
    /// <param name="maxPaths">Maximum number of paths to find.</param>
    /// <returns>All paths from source to target.</returns>
    public static IReadOnlyList<IReadOnlyList<TNode>> FindAllPaths<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph,
        TNode source,
        TNode target,
        int maxPaths = 1000)
        where TNode : notnull
    {
        var paths = new List<IReadOnlyList<TNode>>();
        var currentPath = new List<TNode>();
        var visited = new HashSet<TNode>();

        void FindPaths(TNode current)
        {
            if (paths.Count >= maxPaths)
                return;

            currentPath.Add(current);
            visited.Add(current);

            if (current.Equals(target))
            {
                paths.Add(currentPath.ToList());
            }
            else
            {
                foreach (var successor in graph.GetSuccessors(current))
                {
                    if (!visited.Contains(successor))
                    {
                        FindPaths(successor);
                    }
                }
            }

            currentPath.RemoveAt(currentPath.Count - 1);
            visited.Remove(current);
        }

        if (graph.ContainsNode(source) && graph.ContainsNode(target))
        {
            FindPaths(source);
        }

        return paths;
    }

    /// <summary>
    /// Computes shortest distances from a source using BFS (unweighted).
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to analyze.</param>
    /// <param name="source">Starting node.</param>
    /// <returns>Dictionary of node to distance from source.</returns>
    public static IReadOnlyDictionary<TNode, int> ShortestDistances<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph,
        TNode source)
        where TNode : notnull
    {
        var distances = new Dictionary<TNode, int>();
        if (!graph.ContainsNode(source))
            return distances;

        var queue = new Queue<TNode>();
        distances[source] = 0;
        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentDistance = distances[current];

            foreach (var successor in graph.GetSuccessors(current))
            {
                if (!distances.ContainsKey(successor))
                {
                    distances[successor] = currentDistance + 1;
                    queue.Enqueue(successor);
                }
            }
        }

        return distances;
    }

    /// <summary>
    /// Finds nodes reachable from a source node.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to analyze.</param>
    /// <param name="source">Starting node.</param>
    /// <returns>Set of reachable nodes.</returns>
    public static IReadOnlySet<TNode> GetReachableNodes<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph,
        TNode source)
        where TNode : notnull =>
        graph.BreadthFirstSearch(source).ToHashSet();

    /// <summary>
    /// Checks if a target node is reachable from a source node.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to analyze.</param>
    /// <param name="source">Starting node.</param>
    /// <param name="target">Target node.</param>
    /// <returns>True if target is reachable from source.</returns>
    public static bool IsReachable<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph,
        TNode source,
        TNode target)
        where TNode : notnull =>
        graph.BreadthFirstSearch(source).Any(n => n.Equals(target));

    /// <summary>
    /// Gets the strongly connected components of the graph using Kosaraju's algorithm.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to analyze.</param>
    /// <returns>List of strongly connected components.</returns>
    public static IReadOnlyList<IReadOnlyList<TNode>> GetStronglyConnectedComponents<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph)
        where TNode : notnull
    {
        var visited = new HashSet<TNode>();
        var finishOrder = new Stack<TNode>();

        // First DFS to get finish order
        void Dfs1(TNode node)
        {
            visited.Add(node);
            foreach (var successor in graph.GetSuccessors(node))
            {
                if (!visited.Contains(successor))
                {
                    Dfs1(successor);
                }
            }
            finishOrder.Push(node);
        }

        foreach (var node in graph.Nodes)
        {
            if (!visited.Contains(node))
            {
                Dfs1(node);
            }
        }

        // Second DFS on reverse graph
        visited.Clear();
        var components = new List<IReadOnlyList<TNode>>();

        void Dfs2(TNode node, List<TNode> component)
        {
            visited.Add(node);
            component.Add(node);
            foreach (var predecessor in graph.GetPredecessors(node))
            {
                if (!visited.Contains(predecessor))
                {
                    Dfs2(predecessor, component);
                }
            }
        }

        while (finishOrder.Count > 0)
        {
            var node = finishOrder.Pop();
            if (!visited.Contains(node))
            {
                var component = new List<TNode>();
                Dfs2(node, component);
                components.Add(component);
            }
        }

        return components;
    }

    /// <summary>
    /// Computes the dominator tree for the graph.
    /// </summary>
    /// <typeparam name="TNode">Node type.</typeparam>
    /// <typeparam name="TLabel">Edge label type.</typeparam>
    /// <param name="graph">The graph to analyze.</param>
    /// <param name="entry">The entry node.</param>
    /// <returns>Dictionary mapping each node to its immediate dominator.</returns>
    public static IReadOnlyDictionary<TNode, TNode?> GetDominatorTree<TNode, TLabel>(
        this DirectedGraph<TNode, TLabel> graph,
        TNode entry)
        where TNode : notnull
    {
        var dominators = new Dictionary<TNode, TNode?>();
        var reachable = graph.GetReachableNodes(entry);

        // Initialize: entry dominates itself, others undefined
        foreach (var node in reachable)
        {
            dominators[node] = node.Equals(entry) ? default : default;
        }
        dominators[entry] = entry;

        // Iterative algorithm
        bool changed;
        do
        {
            changed = false;
            foreach (var node in reachable)
            {
                if (node.Equals(entry))
                    continue;

                var predecessors = graph.GetPredecessors(node)
                    .Where(p => dominators.ContainsKey(p) && dominators[p] != null)
                    .ToList();

                if (predecessors.Count == 0)
                    continue;

                var newDom = predecessors[0];
                for (int i = 1; i < predecessors.Count; i++)
                {
                    newDom = Intersect(newDom, predecessors[i], dominators);
                }

                if (!Equals(dominators[node], newDom))
                {
                    dominators[node] = newDom;
                    changed = true;
                }
            }
        } while (changed);

        return dominators;
    }

    private static TNode Intersect<TNode>(TNode b1, TNode b2, Dictionary<TNode, TNode?> dominators)
        where TNode : notnull
    {
        var finger1 = b1;
        var finger2 = b2;

        while (!finger1.Equals(finger2))
        {
            // Simple heuristic - use hash code ordering
            while (finger1.GetHashCode() < finger2.GetHashCode())
            {
                if (dominators[finger1] == null || dominators[finger1]!.Equals(finger1))
                    return finger1;
                finger1 = dominators[finger1]!;
            }
            while (finger2.GetHashCode() < finger1.GetHashCode())
            {
                if (dominators[finger2] == null || dominators[finger2]!.Equals(finger2))
                    return finger2;
                finger2 = dominators[finger2]!;
            }
        }

        return finger1;
    }
}
