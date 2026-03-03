using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Represents an echo log entry that can be revealed during gameplay.
/// Echo logs are collectible story fragments that provide backstory or lore.
/// </summary>
public class EchoLog
{
    /// <summary>
    /// Unique identifier for the echo log.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of the echo log.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content of the echo log.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Category of the echo log (e.g., "history", "character", "world").
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Whether this echo log is unlocked by default.
    /// </summary>
    [JsonPropertyName("unlocked")]
    public bool Unlocked { get; set; }

    /// <summary>
    /// Condition to unlock this echo log.
    /// </summary>
    [JsonPropertyName("unlock_condition")]
    public string? UnlockCondition { get; set; }

    /// <summary>
    /// Related echo logs by ID.
    /// </summary>
    [JsonPropertyName("related_logs")]
    public List<string>? RelatedLogs { get; set; }

    /// <summary>
    /// Order for display purposes.
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// Media references for the echo log.
    /// </summary>
    [JsonPropertyName("media")]
    public MediaReferences? Media { get; set; }
}
