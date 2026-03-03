namespace Mystira.Shared.Polyglot;

/// <summary>
/// Constants for sync operation types.
/// </summary>
public static class SyncOperation
{
    /// <summary>
    /// Insert operation - entity was created.
    /// </summary>
    public const string Insert = "Insert";

    /// <summary>
    /// Update operation - entity was modified.
    /// </summary>
    public const string Update = "Update";

    /// <summary>
    /// Delete operation - entity was removed.
    /// </summary>
    public const string Delete = "Delete";
}
