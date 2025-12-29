namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Constants for sync status values.
/// </summary>
public static class SyncStatus
{
    /// <summary>
    /// Sync is pending - operation not yet applied to secondary.
    /// </summary>
    public const string Pending = "Pending";

    /// <summary>
    /// Sync completed successfully.
    /// </summary>
    public const string Synced = "Synced";

    /// <summary>
    /// Sync failed - requires retry or manual intervention.
    /// </summary>
    public const string Failed = "Failed";

    /// <summary>
    /// Sync was compensated - rollback applied.
    /// </summary>
    public const string Compensated = "Compensated";
}
