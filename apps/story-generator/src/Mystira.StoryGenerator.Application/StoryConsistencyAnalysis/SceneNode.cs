namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a node in a directed graph that tracks entities
/// which are introduced or removed at that node.
/// </summary>
/// <typeparam name="TEntity">
/// The entity identifier type (e.g., string, Guid, custom struct).
/// Entities must support equality comparison consistent with
/// <see cref="HashSet{T}"/>.
/// </typeparam>
public sealed class SceneNode<TEntity>
{
    /// <summary>
    /// Unique identifier of this node in the graph.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The identifiers of all immediate predecessor nodes
    /// (incoming edges).
    /// </summary>
    public IReadOnlyList<string> PredecessorIds { get; }

    /// <summary>
    /// The identifiers of all immediate successor nodes
    /// (outgoing edges).
    /// </summary>
    public IReadOnlyList<string> SuccessorIds { get; }

    /// <summary>
    /// The set of entities that are introduced at this node.
    /// </summary>
    public IReadOnlyCollection<TEntity> IntroducedEntities { get; }

    /// <summary>
    /// The set of entities that are removed / forgotten / invalidated
    /// at this node.
    /// </summary>
    public IReadOnlyCollection<TEntity> RemovedEntities { get; }

    public SceneNode(
        string id,
        IEnumerable<string>? predecessorIds,
        IEnumerable<string>? successorIds,
        IEnumerable<TEntity>? introducedEntities,
        IEnumerable<TEntity>? removedEntities)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        PredecessorIds = predecessorIds?.ToList() ?? new List<string>();
        SuccessorIds = successorIds?.ToList() ?? new List<string>();
        IntroducedEntities = new HashSet<TEntity>(introducedEntities ?? Enumerable.Empty<TEntity>());
        RemovedEntities = new HashSet<TEntity>(removedEntities ?? Enumerable.Empty<TEntity>());
    }
}
