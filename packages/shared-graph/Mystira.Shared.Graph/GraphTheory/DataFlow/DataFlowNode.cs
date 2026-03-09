namespace Mystira.Shared.GraphTheory.DataFlow;

/// <summary>
/// Represents a node in a directed graph that tracks entities
/// which are introduced or removed at that node.
/// </summary>
/// <typeparam name="TEntity">
/// The entity identifier type. Entities must support equality comparison.
/// </typeparam>
public sealed class DataFlowNode<TEntity>
{
    /// <summary>Unique identifier of this node in the graph.</summary>
    public string Id { get; }

    /// <summary>The identifiers of all immediate predecessor nodes.</summary>
    public IReadOnlyList<string> PredecessorIds { get; }

    /// <summary>The identifiers of all immediate successor nodes.</summary>
    public IReadOnlyList<string> SuccessorIds { get; }

    /// <summary>The set of entities that are introduced at this node.</summary>
    public IReadOnlyCollection<TEntity> IntroducedEntities { get; }

    /// <summary>The set of entities that are removed at this node.</summary>
    public IReadOnlyCollection<TEntity> RemovedEntities { get; }

    /// <summary>
    /// Creates a new data flow node.
    /// </summary>
    public DataFlowNode(
        string id,
        IEnumerable<string> predecessorIds,
        IEnumerable<string> successorIds,
        IEnumerable<TEntity> introducedEntities,
        IEnumerable<TEntity> removedEntities)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        PredecessorIds = predecessorIds?.ToList() ?? new List<string>();
        SuccessorIds = successorIds?.ToList() ?? new List<string>();
        IntroducedEntities = new HashSet<TEntity>(introducedEntities ?? Enumerable.Empty<TEntity>());
        RemovedEntities = new HashSet<TEntity>(removedEntities ?? Enumerable.Empty<TEntity>());
    }
}
