namespace Mystira.Contracts.App.Requests.Client;

/// <summary>
/// Request for client status and synchronization information.
/// </summary>
public record ClientStatusRequest
{
    /// <summary>
    /// The client's current app version.
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// The client's platform (e.g., "ios", "android", "web").
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// The client's last known sync timestamp.
    /// </summary>
    public DateTime? LastSyncTimestamp { get; set; }

    /// <summary>
    /// Optional account identifier for authenticated clients.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Optional profile identifier for the current user profile.
    /// </summary>
    public string? ProfileId { get; set; }

    /// <summary>
    /// Current content bundle version on the client.
    /// </summary>
    public string? ContentBundleVersion { get; set; }
}
