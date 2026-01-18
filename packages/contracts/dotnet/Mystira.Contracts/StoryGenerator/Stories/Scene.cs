using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Represents a single scene within a story scenario.
/// </summary>
public class Scene
{
    /// <summary>
    /// Unique identifier for the scene.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title or name of the scene.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Type of scene (narrative, choice, roll, special).
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SceneType Type { get; set; } = SceneType.Narrative;

    /// <summary>
    /// The narrative text for this scene.
    /// </summary>
    [JsonPropertyName("narrative")]
    public string Narrative { get; set; } = string.Empty;

    /// <summary>
    /// Branches (choices or outcomes) available from this scene.
    /// </summary>
    [JsonPropertyName("branches")]
    public List<Branch> Branches { get; set; } = new();

    /// <summary>
    /// Echo log entries that may be revealed in this scene.
    /// </summary>
    [JsonPropertyName("echo_logs")]
    public List<EchoLog>? EchoLogs { get; set; }

    /// <summary>
    /// Compass changes that occur in this scene.
    /// </summary>
    [JsonPropertyName("compass_changes")]
    public List<CompassChange>? CompassChanges { get; set; }

    /// <summary>
    /// Echo reveals that occur in this scene.
    /// </summary>
    [JsonPropertyName("echo_reveals")]
    public List<EchoReveal>? EchoReveals { get; set; }

    /// <summary>
    /// Media references specific to this scene.
    /// </summary>
    [JsonPropertyName("media")]
    public MediaReferences? Media { get; set; }

    /// <summary>
    /// Metadata about the scene.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Order of the scene in the scenario.
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// Whether this scene can be the starting scene.
    /// </summary>
    [JsonPropertyName("is_start")]
    public bool IsStart { get; set; }

    /// <summary>
    /// Whether this scene is an ending scene.
    /// </summary>
    [JsonPropertyName("is_end")]
    public bool IsEnd { get; set; }

    /// <summary>
    /// Determines if this scene is a final/ending scene.
    /// A scene is final if it has no branches or if it's marked as an end scene.
    /// </summary>
    /// <returns>True if this is a final scene, false otherwise.</returns>
    public bool IsFinalScene()
    {
        return IsEnd || (Branches?.Count ?? 0) == 0;
    }
}
