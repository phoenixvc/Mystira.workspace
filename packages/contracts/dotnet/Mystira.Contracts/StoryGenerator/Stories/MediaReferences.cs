using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Contains references to media assets for scenarios, scenes, or characters.
/// </summary>
public class MediaReferences
{
    /// <summary>
    /// Primary image URL or asset ID.
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// Thumbnail image URL or asset ID.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    /// <summary>
    /// Background image URL or asset ID.
    /// </summary>
    [JsonPropertyName("background")]
    public string? Background { get; set; }

    /// <summary>
    /// Audio file URL or asset ID.
    /// </summary>
    [JsonPropertyName("audio")]
    public string? Audio { get; set; }

    /// <summary>
    /// Background music URL or asset ID.
    /// </summary>
    [JsonPropertyName("music")]
    public string? Music { get; set; }

    /// <summary>
    /// Voice narration URL or asset ID.
    /// </summary>
    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    /// <summary>
    /// Video file URL or asset ID.
    /// </summary>
    [JsonPropertyName("video")]
    public string? Video { get; set; }

    /// <summary>
    /// Portrait image for characters.
    /// </summary>
    [JsonPropertyName("portrait")]
    public string? Portrait { get; set; }

    /// <summary>
    /// Icon for UI elements.
    /// </summary>
    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    /// <summary>
    /// Additional media assets keyed by type.
    /// </summary>
    [JsonPropertyName("additional")]
    public Dictionary<string, string>? Additional { get; set; }
}
