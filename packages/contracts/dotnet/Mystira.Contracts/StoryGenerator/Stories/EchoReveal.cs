using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Represents a revelation of an echo log during gameplay.
/// </summary>
public class EchoReveal
{
    /// <summary>
    /// ID of the echo log being revealed.
    /// </summary>
    [JsonPropertyName("echo_log_id")]
    public string EchoLogId { get; set; } = string.Empty;

    /// <summary>
    /// Condition that triggers this reveal.
    /// </summary>
    [JsonPropertyName("trigger_condition")]
    public string? TriggerCondition { get; set; }

    /// <summary>
    /// Whether the reveal is automatic or requires player action.
    /// </summary>
    [JsonPropertyName("automatic")]
    public bool Automatic { get; set; } = true;

    /// <summary>
    /// Message displayed when the echo is revealed.
    /// </summary>
    [JsonPropertyName("reveal_message")]
    public string? RevealMessage { get; set; }

    /// <summary>
    /// Priority for ordering multiple reveals in the same scene.
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; }
}
