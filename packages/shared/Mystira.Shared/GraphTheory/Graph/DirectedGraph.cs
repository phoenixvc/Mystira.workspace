namespace Mystira.Shared.GraphTheory.Graph;

/// <summary>
/// A directed graph with labeled edges.
/// </summary>
/// <typeparam name="TNode">Type of the nodes.</typeparam>
/// <typeparam name="TLabel">Type of the edge labels.</typeparam>
public class DirectedGraph<TNode, TLabel>
    where TNode : notnull
{
    private readonly Dictionary<TNode, HashSet<IEdge<TNode, TLabel>>> _adjacencyList = new();
    private readonly Dictionary<TNode, HashSet<IEdge<TNode, TLabel>>> _reverseAdjacencyList = new();

    /// <summary>
    /// Gets all nodes in the graph.
    /// </summary>
    public IReadOnlyCollection<TNode> Nodes => _adjacencyList.Keys;

    /// <summary>
    /// Gets all edges in the graph.
    /// </summary>
    public IEnumerable<IEdge<TNode, TLabel>> Edges =>
        _adjacencyList.Values.SelectMany(e => e);

    /// <summary>
    /// Gets the number of nodes.
    /// </summary>
    public int NodeCount => _adjacencyList.Count;

    /// <summary>
    /// Gets the number of edges.
    /// </summary>
    public int EdgeCount => _adjacencyList.Values.Sum(e => e.Count);

    /// <summary>
    /// Adds a node to the graph.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <returns>True if the node was added, false if it already exists.</returns>
    public bool AddNode(TNode node)
    {
        if (_adjacencyList.ContainsKey(node))
            return false;

        _adjacencyList[node] = new HashSet<IEdge<TNode, TLabel>>();
        _reverseAdjacencyList[node] = new HashSet<IEdge<TNode, TLabel>>();
        return true;
    }

    /// <summary>
    /// Adds multiple nodes to the graph.
    /// </summary>
    /// <param name="nodes">The nodes to add.</param>
    public void AddNodes(IEnumerable<TNode> nodes)
    {
        foreach (var node in nodes)
        {
            AddNode(node);
        }
    }

    /// <summary>
    /// Adds an edge to the graph.
    /// </summary>
    /// <param name="edge">The edge to add.</param>
    /// <returns>True if the edge was added.</returns>
    public bool AddEdge(IEdge<TNode, TLabel> edge)
    {
        AddNode(edge.Source);
        AddNode(edge.Target);

        _adjacencyList[edge.Source].Add(edge);
        _reverseAdjacencyList[edge.Target].Add(edge);
        return true;
    }

    /// <summary>
    /// Adds an edge to the graph.
    /// </summary>
    /// <param name="source">Source node.</param>
    /// <param name="target">Target node.</param>
    /// <param name="label">Optional label.</param>
    /// <param name="weight">Edge weight.</param>
    /// <returns>The created edge.</returns>
    public IEdge<TNode, TLabel> AddEdge(TNode source, TNode target, TLabel? label = default, double weight = 1.0)
    {
        var edge = new Edge<TNode, TLabel>(source, target, label, weight);
        AddEdge(edge);
        return edge;
    }

    /// <summary>
    /// Removes an edge from the graph.
    /// </summary>
    /// <param name="edge">The edge to remove.</param>
    /// <returns>True if the edge was removed.</returns>
    public bool RemoveEdge(IEdge<TNode, TLabel> edge)
    {
        if (!_adjacencyList.TryGetValue(edge.Source, out var outEdges))
            return false;

        var removed = outEdges.Remove(edge);
        if (removed && _reverseAdjacencyList.TryGetValue(edge.Target, out var inEdges))
        {
            inEdges.Remove(edge);
        }
        return removed;
    }

    /// <summary>
    /// Removes a node and all its edges from the graph.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <returns>True if the node was removed.</returns>
    public bool RemoveNode(TNode node)
    {
        if (!_adjacencyList.ContainsKey(node))
            return false;

        // Remove outgoing edges
        foreach (var edge in _adjacencyList[node].ToList())
        {
            if (_reverseAdjacencyList.TryGetValue(edge.Target, out var inEdges))
            {
                inEdges.Remove(edge);
            }
        }
        _adjacencyList.Remove(node);

        // Remove incoming edges
        foreach (var edge in _reverseAdjacencyList[node].ToList())
        {
            if (_adjacencyList.TryGetValue(edge.Source, out var outEdges))
            {
                outEdges.Remove(edge);
            }
        }
        _reverseAdjacencyList.Remove(node);

        return true;
    }

    /// <summary>
    /// Checks if a node exists in the graph.
    /// </summary>
    public bool ContainsNode(TNode node) => _adjacencyList.ContainsKey(node);

    /// <summary>
    /// Checks if an edge exists between two nodes.
    /// </summary>
    public bool HasEdge(TNode source, TNode target) =>
        _adjacencyList.TryGetValue(source, out var edges) &&
        edges.Any(e => e.Target.Equals(target));

    /// <summary>
    /// Gets edges from a source node.
    /// </summary>
    public IEnumerable<IEdge<TNode, TLabel>> GetOutEdges(TNode node) =>
        _adjacencyList.TryGetValue(node, out var edges) ? edges : Enumerable.Empty<IEdge<TNode, TLabel>>();

    /// <summary>
    /// Gets edges to a target node.
    /// </summary>
    public IEnumerable<IEdge<TNode, TLabel>> GetInEdges(TNode node) =>
        _reverseAdjacencyList.TryGetValue(node, out var edges) ? edges : Enumerable.Empty<IEdge<TNode, TLabel>>();

    /// <summary>
    /// Gets successor nodes (nodes reachable via outgoing edges).
    /// </summary>
    public IEnumerable<TNode> GetSuccessors(TNode node) =>
        GetOutEdges(node).Select(e => e.Target);

    /// <summary>
    /// Gets predecessor nodes (nodes with incoming edges to this node).
    /// </summary>
    public IEnumerable<TNode> GetPredecessors(TNode node) =>
        GetInEdges(node).Select(e => e.Source);

    /// <summary>
    /// Gets the out-degree of a node.
    /// </summary>
    public int GetOutDegree(TNode node) =>
        _adjacencyList.TryGetValue(node, out var edges) ? edges.Count : 0;

    /// <summary>
    /// Gets the in-degree of a node.
    /// </summary>
    public int GetInDegree(TNode node) =>
        _reverseAdjacencyList.TryGetValue(node, out var edges) ? edges.Count : 0;

    /// <summary>
    /// Gets nodes with no incoming edges.
    /// </summary>
    public IEnumerable<TNode> GetRoots() =>
        Nodes.Where(n => GetInDegree(n) == 0);

    /// <summary>
    /// Gets nodes with no outgoing edges.
    /// </summary>
    public IEnumerable<TNode> GetLeaves() =>
        Nodes.Where(n => GetOutDegree(n) == 0);

    /// <summary>
    /// Clears all nodes and edges from the graph.
    /// </summary>
    public void Clear()
    {
        _adjacencyList.Clear();
        _reverseAdjacencyList.Clear();
    }

    /// <summary>
    /// Creates a copy of this graph.
    /// </summary>
    public DirectedGraph<TNode, TLabel> Clone()
    {
        var clone = new DirectedGraph<TNode, TLabel>();
        foreach (var node in Nodes)
        {
            clone.AddNode(node);
        }
        foreach (var edge in Edges)
        {
            clone.AddEdge(edge);
        }
        return clone;
    }
}
