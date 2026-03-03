namespace Mystira.Shared.Polyglot;

/// <summary>
/// Result of a consistency validation between primary and secondary backends.
/// </summary>
public sealed class ConsistencyResult
{
    /// <summary>
    /// Indicates whether the entity is consistent across backends.
    /// </summary>
    public bool IsConsistent { get; init; }

    /// <summary>
    /// The entity ID that was validated.
    /// </summary>
    public required string EntityId { get; init; }

    /// <summary>
    /// The entity type name.
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// Indicates whether the entity exists in the primary backend.
    /// </summary>
    public bool ExistsInPrimary { get; init; }

    /// <summary>
    /// Indicates whether the entity exists in the secondary backend.
    /// </summary>
    public bool ExistsInSecondary { get; init; }

    /// <summary>
    /// Hash or version of the primary entity for comparison.
    /// </summary>
    public string? PrimaryVersion { get; init; }

    /// <summary>
    /// Hash or version of the secondary entity for comparison.
    /// </summary>
    public string? SecondaryVersion { get; init; }

    /// <summary>
    /// Differences found between primary and secondary, if any.
    /// </summary>
    public IReadOnlyList<string> Differences { get; init; } = [];

    /// <summary>
    /// Timestamp when the validation was performed.
    /// </summary>
    public DateTime ValidatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a consistent result.
    /// </summary>
    public static ConsistencyResult Consistent(string entityId, string entityType) => new()
    {
        IsConsistent = true,
        EntityId = entityId,
        EntityType = entityType,
        ExistsInPrimary = true,
        ExistsInSecondary = true
    };

    /// <summary>
    /// Creates an inconsistent result with differences.
    /// </summary>
    public static ConsistencyResult Inconsistent(
        string entityId,
        string entityType,
        IReadOnlyList<string> differences) => new()
    {
        IsConsistent = false,
        EntityId = entityId,
        EntityType = entityType,
        ExistsInPrimary = true,
        ExistsInSecondary = true,
        Differences = differences
    };

    /// <summary>
    /// Creates a result for missing entity in secondary.
    /// </summary>
    public static ConsistencyResult MissingInSecondary(string entityId, string entityType) => new()
    {
        IsConsistent = false,
        EntityId = entityId,
        EntityType = entityType,
        ExistsInPrimary = true,
        ExistsInSecondary = false,
        Differences = ["Entity missing in secondary backend"]
    };

    /// <summary>
    /// Creates a result for missing entity in primary.
    /// </summary>
    public static ConsistencyResult MissingInPrimary(string entityId, string entityType) => new()
    {
        IsConsistent = false,
        EntityId = entityId,
        EntityType = entityType,
        ExistsInPrimary = false,
        ExistsInSecondary = true,
        Differences = ["Entity missing in primary backend (orphaned)"]
    };

    /// <summary>
    /// Creates an error result.
    /// </summary>
    public static ConsistencyResult Error(string entityId, string entityType, string errorMessage) => new()
    {
        IsConsistent = false,
        EntityId = entityId,
        EntityType = entityType,
        ErrorMessage = errorMessage
    };
}
