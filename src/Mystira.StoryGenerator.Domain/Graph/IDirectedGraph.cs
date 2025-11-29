namespace Mystira.StoryGenerator.Domain.Graph;

public interface IDirectedGraph<TNode, TEdgeLabel> : IGraph<TNode, TEdgeLabel>
{
    IReadOnlyList<IEdge<TNode, TEdgeLabel>> GetOutgoingEdges(TNode node);
    IReadOnlyList<IEdge<TNode, TEdgeLabel>> GetIncomingEdges(TNode node);

    IEnumerable<TNode> GetSuccessors(TNode node);
    IEnumerable<TNode> GetPredecessors(TNode node);

    int OutDegree(TNode node);
    int InDegree(TNode node);

    IEnumerable<TNode> Roots();
    IEnumerable<TNode> Terminals();
}
