namespace Mystira.StoryGenerator.Domain.Graph;

public interface IGraph<TNode, TEdgeLabel>
{
    IReadOnlyCollection<TNode> Nodes { get; }
    IReadOnlyCollection<IEdge<TNode, TEdgeLabel>> Edges { get; }
}
