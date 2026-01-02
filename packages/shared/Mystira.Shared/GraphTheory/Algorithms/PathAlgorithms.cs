using System.Collections.ObjectModel;

namespace Mystira.Shared.GraphTheory.Algorithms;

/// <summary>
/// Path enumeration and compression algorithms for directed graphs.
/// </summary>
public static class PathAlgorithms
{
    /// <summary>
    /// Enumerates simple paths from a given start node to terminal nodes using a depth-first strategy.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <typeparam name="TEdgeLabel">The edge label type.</typeparam>
    /// <param name="graph">The directed graph to explore.</param>
    /// <param name="start">The starting node for path enumeration.</param>
    /// <param name="isTerminal">Optional predicate for terminal nodes. Defaults to nodes with no outgoing edges.</param>
    /// <param name="maxDepth">Optional maximum path length.</param>
    /// <returns>A sequence of paths, each represented as a list of nodes.</returns>
    public static IEnumerable<IReadOnlyList<TNode>> EnumeratePaths<TNode, TEdgeLabel>(
        this IDirectedGraph<TNode, TEdgeLabel> graph,
        TNode start,
        Func<TNode, bool>? isTerminal = null,
        int? maxDepth = null)
        where TNode : notnull
    {
        isTerminal ??= node => graph.OutDegree(node) == 0;

        var path = new List<TNode>();
        var stack = new Stack<(TNode Node, IEnumerator<TNode> Succ, int Depth)>();

        path.Add(start);
        var rootSucc = graph.GetSuccessors(start).GetEnumerator();
        stack.Push((start, rootSucc, 0));

        while (stack.Count > 0)
        {
            var (node, succEnum, depth) = stack.Peek();

            bool depthLimitReached = maxDepth.HasValue && depth >= maxDepth.Value;
            bool isTerm = depthLimitReached || isTerminal(node);

            if (isTerm)
            {
                yield return path.ToArray();
                stack.Pop();
                succEnum.Dispose();
                path.RemoveAt(path.Count - 1);
                continue;
            }

            if (!succEnum.MoveNext())
            {
                stack.Pop();
                succEnum.Dispose();
                path.RemoveAt(path.Count - 1);
                continue;
            }

            var child = succEnum.Current;
            path.Add(child);
            var childSucc = graph.GetSuccessors(child).GetEnumerator();
            stack.Push((child, childSucc, depth + 1));
        }
    }

    /// <summary>
    /// Compresses a set of paths by removing redundant repeated suffixes.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <param name="paths">The paths to compress.</param>
    /// <returns>Compressed paths with shared suffixes factored out.</returns>
    public static IReadOnlyList<IReadOnlyList<TNode>> CompressBySharedSuffixes<TNode>(
        IReadOnlyList<IReadOnlyList<TNode>> paths)
        where TNode : notnull
    {
        if (paths.Count == 0)
            return [];

        var keepLength = new int[paths.Count];
        for (var i = 0; i < paths.Count; i++)
        {
            keepLength[i] = paths[i].Count;
        }

        var trieRoot = new TrieNode<TNode>();

        for (var p = 0; p < paths.Count; p++)
        {
            var path = paths[p];
            var l = path.Count;
            var node = trieRoot;
            var matchedDepth = 0;

            for (var i = l - 1; i >= 0; i--)
            {
                var symbol = path[i];

                if (!node.Children.TryGetValue(symbol, out var child))
                {
                    child = new TrieNode<TNode>();
                    node.Children[symbol] = child;
                }

                node = child;
                matchedDepth++;

                if (node.OwnerPathIndex == -1)
                {
                    node.OwnerPathIndex = p;
                }
                else if (node.OwnerPathIndex != p)
                {
                    if (matchedDepth >= 2 && matchedDepth < l)
                    {
                        var ownerIdx = node.OwnerPathIndex;
                        bool samePrefixUpToJoin = true;
                        for (var k = 0; k <= i; k++)
                        {
                            if (!EqualityComparer<TNode>.Default.Equals(paths[p][k], paths[ownerIdx][k]))
                            {
                                samePrefixUpToJoin = false;
                                break;
                            }
                        }

                        if (!samePrefixUpToJoin)
                        {
                            var newLen = i + 1;
                            if (newLen < keepLength[p])
                            {
                                keepLength[p] = newLen;
                            }
                        }
                    }
                }
            }
        }

        var result = new List<IReadOnlyList<TNode>>();
        var seenPrefixes = new HashSet<PrefixKey<TNode>>();

        for (var p = 0; p < paths.Count; p++)
        {
            var original = paths[p];
            var len = keepLength[p];

            if (len <= 0)
                continue;

            var prefix = new TNode[len];
            for (var i = 0; i < len; i++)
                prefix[i] = original[i];

            var key = new PrefixKey<TNode>(prefix);
            if (seenPrefixes.Add(key))
            {
                result.Add(prefix);
            }
        }

        return new ReadOnlyCollection<IReadOnlyList<TNode>>(result);
    }

    /// <summary>
    /// Enumerates all root→terminal paths, compresses them by shared suffixes, and returns edge paths.
    /// </summary>
    /// <typeparam name="TNode">The node identifier type.</typeparam>
    /// <typeparam name="TEdgeLabel">The edge label type.</typeparam>
    /// <param name="graph">The directed graph.</param>
    /// <param name="root">The root node.</param>
    /// <param name="isTerminal">Optional terminal predicate.</param>
    /// <param name="maxDepth">Optional maximum depth.</param>
    /// <returns>Compressed paths as lists of edges.</returns>
    public static IReadOnlyList<IReadOnlyList<IEdge<TNode, TEdgeLabel>>> CompressGraphPathsToEdgePaths<TNode, TEdgeLabel>(
        this IDirectedGraph<TNode, TEdgeLabel> graph,
        TNode root,
        Func<TNode, bool>? isTerminal = null,
        int? maxDepth = null)
        where TNode : notnull
    {
        isTerminal ??= node => graph.OutDegree(node) == 0;

        var allNodePaths = graph
            .EnumeratePaths(start: root, isTerminal: isTerminal, maxDepth: maxDepth)
            .ToList();

        var compressedNodePaths = CompressBySharedSuffixes(allNodePaths);

        var result = new List<IReadOnlyList<IEdge<TNode, TEdgeLabel>>>(compressedNodePaths.Count);

        foreach (var nodePath in compressedNodePaths)
        {
            if (nodePath.Count < 2)
            {
                result.Add(Array.Empty<IEdge<TNode, TEdgeLabel>>());
                continue;
            }

            var edgePath = new List<IEdge<TNode, TEdgeLabel>>(nodePath.Count - 1);

            for (var i = 0; i < nodePath.Count - 1; i++)
            {
                var from = nodePath[i];
                var to = nodePath[i + 1];

                IEdge<TNode, TEdgeLabel>? chosen = null;

                foreach (var e in graph.GetOutgoingEdges(from))
                {
                    if (EqualityComparer<TNode>.Default.Equals(e.To, to))
                    {
                        chosen = e;
                        break;
                    }
                }

                if (chosen is null)
                {
                    throw new InvalidOperationException(
                        $"No edge found from '{from}' to '{to}' when reconstructing edge path.");
                }

                edgePath.Add(chosen);
            }

            result.Add(edgePath);
        }

        return new ReadOnlyCollection<IReadOnlyList<IEdge<TNode, TEdgeLabel>>>(result);
    }

    /// <summary>
    /// Backwards-compatibility overload for DirectedGraph.
    /// </summary>
    public static IEnumerable<IReadOnlyList<TNode>> EnumeratePaths<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph,
        TNode start,
        Func<TNode, bool>? isTerminal = null,
        int? maxDepth = null)
        where TNode : notnull
        => EnumeratePaths<TNode, TEdgeLabel>((IDirectedGraph<TNode, TEdgeLabel>)graph, start, isTerminal, maxDepth);

    /// <summary>
    /// Backwards-compatibility overload for DirectedGraph with concrete Edge return type.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<Edge<TNode, TEdgeLabel>>> CompressGraphPathsToEdgePaths<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph,
        TNode root,
        Func<TNode, bool>? isTerminal = null,
        int? maxDepth = null)
        where TNode : notnull
    {
        var paths = CompressGraphPathsToEdgePaths<TNode, TEdgeLabel>((IDirectedGraph<TNode, TEdgeLabel>)graph, root, isTerminal, maxDepth);
        return new ReadOnlyCollection<IReadOnlyList<Edge<TNode, TEdgeLabel>>>(
            paths.Select(list => (IReadOnlyList<Edge<TNode, TEdgeLabel>>)list.Cast<Edge<TNode, TEdgeLabel>>().ToList()).ToList());
    }

    private sealed class TrieNode<TNode>
        where TNode : notnull
    {
        public Dictionary<TNode, TrieNode<TNode>> Children { get; } = new();
        public int OwnerPathIndex { get; set; } = -1;
    }

    private readonly struct PrefixKey<TNode> : IEquatable<PrefixKey<TNode>>
        where TNode : notnull
    {
        private readonly TNode[] _nodes;

        public PrefixKey(TNode[] nodes) => _nodes = nodes;

        public bool Equals(PrefixKey<TNode> other)
        {
            if (_nodes == null && other._nodes == null) return true;
            if (_nodes == null || other._nodes == null) return false;
            if (_nodes.Length != other._nodes.Length) return false;
            var cmp = EqualityComparer<TNode>.Default;
            for (var i = 0; i < _nodes.Length; i++)
            {
                if (!cmp.Equals(_nodes[i], other._nodes[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is PrefixKey<TNode> other && Equals(other);

        public override int GetHashCode()
        {
            if (_nodes == null) return 0;
            unchecked
            {
                var hash = 17;
                var cmp = EqualityComparer<TNode>.Default;
                foreach (var n in _nodes)
                {
                    hash = hash * 31 + cmp.GetHashCode(n);
                }
                return hash;
            }
        }
    }
}
