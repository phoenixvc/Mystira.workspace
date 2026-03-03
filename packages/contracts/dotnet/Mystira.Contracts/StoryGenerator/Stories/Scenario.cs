using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Represents a complete story scenario with all its components.
/// </summary>
public class Scenario
{
    /// <summary>
    /// Unique identifier for the scenario.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Title of the scenario.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description of the scenario.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Difficulty level of the scenario.
    /// </summary>
    [JsonPropertyName("difficulty")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

    /// <summary>
    /// Expected session length.
    /// </summary>
    [JsonPropertyName("session_length")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SessionLength SessionLength { get; set; } = SessionLength.Medium;

    /// <summary>
    /// Target age group for the scenario.
    /// </summary>
    [JsonPropertyName("age_group")]
    public string? AgeGroup { get; set; }

    /// <summary>
    /// Minimum recommended age.
    /// </summary>
    [JsonPropertyName("minimum_age")]
    public int MinimumAge { get; set; }

    /// <summary>
    /// Core narrative axes (themes) for the scenario.
    /// </summary>
    [JsonPropertyName("core_axes")]
    public List<string> CoreAxes { get; set; } = new();

    /// <summary>
    /// Character archetypes available in the scenario.
    /// </summary>
    [JsonPropertyName("archetypes")]
    public List<string> Archetypes { get; set; } = new();

    /// <summary>
    /// Tags for categorization and search.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Tone of the scenario (e.g., "mysterious", "adventurous").
    /// </summary>
    [JsonPropertyName("tone")]
    public string? Tone { get; set; }

    /// <summary>
    /// All scenes in the scenario.
    /// </summary>
    [JsonPropertyName("scenes")]
    public List<Scene> Scenes { get; set; } = new();

    /// <summary>
    /// Characters in the scenario.
    /// </summary>
    [JsonPropertyName("characters")]
    public List<ScenarioCharacter> Characters { get; set; } = new();

    /// <summary>
    /// Media references for the scenario.
    /// </summary>
    [JsonPropertyName("media")]
    public MediaReferences? Media { get; set; }

    /// <summary>
    /// Version of the scenario format.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// When the scenario was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the scenario was last updated.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
