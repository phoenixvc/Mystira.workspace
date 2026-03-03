namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a cache entry should be invalidated.
/// Used for cross-service cache coordination via Redis pub/sub.
/// </summary>
public sealed record CacheInvalidated : IntegrationEventBase
{
    /// <summary>
    /// The cache key pattern to invalidate (supports wildcards).
    /// </summary>
    public required string KeyPattern { get; init; }

    /// <summary>
    /// The entity type that was changed.
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// The entity ID that was changed (optional).
    /// </summary>
    public string? EntityId { get; init; }

    /// <summary>
    /// The service that triggered the invalidation.
    /// </summary>
    public required string SourceService { get; init; }
}

/// <summary>
/// Published when cache should be warmed with data.
/// </summary>
public sealed record CacheWarmupRequested : IntegrationEventBase
{
    /// <summary>
    /// The entity type to warm.
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// Optional list of specific entity IDs to warm.
    /// If empty, warm all entities of the type.
    /// </summary>
    public IReadOnlyList<string>? EntityIds { get; init; }

    /// <summary>
    /// Priority level (higher = more urgent).
    /// </summary>
    public int Priority { get; init; } = 0;
}
