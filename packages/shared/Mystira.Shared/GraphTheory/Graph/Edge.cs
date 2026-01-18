namespace Mystira.Shared.GraphTheory.Graph;

/// <summary>
/// Default implementation of a graph edge.
/// </summary>
/// <typeparam name="TNode">Type of the nodes.</typeparam>
/// <typeparam name="TLabel">Type of the edge label.</typeparam>
public class Edge<TNode, TLabel> : IEdge<TNode, TLabel>
    where TNode : notnull
{
    /// <summary>
    /// Creates a new edge.
    /// </summary>
    /// <param name="source">Source node.</param>
    /// <param name="target">Target node.</param>
    /// <param name="label">Optional edge label.</param>
    /// <param name="weight">Edge weight (default 1.0).</param>
    public Edge(TNode source, TNode target, TLabel? label = default, double weight = 1.0)
    {
        Source = source;
        Target = target;
        Label = label;
        Weight = weight;
    }

    /// <inheritdoc />
    public TNode Source { get; }

    /// <inheritdoc />
    public TNode Target { get; }

    /// <inheritdoc />
    public TLabel? Label { get; }

    /// <inheritdoc />
    public double Weight { get; }

    /// <summary>
    /// Returns the reverse of this edge.
    /// </summary>
    public Edge<TNode, TLabel> Reverse() =>
        new(Target, Source, Label, Weight);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is Edge<TNode, TLabel> other)
        {
            return Source.Equals(other.Source) &&
                   Target.Equals(other.Target) &&
                   Equals(Label, other.Label);
        }
        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(Source, Target, Label);

    /// <inheritdoc />
    public override string ToString() =>
        $"({Source} -> {Target}{(Label != null ? $" [{Label}]" : "")})";
}
