using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Represents a change to a compass axis value.
/// Compass values track character progression along moral/personality axes.
/// </summary>
public class CompassChange
{
    /// <summary>
    /// The compass axis being modified (e.g., "courage", "kindness", "wisdom").
    /// </summary>
    [JsonPropertyName("axis")]
    public string Axis { get; set; } = string.Empty;

    /// <summary>
    /// The amount of change (positive or negative).
    /// </summary>
    [JsonPropertyName("delta")]
    public int Delta { get; set; }

    /// <summary>
    /// Optional condition for this change to apply.
    /// </summary>
    [JsonPropertyName("condition")]
    public string? Condition { get; set; }

    /// <summary>
    /// Description of why this change occurs.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    /// <summary>
    /// Whether this change is visible to the player.
    /// </summary>
    [JsonPropertyName("visible")]
    public bool Visible { get; set; } = true;
}
