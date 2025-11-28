namespace Mystira.StoryGenerator.GraphTheory.Graph;

/// <summary>
/// <para>
/// Represents a directed edge in a graph from one node to another, with an
/// associated label.
/// </para>
/// <para>
/// This is a simple immutable value object used by
/// <see cref="TNode"/> and related algorithms.
/// </para>
/// </summary>
/// <typeparam name="TEdgeLabel">
/// <para>
/// The type used to identify nodes in the graph (for example, string or GUID).
/// </para>
/// </typeparam>
/// <typeparam name="TEdgeLabel">
/// <para>
/// The type of the label carried by the edge. This can be any metadata
/// associated with the transition between nodes (for example, choice ID,
/// roll outcome, or a simple unit type).
/// </para>
/// </typeparam>
public sealed record Edge<TNode, TEdgeLabel>(TNode From, TNode To, TEdgeLabel Label);
