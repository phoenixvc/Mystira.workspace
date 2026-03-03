namespace Mystira.Shared.GraphTheory;

/// <summary>
/// Generic directed edge abstraction used by graph algorithms.
/// </summary>
/// <typeparam name="TNode">The type used to identify nodes in the graph.</typeparam>
/// <typeparam name="TEdgeLabel">The type of the label carried by the edge.</typeparam>
public interface IEdge<out TNode, out TEdgeLabel>
{
    /// <summary>The source node of the edge.</summary>
    TNode From { get; }

    /// <summary>The target node of the edge.</summary>
    TNode To { get; }

    /// <summary>The label/metadata attached to the edge.</summary>
    TEdgeLabel Label { get; }
}
