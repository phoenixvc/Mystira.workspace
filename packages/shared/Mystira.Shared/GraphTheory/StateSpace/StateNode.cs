namespace Mystira.Shared.GraphTheory.StateSpace;

/// <summary>
/// <para>
/// Represents a node in a frontier-merged state-space graph, identified by
/// a scene identifier and an abstract state signature.
/// </para>
/// <para>
/// Two concrete states are considered equivalent and mapped to the same node if they
/// are at the same scene and share the same abstract state signature.
/// </para>
/// </summary>
/// <typeparam name="TSceneId">The type of scene identifiers.</typeparam>
/// <typeparam name="TStateSig">The type of the abstract state signature used for merging.</typeparam>
public sealed record StateNode<TSceneId, TStateSig>(TSceneId SceneId, TStateSig StateSignature)
    where TSceneId : notnull
    where TStateSig : notnull;
