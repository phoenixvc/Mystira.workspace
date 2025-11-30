using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.GraphTheory.FrontierMergedGraph;

/// <summary>
/// <para>
/// Result of building a frontier-merged state-space graph for a branching system.
/// </para>
/// <para>
/// This represents the quotient graph obtained by: <br/>
/// - Starting from an initial (scene, concreteState), <br/>
/// - Exploring all reachable transitions using a user-supplied transition function, <br/>
/// - Merging any two concrete states that share the same (sceneId, stateSignature).
/// </para>
/// <para>
/// Typical usage (Mystira context): <br/>
/// - TSceneId:   scene identifier (e.g. string), <br/>
/// - TState:     full story state (known entities, flags, time, etc.), <br/>
/// - TStateSig:  abstracted state signature used for equivalence / merging, <br/>
/// - TEdgeLabel: transition metadata (choice, roll outcome, etc.).
/// </para>
/// <para>
/// Once built, this result can be used for: <br/>
/// - Counting effective root→ending paths in the merged state-space, <br/>
/// - Running consistency checks along effective paths, <br/>
/// - Generating per-node or per-path diagnostics using the representative state.
/// </para>
/// </summary>
/// <typeparam name="TSceneId">
/// <para>
/// The type of scene identifiers (e.g. string, GUID).
/// Must be comparable via equality since it is used as part of the graph node key.
/// </para>
/// </typeparam>
/// <typeparam name="TState">
/// <para>
/// The type of the full, concrete story state used during exploration.
/// This can contain rich information (entities, flags, time, etc.) that you
/// do not necessarily use for merging, but still want available for analysis.
/// </para>
/// </typeparam>
/// <typeparam name="TStateSig">
/// <para>
/// The type of the abstract state signature used for frontier merging.
/// Two concrete states are merged into the same graph node when they share
/// the same scene identifier and the same state signature.
/// </para>
/// <para>
/// This type should implement a meaningful equality (value semantics).
/// </para>
/// </typeparam>
/// <typeparam name="TEdgeLabel">
/// <para>
/// The type of labels attached to graph edges (transitions).
/// In Mystira, this will typically capture transition kind (narrative/choice/roll),
/// choice IDs, roll outcomes, or other metadata useful for later analysis.
/// </para>
/// </typeparam>
public sealed class FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel>
    where TSceneId : notnull
    where TStateSig : notnull
{
    public DirectedGraph<SceneStateNode<TSceneId, TStateSig>, TEdgeLabel> Graph { get; }

    /// <summary>
    /// Representative underlying state for each merged node.
    /// (If multiple raw histories merge into the same (scene, signature),
    /// this stores one representative state – usually enough for debugging / inspection.)
    /// </summary>
    public IReadOnlyDictionary<SceneStateNode<TSceneId, TStateSig>, TState> RepresentativeState { get; }

    /// <summary>
    /// Nodes considered terminal (endings or dead-ends) in the merged graph.
    /// </summary>
    public IReadOnlyCollection<SceneStateNode<TSceneId, TStateSig>> TerminalNodes { get; }

    public FrontierMergedGraphResult(
        DirectedGraph<SceneStateNode<TSceneId, TStateSig>, TEdgeLabel> graph,
        IReadOnlyDictionary<SceneStateNode<TSceneId, TStateSig>, TState> representativeState,
        IReadOnlyCollection<SceneStateNode<TSceneId, TStateSig>> terminalNodes)
    {
        Graph = graph;
        RepresentativeState = representativeState;
        TerminalNodes = terminalNodes;
    }
}
