namespace Mystira.Shared.GraphTheory.StateSpace;

/// <summary>
/// <para>
/// Result of building a frontier-merged state-space graph for a branching system.
/// </para>
/// <para>
/// This represents the quotient graph obtained by merging any two concrete states
/// that share the same (sceneId, stateSignature).
/// </para>
/// </summary>
/// <typeparam name="TSceneId">The type of scene identifiers.</typeparam>
/// <typeparam name="TState">The type of the full, concrete state.</typeparam>
/// <typeparam name="TStateSig">The type of the abstract state signature.</typeparam>
/// <typeparam name="TEdgeLabel">The type of labels attached to edges.</typeparam>
public sealed class FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel>
    where TSceneId : notnull
    where TStateSig : notnull
{
    /// <summary>The merged directed graph.</summary>
    public DirectedGraph<StateNode<TSceneId, TStateSig>, TEdgeLabel> Graph { get; }

    /// <summary>
    /// Representative underlying state for each merged node.
    /// </summary>
    public IReadOnlyDictionary<StateNode<TSceneId, TStateSig>, TState> RepresentativeState { get; }

    /// <summary>
    /// Nodes considered terminal (endings or dead-ends) in the merged graph.
    /// </summary>
    public IReadOnlyCollection<StateNode<TSceneId, TStateSig>> TerminalNodes { get; }

    /// <summary>
    /// Creates a new frontier-merged graph result.
    /// </summary>
    public FrontierMergedGraphResult(
        DirectedGraph<StateNode<TSceneId, TStateSig>, TEdgeLabel> graph,
        IReadOnlyDictionary<StateNode<TSceneId, TStateSig>, TState> representativeState,
        IReadOnlyCollection<StateNode<TSceneId, TStateSig>> terminalNodes)
    {
        Graph = graph;
        RepresentativeState = representativeState;
        TerminalNodes = terminalNodes;
    }
}
