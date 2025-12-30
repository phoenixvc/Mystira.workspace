namespace Mystira.App.Admin.Api.Models;

/// <summary>
/// Request for checking client status
/// </summary>
public class ClientStatusRequest
{
    public string ClientVersion { get; set; } = string.Empty;
    public string? ContentVersion { get; set; }
}

/// <summary>
/// Response for client status check
/// </summary>
public class ClientStatusResponse
{
    public bool ForceRefresh { get; set; }
    public string MinSupportedVersion { get; set; } = string.Empty;
    public string LatestVersion { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ContentManifest ContentManifest { get; set; } = new();
    public string BundleVersion { get; set; } = string.Empty;
    public bool UpdateRequired { get; set; }
}

/// <summary>
/// Content manifest for client synchronization
/// </summary>
public class ContentManifest
{
    public ScenarioChanges Scenarios { get; set; } = new();
    public MediaChanges Media { get; set; } = new();
    public string BundleVersion { get; set; } = string.Empty;
}

/// <summary>
/// Scenario changes for content sync
/// </summary>
public class ScenarioChanges
{
    public List<string> Added { get; set; } = new();
    public List<string> Updated { get; set; } = new();
    public List<string> Removed { get; set; } = new();
}

/// <summary>
/// Media changes for content sync
/// </summary>
public class MediaChanges
{
    public List<MediaItem> Added { get; set; } = new();
    public List<MediaItem> Updated { get; set; } = new();
    public List<string> Removed { get; set; } = new();
}

/// <summary>
/// Individual media item for content sync
/// </summary>
public class MediaItem
{
    public string MediaId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
}
