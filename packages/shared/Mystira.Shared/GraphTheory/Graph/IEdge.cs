namespace Mystira.Shared.GraphTheory.Graph;

/// <summary>
/// Interface for a graph edge.
/// </summary>
/// <typeparam name="TNode">Type of the nodes.</typeparam>
/// <typeparam name="TLabel">Type of the edge label.</typeparam>
public interface IEdge<TNode, TLabel>
    where TNode : notnull
{
    /// <summary>
    /// Source node of the edge.
    /// </summary>
    TNode Source { get; }

    /// <summary>
    /// Target node of the edge.
    /// </summary>
    TNode Target { get; }

    /// <summary>
    /// Label associated with the edge.
    /// </summary>
    TLabel? Label { get; }

    /// <summary>
    /// Weight of the edge (default 1.0).
    /// </summary>
    double Weight { get; }
}
