namespace Mystira.Shared.GraphTheory;

/// <summary>
/// Base interface for a graph with nodes and labelled edges.
/// </summary>
/// <typeparam name="TNode">The type used to identify nodes in the graph.</typeparam>
/// <typeparam name="TEdgeLabel">The type of labels attached to edges.</typeparam>
public interface IGraph<TNode, TEdgeLabel>
{
    /// <summary>All nodes present in the graph.</summary>
    IReadOnlyCollection<TNode> Nodes { get; }

    /// <summary>All directed edges present in the graph.</summary>
    IReadOnlyCollection<IEdge<TNode, TEdgeLabel>> Edges { get; }
}
