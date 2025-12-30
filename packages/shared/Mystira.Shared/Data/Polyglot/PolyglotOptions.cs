namespace Mystira.Shared.Polyglot;

/// <summary>
/// Polyglot persistence mode.
/// </summary>
public enum PolyglotMode
{
    /// <summary>
    /// Single store mode - writes only to primary backend.
    /// </summary>
    SingleStore,

    /// <summary>
    /// Dual-write mode - writes to both primary and secondary backends.
    /// </summary>
    DualWrite
}

/// <summary>
/// Configuration options for polyglot persistence.
/// </summary>
public class PolyglotOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "PolyglotPersistence";

    /// <summary>
    /// The persistence mode (SingleStore or DualWrite).
    /// </summary>
    public PolyglotMode Mode { get; set; } = PolyglotMode.SingleStore;

    /// <summary>
    /// Enable compensation on secondary write failure.
    /// When true, primary write will be rolled back if secondary fails.
    /// </summary>
    public bool EnableCompensation { get; set; } = true;

    /// <summary>
    /// Timeout for secondary write operations in milliseconds.
    /// </summary>
    public int SecondaryWriteTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Enable consistency validation between backends.
    /// </summary>
    public bool EnableConsistencyValidation { get; set; } = false;

    /// <summary>
    /// Default database target for entities without explicit routing.
    /// </summary>
    public DatabaseTarget DefaultTarget { get; set; } = DatabaseTarget.CosmosDb;

    /// <summary>
    /// Enable cache-aside pattern for read operations.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Default cache expiration in seconds.
    /// </summary>
    public int CacheExpirationSeconds { get; set; } = 300;

    /// <summary>
    /// Enable resilience policies (retry, circuit breaker).
    /// </summary>
    public bool EnableResilience { get; set; } = true;

    /// <summary>
    /// Number of retry attempts for failed operations.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Initial retry delay in milliseconds.
    /// </summary>
    public int RetryDelayMs { get; set; } = 100;

    /// <summary>
    /// Circuit breaker failure threshold before opening.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Circuit breaker duration of break in seconds.
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 30;

    /// <summary>
    /// Entity-specific routing overrides.
    /// Key is the entity type full name, value is the target database.
    /// </summary>
    public Dictionary<string, DatabaseTarget> EntityRouting { get; set; } = new();

    /// <summary>
    /// Enable sync logging for audit trail.
    /// </summary>
    public bool EnableSyncLogging { get; set; } = true;

    /// <summary>
    /// Maximum sync log retention in days.
    /// </summary>
    public int SyncLogRetentionDays { get; set; } = 30;
}

/// <summary>
/// Target database for entity storage.
/// </summary>
public enum DatabaseTarget
{
    /// <summary>
    /// Azure Cosmos DB - for document storage, complex nested structures.
    /// </summary>
    CosmosDb,

    /// <summary>
    /// PostgreSQL - for relational data, analytics, embeddings.
    /// </summary>
    PostgreSql
}

/// <summary>
/// Attribute to specify the target database for an entity.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DatabaseTargetAttribute : Attribute
{
    /// <summary>
    /// The target database for this entity.
    /// </summary>
    public DatabaseTarget Target { get; }

    /// <summary>
    /// Reason for the database choice (for documentation).
    /// </summary>
    public string? Rationale { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseTargetAttribute"/> class.
    /// </summary>
    /// <param name="target">The target database for this entity.</param>
    public DatabaseTargetAttribute(DatabaseTarget target)
    {
        Target = target;
    }
}
