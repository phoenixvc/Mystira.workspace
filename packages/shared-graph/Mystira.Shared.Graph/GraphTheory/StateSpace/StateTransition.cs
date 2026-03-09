namespace Mystira.Shared.GraphTheory.StateSpace;

/// <summary>
/// <para>
/// Represents a single transition step in state-space exploration:
/// from a given (scene, state) to a successor scene and state.
/// </para>
/// </summary>
/// <typeparam name="TSceneId">The type of scene identifiers.</typeparam>
/// <typeparam name="TEdgeLabel">The type of labels attached to transitions.</typeparam>
/// <typeparam name="TState">The type of the concrete state.</typeparam>
public sealed record StateTransition<TSceneId, TEdgeLabel, TState>(
    TSceneId ToScene,
    TEdgeLabel Label,
    TState NextState);
