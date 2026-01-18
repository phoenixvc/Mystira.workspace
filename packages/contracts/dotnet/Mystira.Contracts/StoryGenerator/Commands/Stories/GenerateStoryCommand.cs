using System.Text.Json.Serialization;
using Mystira.Contracts.StoryGenerator.Stories;

namespace Mystira.Contracts.StoryGenerator.Commands.Stories;

/// <summary>
/// Command to generate a new story.
/// </summary>
public class GenerateStoryCommand
{
    /// <summary>
    /// Title of the story.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description or premise of the story.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Difficulty level.
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
    /// Target age group.
    /// </summary>
    [JsonPropertyName("age_group")]
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// Minimum recommended age.
    /// </summary>
    [JsonPropertyName("minimum_age")]
    public int MinimumAge { get; set; }

    /// <summary>
    /// Core narrative axes (themes).
    /// </summary>
    [JsonPropertyName("core_axes")]
    public List<string> CoreAxes { get; set; } = new();

    /// <summary>
    /// Character archetypes to include.
    /// </summary>
    [JsonPropertyName("archetypes")]
    public List<string> Archetypes { get; set; } = new();

    /// <summary>
    /// Tags for categorization.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Tone of the story.
    /// </summary>
    [JsonPropertyName("tone")]
    public string? Tone { get; set; }

    /// <summary>
    /// Minimum number of scenes.
    /// </summary>
    [JsonPropertyName("min_scenes")]
    public int MinScenes { get; set; } = 6;

    /// <summary>
    /// Maximum number of scenes.
    /// </summary>
    [JsonPropertyName("max_scenes")]
    public int MaxScenes { get; set; } = 12;

    /// <summary>
    /// Number of player characters to generate.
    /// </summary>
    [JsonPropertyName("character_count")]
    public int CharacterCount { get; set; } = 0;

    /// <summary>
    /// AI provider to use.
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// Model ID to use.
    /// </summary>
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    /// <summary>
    /// Specific model name.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Whether to stream the response.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    /// <summary>
    /// Additional instructions for generation.
    /// </summary>
    [JsonPropertyName("additional_instructions")]
    public string? AdditionalInstructions { get; set; }
}
