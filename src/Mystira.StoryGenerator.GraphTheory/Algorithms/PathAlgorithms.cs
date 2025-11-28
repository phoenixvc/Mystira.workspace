using System.Collections.ObjectModel;
using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.GraphTheory.Algorithms;

public static class PathAlgorithms
{
    /// <summary>
    /// <para>
    /// Enumerates simple paths from a given start node to terminal nodes,
    /// using a depth-first strategy.
    /// </para>
    /// <para>
    /// A node is considered terminal if the <paramref name="isTerminal"/> predicate
    /// returns <c>true</c>, or, by default, if it has no outgoing edges.
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
    /// The directed graph to explore for paths.
    /// </para>
    /// </param>
    /// <param name="start">
    /// <para>
    /// The starting node for path enumeration.
    /// </para>
    /// </param>
    /// <param name="isTerminal">
    /// <para>
    /// Optional predicate that determines whether a node should be treated
    /// as terminal. If omitted, a node with no outgoing edges is terminal.
    /// </para>
    /// </param>
    /// <param name="maxDepth">
    /// <para>
    /// Optional maximum path length. When provided, paths are truncated once
    /// they reach this length and yielded as-is.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A sequence of paths, where each path is represented as a list of nodes
    /// from the start node to a terminal node (or truncated by <paramref name="maxDepth"/>).
    /// </para>
    /// </returns>
    public static IEnumerable<IReadOnlyList<TNode>> EnumeratePaths<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph,
        TNode start,
        Func<TNode, bool>? isTerminal = null,
        int? maxDepth = null)
        where TNode : notnull
    {
        isTerminal ??= node => graph.OutDegree(node) == 0;

        var path = new List<TNode>();
        var stack = new Stack<(TNode Node, IEnumerator<TNode> Succ, int Depth)>();

        // Initialize with root
        path.Add(start);
        var rootSucc = graph.GetSuccessors(start).GetEnumerator();
        stack.Push((start, rootSucc, depth: 0));

        while (stack.Count > 0)
        {
            var (node, succEnum, depth) = stack.Peek();

            bool depthLimitReached = maxDepth.HasValue && depth >= maxDepth.Value;
            bool isTerm = depthLimitReached || isTerminal(node);

            if (isTerm)
            {
                // Current node is terminal (or depth limit): emit current path.
                yield return path.ToArray();

                // Backtrack
                stack.Pop();
                succEnum.Dispose();
                path.RemoveAt(path.Count - 1);

                continue;
            }

            // Try to advance to the next successor
            if (!succEnum.MoveNext())
            {
                // No more successors: just backtrack without emitting; this node is not terminal.
                stack.Pop();
                succEnum.Dispose();
                path.RemoveAt(path.Count - 1);
                continue;
            }

            // Go deeper to the child
            var child = succEnum.Current;
            path.Add(child);
            var childSucc = graph.GetSuccessors(child).GetEnumerator();
            stack.Push((child, childSucc, depth + 1));
        }

        yield break;
    }

    /// <summary>
    /// <para>
    /// Compresses a set of paths by removing redundant repeated suffixes.
    /// </para>
    /// <para>
    /// For each group of paths that share an identical suffix (same node
    /// sequence from some position to the end), this method:
    /// - Keeps one path with that full suffix,
    /// - Truncates all other paths at the node just before the shared suffix.
    /// </para>
    /// <para>
    /// Example:
    /// - Paths: (v1,v2,v4,v5) and (v1,v3,v4,v5)
    /// - Shared suffix: (v4,v5)
    /// - Result: (v1,v2,v4,v5), (v1,v3,v4)
    /// </para>
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<TNode>> CompressBySharedSuffixes<TNode>(
        IReadOnlyList<IReadOnlyList<TNode>> paths)
        where TNode : notnull
    {
        if (paths.Count == 0)
            return [];

        // How much of each path we ultimately keep (prefix length).
        var keepLength = new int[paths.Count];
        for (var i = 0; i < paths.Count; i++)
        {
            keepLength[i] = paths[i].Count;
        }

        var trieRoot = new TrieNode<TNode>();

        // Process each path once, from end to start.
        for (var p = 0; p < paths.Count; p++)
        {
            var path = paths[p];
            var l = path.Count;

            var node = trieRoot;
            var matchedDepth = 0; // how many symbols of the suffix have matched so far (from the end)

            // Walk suffixes backwards: path[i..L)
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
                    // First time this suffix is seen: current path owns it.
                    node.OwnerPathIndex = p;
                }
                else if (node.OwnerPathIndex != p)
                {
                    // This suffix is already owned by another path.
                    // Only truncate if we've matched at least two symbols of the suffix
                    // (i.e., there is an actual shared tail, not just the starting node).
                    if (matchedDepth >= 2)
                    {
                        var newLen = i + 1; // include join node at i
                        if (newLen < keepLength[p])
                        {
                            keepLength[p] = newLen;
                        }
                    }
                }
            }
        }

        // Build final trimmed paths, removing duplicate prefixes.
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
    /// <para>
    /// Enumerates all root→terminal paths in the given directed graph,
    /// compresses them by factoring out shared suffixes, and returns the
    /// compressed paths as lists of edges.
    /// </para>
    /// <para>
    /// Behavior:
    /// </para>
    /// <para>
    /// 1. All root→terminal node paths starting at <paramref name="root"/>
    ///    are enumerated using
    ///    <see cref="PathAlgorithms.EnumeratePaths{TNode, TEdgeLabel}(DirectedGraph{TNode, TEdgeLabel}, TNode, System.Func{TNode, bool}?, int?)"/>.
    /// </para>
    /// <para>
    /// 2. These node paths are compressed with
    ///    <see cref="CompressBySharedSuffixes{TNode}(IReadOnlyList{IReadOnlyList{TNode}})"/>,
    ///    which:
    ///    - Keeps one path carrying each shared suffix in full,
    ///    - Truncates other paths that share that suffix just before it begins.
    /// </para>
    /// <para>
    /// 3. Each compressed node path is converted back into a path of
    ///    <see cref="Edge{TNode, TEdgeLabel}"/> instances using the original
    ///    <paramref name="graph"/>.
    /// </para>
    /// <para>
    /// The number of returned edge paths always matches the number of
    /// compressed node paths produced in step 2.
    /// </para>
    /// </summary>
    /// <typeparam name="TNode">
    /// <para>
    /// The node identifier type (for example <see cref="string"/> or a
    /// record type such as <see cref="FrontierMergedGraph.SceneStateNode{TSceneId, TStateSig}"/>).
    /// </para>
    /// </typeparam>
    /// <typeparam name="TEdgeLabel">
    /// <para>
    /// The type of labels attached to edges in the graph.
    /// </para>
    /// </typeparam>
    /// <param name="graph">
    /// <para>
    /// The directed graph whose paths are to be compressed.
    /// </para>
    /// </param>
    /// <param name="root">
    /// <para>
    /// The root node from which root→terminal paths are enumerated.
    /// </para>
    /// </param>
    /// <param name="isTerminal">
    /// <para>
    /// Optional predicate that determines whether a node is considered
    /// terminal. If omitted, any node with no outgoing edges is treated as
    /// terminal.
    /// </para>
    /// </param>
    /// <param name="maxDepth">
    /// <para>
    /// Optional maximum depth for path enumeration. If provided, paths are
    /// truncated at this length and treated as terminal at that point.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A read-only list of compressed paths, where each path is represented
    /// as a list of <see cref="Edge{TNode, TEdgeLabel}"/> from the root to
    /// its terminal node (or to the truncation depth).
    /// </para>
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <para>
    /// Thrown if, for any adjacent pair of nodes in a compressed node path,
    /// there is no matching edge in <paramref name="graph"/> whose
    /// <see cref="Edge{TNode, TEdgeLabel}.From"/> and
    /// <see cref="Edge{TNode, TEdgeLabel}.To"/> correspond to that pair.
    /// </para>
    /// </exception>
    public static IReadOnlyList<IReadOnlyList<Edge<TNode, TEdgeLabel>>> CompressGraphPathsToEdgePaths<TNode, TEdgeLabel>(
        this DirectedGraph<TNode, TEdgeLabel> graph,
        TNode root,
        Func<TNode, bool>? isTerminal = null,
        int? maxDepth = null)
        where TNode : notnull
    {
        isTerminal ??= node => graph.OutDegree(node) == 0;

        // 1) Enumerate all root→terminal paths as node sequences.
        var allNodePaths = graph
            .EnumeratePaths(
                start: root,
                isTerminal: isTerminal,
                maxDepth: maxDepth)
            .ToList();

        // 2) Compress node paths by shared suffixes.
        var compressedNodePaths = CompressBySharedSuffixes(allNodePaths);

        // 3) Convert each compressed node path into a list of edges.
        var result = new List<IReadOnlyList<Edge<TNode, TEdgeLabel>>>(compressedNodePaths.Count);

        foreach (var nodePath in compressedNodePaths)
        {
            if (nodePath.Count < 2)
            {
                // No edges in this path (singleton root, or degenerate).
                result.Add(Array.Empty<Edge<TNode, TEdgeLabel>>());
                continue;
            }

            var edgePath = new List<Edge<TNode, TEdgeLabel>>(nodePath.Count - 1);

            for (var i = 0; i < nodePath.Count - 1; i++)
            {
                var from = nodePath[i];
                var to   = nodePath[i + 1];

                Edge<TNode, TEdgeLabel>? chosen = null;

                foreach (var e in graph.GetOutgoingEdges(from))
                {
                    if (EqualityComparer<TNode>.Default.Equals(e.To, to))
                    {
                        chosen = e;
                        break; // assume at most one edge per (from,to)
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

        return new ReadOnlyCollection<IReadOnlyList<Edge<TNode, TEdgeLabel>>>(result);
    }

    private sealed class TrieNode<TNode>
        where TNode : notnull
    {
        public Dictionary<TNode, TrieNode<TNode>> Children { get; } = new();
        public int OwnerPathIndex { get; set; } = -1; // -1 = none
    }

    private readonly struct PrefixKey<TNode> : IEquatable<PrefixKey<TNode>>
        where TNode : notnull
    {
        private readonly TNode[] _nodes;

        public PrefixKey(TNode[] nodes)
        {
            _nodes = nodes;
        }

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

        public override bool Equals(object? obj)
            => obj is PrefixKey<TNode> other && Equals(other);

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
