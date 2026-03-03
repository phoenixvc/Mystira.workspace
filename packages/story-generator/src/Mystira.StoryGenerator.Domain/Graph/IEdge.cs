namespace Mystira.StoryGenerator.Domain.Graph;

/// <summary>
/// Generic directed edge abstraction used by graph algorithms.
/// </summary>
public interface IEdge<TNode, TEdgeLabel>
{
    TNode From { get; }
    TNode To { get; }
    TEdgeLabel Label { get; }
}
