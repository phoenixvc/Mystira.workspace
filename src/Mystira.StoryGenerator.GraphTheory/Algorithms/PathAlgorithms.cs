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
        var stack = new Stack<(TNode Node, int ChildIndex)>();

        path.Add(start);
        stack.Push((start, 0));

        while (stack.Count > 0)
        {
            var (node, _) = stack.Pop();

            // Fix path length to match stack depth+1
            while (path.Count > stack.Count + 1)
                path.RemoveAt(path.Count - 1);

            path[path.Count - 1] = node;

            if (isTerminal(node) || (maxDepth.HasValue && path.Count >= maxDepth.Value))
            {
                yield return path.ToArray();
                continue;
            }

            var successors = graph.GetSuccessors(node).ToArray();
            if (successors.Length == 0)
            {
                yield return path.ToArray();
                continue;
            }

            // Push children in reverse to preserve natural order
            for (int i = successors.Length - 1; i >= 0; i--)
            {
                var child = successors[i];
                path.Add(child);
                stack.Push((child, 0));
            }
        }
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
            return Array.Empty<IReadOnlyList<TNode>>();

        // How much of each path we ultimately keep (prefix length).
        var keepLength = new int[paths.Count];
        for (int i = 0; i < paths.Count; i++)
        {
            keepLength[i] = paths[i].Count;
        }

        var trieRoot = new TrieNode<TNode>();

        // Process each path once, from end to start.
        for (int p = 0; p < paths.Count; p++)
        {
            var path = paths[p];
            int L = path.Count;

            var node = trieRoot;

            // Walk suffixes backwards: path[i..L)
            for (int i = L - 1; i >= 0; i--)
            {
                var symbol = path[i];

                if (!node.Children.TryGetValue(symbol, out var child))
                {
                    child = new TrieNode<TNode>();
                    node.Children[symbol] = child;
                }

                node = child;

                if (node.OwnerPathIndex == -1)
                {
                    // First time this suffix is seen: current path owns it.
                    node.OwnerPathIndex = p;
                }
                else if (node.OwnerPathIndex != p)
                {
                    // This suffix is already owned by another path.
                    // We can cut current path before this suffix.
                    var newLen = i + 1; // include join node at i
                    if (newLen < keepLength[p])
                    {
                        keepLength[p] = newLen;
                    }
                }
            }
        }

        // Build final trimmed paths, removing duplicate prefixes.
        var result = new List<IReadOnlyList<TNode>>();
        var seenPrefixes = new HashSet<PrefixKey<TNode>>();

        for (int p = 0; p < paths.Count; p++)
        {
            var original = paths[p];
            int len = keepLength[p];

            if (len <= 0)
                continue;

            var prefix = new TNode[len];
            for (int i = 0; i < len; i++)
                prefix[i] = original[i];

            var key = new PrefixKey<TNode>(prefix);
            if (seenPrefixes.Add(key))
            {
                result.Add(prefix);
            }
        }

        return new ReadOnlyCollection<IReadOnlyList<TNode>>(result);
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
            for (int i = 0; i < _nodes.Length; i++)
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
                int hash = 17;
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
