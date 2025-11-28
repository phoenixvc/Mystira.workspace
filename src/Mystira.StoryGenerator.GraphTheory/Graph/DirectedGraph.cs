using System.Collections.ObjectModel;

namespace Mystira.StoryGenerator.GraphTheory.Graph;

/// <summary>
/// <para>
/// Immutable, directed graph over a set of nodes and labelled edges.
/// </para>
/// <para>
/// The graph is constructed from a collection of edges (and optionally an
/// explicit node set) via <see cref="FromEdges"/> and then treated as a
/// read-only, mathematical object. All traversal and analysis is done by
/// external algorithms in <see cref="GraphAlgorithms"/>.
/// </para>
/// </summary>
/// <typeparam name="TNode">
/// <para>
/// The type used to identify nodes in the graph (for example, string or GUID).
/// Must support equality, as it is used as a dictionary key.
/// </para>
/// </typeparam>
/// <typeparam name="TEdgeLabel">
/// <para>
/// The type of labels attached to edges. This can be domain-specific metadata
/// or a placeholder type if no label content is required.
/// </para>
/// </typeparam>
public sealed class DirectedGraph<TNode, TEdgeLabel>
    where TNode : notnull
{
    private readonly IReadOnlyDictionary<TNode, IReadOnlyList<Edge<TNode, TEdgeLabel>>> _outgoing;
    private readonly IReadOnlyDictionary<TNode, IReadOnlyList<Edge<TNode, TEdgeLabel>>> _incoming;

    /// <summary>
    /// <para>
    /// All nodes present in the graph.
    /// </para>
    /// </summary>
    public IReadOnlyCollection<TNode> Nodes { get; }

    /// <summary>
    /// <para>
    /// All directed edges present in the graph.
    /// </para>
    /// </summary>
    public IReadOnlyCollection<Edge<TNode, TEdgeLabel>> Edges { get; }

    private DirectedGraph(
        IReadOnlyCollection<TNode> nodes,
        IReadOnlyCollection<Edge<TNode, TEdgeLabel>> edges,
        IReadOnlyDictionary<TNode, IReadOnlyList<Edge<TNode, TEdgeLabel>>> outgoing,
        IReadOnlyDictionary<TNode, IReadOnlyList<Edge<TNode, TEdgeLabel>>> incoming)
    {
        Nodes = nodes;
        Edges = edges;
        _outgoing = outgoing;
        _incoming = incoming;
    }

    /// <summary>
    /// <para>
    /// Constructs a directed graph from a collection of edges and an optional
    /// explicit node set.
    /// </para>
    /// <para>
    /// If <paramref name="nodes"/> is provided, all nodes in that sequence
    /// are included, even if they have no incident edges. Any node appearing
    /// as <c>From</c> or <c>To</c> in <paramref name="edges"/> is also added.
    /// </para>
    /// <para>
    /// The resulting graph is immutable: its node and edge sets are materialised
    /// and stored internally in read-only collections.
    /// </para>
    /// </summary>
    /// <param name="edges">
    /// <para>
    /// The collection of directed edges defining the graph structure.
    /// </para>
    /// </param>
    /// <param name="nodes">
    /// <para>
    /// Optional explicit sequence of nodes to include, in addition to those
    /// inferred from the edge endpoints.
    /// </para>
    /// </param>
    /// <param name="comparer">
    /// <para>
    /// Optional equality comparer for <typeparamref name="TNode"/>. If omitted,
    /// <see cref="EqualityComparer{T}.Default"/> is used.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A new <see cref="DirectedGraph{TNode, TEdgeLabel}"/> instance containing
    /// the specified nodes and edges, with adjacency information precomputed.
    /// </para>
    /// </returns>
    public static DirectedGraph<TNode, TEdgeLabel> FromEdges(
        IEnumerable<Edge<TNode, TEdgeLabel>> edges,
        IEnumerable<TNode>? nodes = null,
        IEqualityComparer<TNode>? comparer = null)
    {
        comparer ??= EqualityComparer<TNode>.Default;

        // Materialize edges
        var edgeList = edges.ToList();

        // Build node set
        var nodeSet = new HashSet<TNode>(comparer);
        if (nodes != null)
        {
            foreach (var n in nodes)
                nodeSet.Add(n);
        }

        foreach (var e in edgeList)
        {
            nodeSet.Add(e.From);
            nodeSet.Add(e.To);
        }

        // Build adjacency dictionaries
        var outgoing = new Dictionary<TNode, List<Edge<TNode, TEdgeLabel>>>(comparer);
        var incoming = new Dictionary<TNode, List<Edge<TNode, TEdgeLabel>>>(comparer);

        foreach (var n in nodeSet)
        {
            outgoing[n] = new List<Edge<TNode, TEdgeLabel>>();
            incoming[n] = new List<Edge<TNode, TEdgeLabel>>();
        }

        foreach (var e in edgeList)
        {
            outgoing[e.From].Add(e);
            incoming[e.To].Add(e);
        }

        // Wrap in read-only
        var roNodes = new ReadOnlyCollection<TNode>(nodeSet.ToList());
        var roEdges = new ReadOnlyCollection<Edge<TNode, TEdgeLabel>>(edgeList);

        var roOutgoing = outgoing.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<Edge<TNode, TEdgeLabel>>)new ReadOnlyCollection<Edge<TNode, TEdgeLabel>>(kvp.Value),
            comparer);

        var roIncoming = incoming.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<Edge<TNode, TEdgeLabel>>)new ReadOnlyCollection<Edge<TNode, TEdgeLabel>>(kvp.Value),
            comparer);

        return new DirectedGraph<TNode, TEdgeLabel>(
            roNodes,
            roEdges,
            roOutgoing,
            roIncoming);
    }

    /// <summary>
    /// <para>
    /// Gets the outgoing edges for the specified node.
    /// </para>
    /// <para>
    /// If the node is not present in the graph, an empty sequence is returned.
    /// </para>
    /// </summary>
    /// <param name="node">
    /// <para>
    /// The node whose outgoing edges are requested.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A read-only list of edges whose <see cref="Edge{TNode, TEdgeLabel}.From"/>
    /// equals <paramref name="node"/>.
    /// </para>
    /// </returns>
    public IReadOnlyList<Edge<TNode, TEdgeLabel>> GetOutgoingEdges(TNode node)
        => _outgoing.TryGetValue(node, out var list)
            ? list
            : Array.Empty<Edge<TNode, TEdgeLabel>>();

    /// <summary>
    /// <para>
    /// Gets the incoming edges for the specified node.
    /// </para>
    /// <para>
    /// If the node is not present in the graph, an empty sequence is returned.
    /// </para>
    /// </summary>
    /// <param name="node">
    /// <para>
    /// The node whose incoming edges are requested.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A read-only list of edges whose <see cref="Edge{TNode, TEdgeLabel}.To"/>
    /// equals <paramref name="node"/>.
    /// </para>
    /// </returns>
    public IReadOnlyList<Edge<TNode, TEdgeLabel>> GetIncomingEdges(TNode node)
        => _incoming.TryGetValue(node, out var list)
            ? list
            : Array.Empty<Edge<TNode, TEdgeLabel>>();

    /// <summary>
    /// <para>
    /// Gets the direct successor nodes of the specified node, based on outgoing edges.
    /// </para>
    /// </summary>
    /// <param name="node">
    /// <para>
    /// The node whose successors are requested.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A sequence of nodes that are direct targets of outgoing edges from
    /// <paramref name="node"/>.
    /// </para>
    /// </returns>
    public IEnumerable<TNode> GetSuccessors(TNode node)
        => GetOutgoingEdges(node).Select(e => e.To);


    /// <summary>
    /// <para>
    /// Gets the direct predecessor nodes of the specified node, based on incoming edges.
    /// </para>
    /// </summary>
    /// <param name="node">
    /// <para>
    /// The node whose predecessors are requested.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A sequence of nodes that have outgoing edges leading to
    /// <paramref name="node"/>.
    /// </para>
    /// </returns>
    public IEnumerable<TNode> GetPredecessors(TNode node)
        => GetIncomingEdges(node).Select(e => e.From);

    /// <summary>
    /// <para>
    /// Returns the out-degree of the specified node (number of outgoing edges).
    /// </para>
    /// </summary>
    public int OutDegree(TNode node) => GetOutgoingEdges(node).Count;

    /// <summary>
    /// <para>
    /// Returns the in-degree of the specified node (number of incoming edges).
    /// </para>
    /// </summary>
    public int InDegree(TNode node)  => GetIncomingEdges(node).Count;

    /// <summary>
    /// <para>
    /// Returns all nodes with in-degree zero (no incoming edges).
    /// </para>
    /// <para>
    /// In many applications, these represent root or entry nodes of the graph.
    /// </para>
    /// </summary>
    public IEnumerable<TNode> Roots()
        => Nodes.Where(n => InDegree(n) == 0);


    /// <summary>
    /// <para>
    /// Returns all nodes with out-degree zero (no outgoing edges).
    /// </para>
    /// <para>
    /// In many applications, these represent terminal or exit nodes of the graph.
    /// </para>
    /// </summary>
    public IEnumerable<TNode> Terminals()
        => Nodes.Where(n => OutDegree(n) == 0);
}
