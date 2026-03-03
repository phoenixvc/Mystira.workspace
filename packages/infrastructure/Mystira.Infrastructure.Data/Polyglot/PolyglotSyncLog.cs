using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mystira.Infrastructure.Data.Polyglot;

/// <summary>
/// Entity for tracking dual-write sync operations between Cosmos DB and PostgreSQL.
/// Stored in PostgreSQL _polyglot_sync_log table.
/// </summary>
[Table("_polyglot_sync_log")]
public class PolyglotSyncLog
{
    /// <summary>Gets or sets the auto-generated primary key.</summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>Gets or sets the entity type name being synced.</summary>
    [Required]
    [MaxLength(100)]
    [Column("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity ID being synced.</summary>
    [Required]
    [MaxLength(36)]
    [Column("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Gets or sets the operation type (INSERT, UPDATE, DELETE).</summary>
    [Required]
    [MaxLength(20)]
    [Column("operation")]
    public string Operation { get; set; } = string.Empty;

    /// <summary>Gets or sets the source backend (cosmos or postgres).</summary>
    [Required]
    [MaxLength(20)]
    [Column("source_backend")]
    public string SourceBackend { get; set; } = "cosmos";

    /// <summary>Gets or sets the sync status (pending, synced, failed).</summary>
    [MaxLength(20)]
    [Column("sync_status")]
    public string SyncStatus { get; set; } = "pending";

    /// <summary>Gets or sets the Cosmos DB timestamp when the change occurred.</summary>
    [Column("cosmos_timestamp")]
    public DateTime? CosmosTimestamp { get; set; }

    /// <summary>Gets or sets the PostgreSQL timestamp when the sync was recorded.</summary>
    [Column("postgres_timestamp")]
    public DateTime PostgresTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the error message if sync failed.</summary>
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    /// <summary>Gets or sets the retry count for failed syncs.</summary>
    [Column("retry_count")]
    public int RetryCount { get; set; } = 0;

    /// <summary>Gets or sets the record creation timestamp.</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Sync operation types for polyglot persistence logging.
/// </summary>
public static class SyncOperation
{
    /// <summary>Insert operation.</summary>
    public const string Insert = "INSERT";
    /// <summary>Update operation.</summary>
    public const string Update = "UPDATE";
    /// <summary>Delete operation.</summary>
    public const string Delete = "DELETE";
}

/// <summary>
/// Sync status values for polyglot persistence logging.
/// </summary>
public static class SyncStatus
{
    /// <summary>Sync is pending.</summary>
    public const string Pending = "pending";
    /// <summary>Sync completed successfully.</summary>
    public const string Synced = "synced";
    /// <summary>Sync failed.</summary>
    public const string Failed = "failed";
}
