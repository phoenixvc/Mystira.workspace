namespace Mystira.Admin.Api.Configuration;

/// <summary>
/// Migration phase for the hybrid data strategy.
/// See ADR-0013 and hybrid-data-strategy-roadmap.md for details.
/// </summary>
public enum MigrationPhase
{
    /// <summary>
    /// Phase 0: Cosmos DB only (current state)
    /// All reads and writes go to Cosmos DB.
    /// </summary>
    CosmosOnly = 0,

    /// <summary>
    /// Phase 1: Dual-write with Cosmos as primary read source
    /// Writes go to both Cosmos and PostgreSQL.
    /// Reads come from Cosmos DB.
    /// </summary>
    DualWriteCosmosRead = 1,

    /// <summary>
    /// Phase 2: Dual-write with PostgreSQL as primary read source
    /// Writes go to both Cosmos and PostgreSQL.
    /// Reads come from PostgreSQL.
    /// </summary>
    DualWritePostgresRead = 2,

    /// <summary>
    /// Phase 3: PostgreSQL only (target state)
    /// All reads and writes go to PostgreSQL.
    /// Cosmos DB is read-only/archived.
    /// </summary>
    PostgresOnly = 3
}

/// <summary>
/// Configuration options for the Admin API data migration.
/// Admin API has read-only access to user data in PostgreSQL.
/// </summary>
/// <remarks>
/// Key differences from App API:
/// - Admin API only reads from PostgreSQL, never writes user data
/// - Admin API manages content (scenarios, characters, badges) in Cosmos DB
/// - Redis caching is used for content, not user sessions
/// </remarks>
public class AdminDataMigrationOptions
{
    public const string SectionName = "DataMigration";

    /// <summary>
    /// Current migration phase. Controls read/write behavior.
    /// Default: CosmosOnly (Phase 0)
    /// </summary>
    public MigrationPhase Phase { get; set; } = MigrationPhase.CosmosOnly;

    /// <summary>
    /// Whether migration features are enabled.
    /// When false, behaves as Phase 0 regardless of Phase setting.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Admin API only reads from PostgreSQL, never writes user data.
    /// This is always true for Admin API (enforced in code).
    /// </summary>
    public bool ReadOnlyPostgresAccess { get; set; } = true;

    /// <summary>
    /// Enable dual-write for content entities (scenarios, etc.)
    /// When true, content updates are written to both Cosmos and PostgreSQL.
    /// </summary>
    public bool EnableContentDualWrite { get; set; }

    /// <summary>
    /// Enable Redis caching for content entities.
    /// </summary>
    public bool EnableContentCaching { get; set; } = true;

    /// <summary>
    /// Cache duration for content entities in minutes.
    /// </summary>
    public int ContentCacheMinutes { get; set; } = 30;

    /// <summary>
    /// Cache duration for user data lookups in minutes.
    /// Shorter than content cache since user data changes more frequently.
    /// </summary>
    public int UserCacheMinutes { get; set; } = 5;
}
