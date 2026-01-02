namespace Mystira.Shared.GraphTheory;

/// <summary>
/// Interface for a directed graph with adjacency queries.
/// </summary>
/// <typeparam name="TNode">The type used to identify nodes in the graph.</typeparam>
/// <typeparam name="TEdgeLabel">The type of labels attached to edges.</typeparam>
public interface IDirectedGraph<TNode, TEdgeLabel> : IGraph<TNode, TEdgeLabel>
{
    /// <summary>Gets the outgoing edges for the specified node.</summary>
    IReadOnlyList<IEdge<TNode, TEdgeLabel>> GetOutgoingEdges(TNode node);

    /// <summary>Gets the incoming edges for the specified node.</summary>
    IReadOnlyList<IEdge<TNode, TEdgeLabel>> GetIncomingEdges(TNode node);

    /// <summary>Gets the direct successor nodes of the specified node.</summary>
    IEnumerable<TNode> GetSuccessors(TNode node);

    /// <summary>Gets the direct predecessor nodes of the specified node.</summary>
    IEnumerable<TNode> GetPredecessors(TNode node);

    /// <summary>Returns the out-degree of the specified node.</summary>
    int OutDegree(TNode node);

    /// <summary>Returns the in-degree of the specified node.</summary>
    int InDegree(TNode node);

    /// <summary>Returns all nodes with in-degree zero (no incoming edges).</summary>
    IEnumerable<TNode> Roots();

    /// <summary>Returns all nodes with out-degree zero (no outgoing edges).</summary>
    IEnumerable<TNode> Terminals();
}
