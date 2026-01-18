using System.Text.Json.Serialization;

namespace Mystira.Contracts.Agents.Sessions;

/// <summary>
/// Represents an immutable snapshot of a story version.
/// Used to track story state during agent orchestration sessions.
/// </summary>
public record StoryVersionSnapshot
{
    /// <summary>
    /// The version number (1-based).
    /// </summary>
    [JsonPropertyName("version_number")]
    public int VersionNumber { get; init; }

    /// <summary>
    /// The iteration number when this version was created.
    /// </summary>
    [JsonPropertyName("iteration_number")]
    public int IterationNumber { get; init; }

    /// <summary>
    /// The story content as JSON.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// The story content as YAML (optional).
    /// </summary>
    [JsonPropertyName("yaml")]
    public string? Yaml { get; init; }

    /// <summary>
    /// Timestamp when this version was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Description of changes in this version.
    /// </summary>
    [JsonPropertyName("change_description")]
    public string? ChangeDescription { get; init; }

    /// <summary>
    /// The stage at which this version was created (as string for serialization compatibility).
    /// </summary>
    [JsonPropertyName("stage_when_created")]
    public string StageWhenCreated { get; init; } = string.Empty;
}
