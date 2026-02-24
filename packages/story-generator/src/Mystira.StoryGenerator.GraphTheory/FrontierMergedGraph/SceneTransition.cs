namespace Mystira.StoryGenerator.GraphTheory.FrontierMergedGraph;

/// <summary>
/// <para>
/// Represents a single transition step in the underlying state-space
/// exploration: from a given (scene, state) to a successor scene and state.
/// </para>
/// <para>
/// This is the domain-level result produced by the user-supplied transition
/// function during frontier-merged graph construction. It combines:
/// - the identifier of the next scene,
/// - a label describing the transition, and
/// - the resulting concrete story state after applying that transition.
/// </para>
/// </summary>
/// <typeparam name="TSceneId">
/// <para>
/// The type of scene identifiers (for example, string or GUID).
/// </para>
/// </typeparam>
/// <typeparam name="TEdgeLabel">
/// <para>
/// The type of labels attached to transitions in the merged graph.
/// In Mystira, this typically captures transition kind (narrative, choice,
/// roll outcome) and any associated metadata (choice ID, roll ID, etc.).
/// </para>
/// </typeparam>
/// <typeparam name="TState">
/// <para>
/// The type of the concrete story state used during exploration.
/// This can contain rich information beyond what is kept in the abstract
/// state signature, and is used to compute successors and run analyses.
/// </para>
/// </typeparam>
public sealed record SceneTransition<TSceneId, TEdgeLabel, TState>(
    TSceneId ToScene,
    TEdgeLabel Label,
    TState NextState);
