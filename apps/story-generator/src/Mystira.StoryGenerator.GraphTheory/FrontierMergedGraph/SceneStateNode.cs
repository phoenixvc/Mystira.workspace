namespace Mystira.StoryGenerator.GraphTheory.FrontierMergedGraph;

/// <summary>
/// <para>
/// Represents a node in a frontier-merged state-space graph, identified by
/// a scene identifier and an abstract state signature.
/// </para>
/// <para>
/// In the merged graph, two concrete states are considered equivalent and
/// mapped to the same <see cref="SceneStateNode{TSceneId, TStateSig}"/> if they:
/// - are at the same scene, and
/// - share the same abstract state signature.
/// </para>
/// <para>
/// This abstraction allows multiple raw histories (different paths) that lead
/// to indistinguishable future behaviour for the properties of interest to be
/// merged into a single node, preventing combinatorial explosion.
/// </para>
/// </summary>
/// <typeparam name="TSceneId">
/// <para>
/// The type of scene identifiers (for example, string or GUID).
/// This is used to refer back to the original scenario / story structure.
/// </para>
/// </typeparam>
/// <typeparam name="TStateSig">
/// <para>
/// The type of the abstract state signature used for merging.
/// It should encode exactly the information relevant for your consistency
/// checks (for example, known entities, critical flags, time bucket).
/// </para>
/// <para>
/// Two concrete states that map to the same <typeparamref name="TStateSig"/>
/// for a given scene are treated as equivalent in the merged graph.
/// </para>
/// </typeparam>
public sealed record SceneStateNode<TSceneId, TStateSig>(TSceneId SceneId, TStateSig StateSignature)
    where TSceneId : notnull
    where TStateSig : notnull;
