using System.Collections.ObjectModel;

namespace Mystira.Shared.GraphTheory;

/// <summary>
/// <para>
/// Immutable, directed graph over a set of nodes and labelled edges.
/// </para>
/// <para>
/// The graph is constructed from a collection of edges (and optionally an
/// explicit node set) via <see cref="FromEdges"/> and then treated as a
/// read-only, mathematical object.
/// </para>
/// </summary>
/// <typeparam name="TNode">
/// The type used to identify nodes in the graph. Must support equality.
/// </typeparam>
/// <typeparam name="TEdgeLabel">
/// The type of labels attached to edges.
/// </typeparam>
public sealed class DirectedGraph<TNode, TEdgeLabel> : IDirectedGraph<TNode, TEdgeLabel>
    where TNode : notnull
{
    private readonly IReadOnlyDictionary<TNode, IReadOnlyList<IEdge<TNode, TEdgeLabel>>> _outgoing;
    private readonly IReadOnlyDictionary<TNode, IReadOnlyList<IEdge<TNode, TEdgeLabel>>> _incoming;

    /// <inheritdoc />
    public IReadOnlyCollection<TNode> Nodes { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<IEdge<TNode, TEdgeLabel>> Edges { get; }

    private DirectedGraph(
        IReadOnlyCollection<TNode> nodes,
        IReadOnlyCollection<IEdge<TNode, TEdgeLabel>> edges,
        IReadOnlyDictionary<TNode, IReadOnlyList<IEdge<TNode, TEdgeLabel>>> outgoing,
        IReadOnlyDictionary<TNode, IReadOnlyList<IEdge<TNode, TEdgeLabel>>> incoming)
    {
        Nodes = nodes;
        Edges = edges;
        _outgoing = outgoing;
        _incoming = incoming;
    }

    /// <summary>
    /// Constructs a directed graph from a collection of edges and an optional explicit node set.
    /// </summary>
    /// <param name="edges">The collection of directed edges defining the graph structure.</param>
    /// <param name="nodes">Optional explicit sequence of nodes to include.</param>
    /// <param name="comparer">Optional equality comparer for nodes.</param>
    /// <returns>A new DirectedGraph instance.</returns>
    public static DirectedGraph<TNode, TEdgeLabel> FromEdges(
        IEnumerable<Edge<TNode, TEdgeLabel>> edges,
        IEnumerable<TNode>? nodes = null,
        IEqualityComparer<TNode>? comparer = null)
    {
        comparer ??= EqualityComparer<TNode>.Default;

        var edgeList = edges.ToList();
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

        var outgoing = new Dictionary<TNode, List<IEdge<TNode, TEdgeLabel>>>(comparer);
        var incoming = new Dictionary<TNode, List<IEdge<TNode, TEdgeLabel>>>(comparer);

        foreach (var n in nodeSet)
        {
            outgoing[n] = new List<IEdge<TNode, TEdgeLabel>>();
            incoming[n] = new List<IEdge<TNode, TEdgeLabel>>();
        }

        foreach (var e in edgeList)
        {
            outgoing[e.From].Add(e);
            incoming[e.To].Add(e);
        }

        var roNodes = new ReadOnlyCollection<TNode>(nodeSet.ToList());
        var roEdges = new ReadOnlyCollection<IEdge<TNode, TEdgeLabel>>(edgeList.Cast<IEdge<TNode, TEdgeLabel>>().ToList());

        var roOutgoing = outgoing.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<IEdge<TNode, TEdgeLabel>>)new ReadOnlyCollection<IEdge<TNode, TEdgeLabel>>(kvp.Value),
            comparer);

        var roIncoming = incoming.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<IEdge<TNode, TEdgeLabel>>)new ReadOnlyCollection<IEdge<TNode, TEdgeLabel>>(kvp.Value),
            comparer);

        return new DirectedGraph<TNode, TEdgeLabel>(roNodes, roEdges, roOutgoing, roIncoming);
    }

    /// <inheritdoc />
    public IReadOnlyList<IEdge<TNode, TEdgeLabel>> GetOutgoingEdges(TNode node)
        => _outgoing.TryGetValue(node, out var list) ? list : Array.Empty<IEdge<TNode, TEdgeLabel>>();

    /// <inheritdoc />
    public IReadOnlyList<IEdge<TNode, TEdgeLabel>> GetIncomingEdges(TNode node)
        => _incoming.TryGetValue(node, out var list) ? list : Array.Empty<IEdge<TNode, TEdgeLabel>>();

    /// <inheritdoc />
    public IEnumerable<TNode> GetSuccessors(TNode node)
        => GetOutgoingEdges(node).Select(e => e.To);

    /// <inheritdoc />
    public IEnumerable<TNode> GetPredecessors(TNode node)
        => GetIncomingEdges(node).Select(e => e.From);

    /// <inheritdoc />
    public int OutDegree(TNode node) => GetOutgoingEdges(node).Count;

    /// <inheritdoc />
    public int InDegree(TNode node) => GetIncomingEdges(node).Count;

    /// <inheritdoc />
    public IEnumerable<TNode> Roots() => Nodes.Where(n => InDegree(n) == 0);

    /// <inheritdoc />
    public IEnumerable<TNode> Terminals() => Nodes.Where(n => OutDegree(n) == 0);
}
