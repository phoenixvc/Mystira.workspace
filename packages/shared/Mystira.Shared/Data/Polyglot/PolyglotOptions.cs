namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Configuration options for polyglot persistence.
/// </summary>
public class PolyglotOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Polyglot";

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
    /// Entity-specific routing overrides.
    /// Key is the entity type full name, value is the target database.
    /// </summary>
    public Dictionary<string, DatabaseTarget> EntityRouting { get; set; } = new();
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
