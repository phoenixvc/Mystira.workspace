namespace Mystira.Shared.GraphTheory;

/// <summary>
/// <para>
/// Represents a directed edge in a graph from one node to another, with an
/// associated label.
/// </para>
/// <para>
/// This is a simple immutable value object used by graph algorithms.
/// </para>
/// </summary>
/// <typeparam name="TNode">The type used to identify nodes in the graph.</typeparam>
/// <typeparam name="TEdgeLabel">
/// The type of the label carried by the edge. This can be any metadata
/// associated with the transition between nodes.
/// </typeparam>
public sealed record Edge<TNode, TEdgeLabel>(TNode From, TNode To, TEdgeLabel Label) : IEdge<TNode, TEdgeLabel>;
