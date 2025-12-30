namespace Mystira.Contracts.App.Responses.Client;

/// <summary>
/// Response containing client status and required updates.
/// </summary>
public record ClientStatusResponse
{
    /// <summary>
    /// Whether the client is up to date.
    /// </summary>
    public bool IsUpToDate { get; set; }

    /// <summary>
    /// Whether an app update is required.
    /// </summary>
    public bool RequiresUpdate { get; set; }

    /// <summary>
    /// The minimum required app version.
    /// </summary>
    public string? MinimumVersion { get; set; }

    /// <summary>
    /// The latest available app version.
    /// </summary>
    public string? LatestVersion { get; set; }

    /// <summary>
    /// URL to download the latest version.
    /// </summary>
    public string? UpdateUrl { get; set; }

    /// <summary>
    /// Whether content updates are available.
    /// </summary>
    public bool HasContentUpdates { get; set; }

    /// <summary>
    /// Content manifest for synchronization.
    /// </summary>
    public ContentManifest? ContentManifest { get; set; }

    /// <summary>
    /// Server timestamp for synchronization.
    /// </summary>
    public DateTime ServerTimestamp { get; set; }

    /// <summary>
    /// Feature flags for the client.
    /// </summary>
    public Dictionary<string, bool>? FeatureFlags { get; set; }

    /// <summary>
    /// Maintenance mode information if applicable.
    /// </summary>
    public MaintenanceInfo? Maintenance { get; set; }
}

/// <summary>
/// Content manifest for client synchronization.
/// </summary>
public record ContentManifest
{
    /// <summary>
    /// The content bundle version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the manifest was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Changes to scenarios since last sync.
    /// </summary>
    public ScenarioChanges? ScenarioChanges { get; set; }

    /// <summary>
    /// Changes to media since last sync.
    /// </summary>
    public MediaChanges? MediaChanges { get; set; }

    /// <summary>
    /// Total download size in bytes.
    /// </summary>
    public long TotalDownloadSize { get; set; }
}

/// <summary>
/// Scenario changes since last synchronization.
/// </summary>
public record ScenarioChanges
{
    /// <summary>
    /// List of new scenario IDs.
    /// </summary>
    public List<string> Added { get; set; } = new();

    /// <summary>
    /// List of updated scenario IDs.
    /// </summary>
    public List<string> Updated { get; set; } = new();

    /// <summary>
    /// List of removed scenario IDs.
    /// </summary>
    public List<string> Removed { get; set; } = new();

    /// <summary>
    /// Total count of changes.
    /// </summary>
    public int TotalChanges => Added.Count + Updated.Count + Removed.Count;
}

/// <summary>
/// Media changes since last synchronization.
/// </summary>
public record MediaChanges
{
    /// <summary>
    /// List of new media items.
    /// </summary>
    public List<MediaItem> Added { get; set; } = new();

    /// <summary>
    /// List of updated media items.
    /// </summary>
    public List<MediaItem> Updated { get; set; } = new();

    /// <summary>
    /// List of removed media IDs.
    /// </summary>
    public List<string> Removed { get; set; } = new();

    /// <summary>
    /// Total download size for media in bytes.
    /// </summary>
    public long TotalDownloadSize { get; set; }
}

/// <summary>
/// Represents a media item for synchronization.
/// </summary>
public record MediaItem
{
    /// <summary>
    /// The unique identifier of the media item.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The type of media (e.g., "image", "audio", "video").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The URL to download the media.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The file size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The content hash for integrity verification.
    /// </summary>
    public string? Hash { get; set; }

    /// <summary>
    /// The MIME type of the media.
    /// </summary>
    public string? MimeType { get; set; }
}

/// <summary>
/// Information about scheduled maintenance.
/// </summary>
public record MaintenanceInfo
{
    /// <summary>
    /// Whether maintenance mode is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Message to display to users.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Scheduled start time of maintenance.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Expected end time of maintenance.
    /// </summary>
    public DateTime? EndTime { get; set; }
}
