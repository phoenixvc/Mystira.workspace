using System.Text.Json.Serialization;
using Mystira.Contracts.StoryGenerator.Common;

namespace Mystira.Contracts.StoryGenerator.Commands.Stories;

/// <summary>
/// Command to summarize a story.
/// </summary>
public class SummarizeStoryCommand
{
    /// <summary>
    /// The story to summarize (JSON or YAML).
    /// </summary>
    [JsonPropertyName("story")]
    public string Story { get; set; } = string.Empty;

    /// <summary>
    /// Format of the story.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "json";

    /// <summary>
    /// Type of summary to generate.
    /// </summary>
    [JsonPropertyName("summary_type")]
    public SummaryType SummaryType { get; set; } = SummaryType.Standard;

    /// <summary>
    /// Maximum length of the summary.
    /// </summary>
    [JsonPropertyName("max_length")]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Whether to include character summaries.
    /// </summary>
    [JsonPropertyName("include_characters")]
    public bool IncludeCharacters { get; set; } = true;

    /// <summary>
    /// Whether to include path analysis.
    /// </summary>
    [JsonPropertyName("include_paths")]
    public bool IncludePaths { get; set; } = false;

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
}

/// <summary>
/// Types of story summaries.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SummaryType
{
    /// <summary>
    /// Brief one-paragraph summary.
    /// </summary>
    Brief,

    /// <summary>
    /// Standard summary with key elements.
    /// </summary>
    Standard,

    /// <summary>
    /// Detailed summary with all elements.
    /// </summary>
    Detailed,

    /// <summary>
    /// Technical summary for developers.
    /// </summary>
    Technical,

    /// <summary>
    /// Marketing-style description.
    /// </summary>
    Marketing
}

/// <summary>
/// Response from story summarization.
/// </summary>
public class SummarizeStoryResponse
{
    /// <summary>
    /// Whether summarization succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// The main summary.
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Summary type.
    /// </summary>
    [JsonPropertyName("summary_type")]
    public SummaryType SummaryType { get; set; }

    /// <summary>
    /// Key themes identified.
    /// </summary>
    [JsonPropertyName("themes")]
    public List<string>? Themes { get; set; }

    /// <summary>
    /// Character summaries.
    /// </summary>
    [JsonPropertyName("character_summaries")]
    public Dictionary<string, string>? CharacterSummaries { get; set; }

    /// <summary>
    /// Path summaries if requested.
    /// </summary>
    [JsonPropertyName("path_summaries")]
    public List<PathSummary>? PathSummaries { get; set; }

    /// <summary>
    /// Statistics about the story.
    /// </summary>
    [JsonPropertyName("statistics")]
    public StoryStatistics? Statistics { get; set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Token usage.
    /// </summary>
    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}

/// <summary>
/// Summary of a story path.
/// </summary>
public class PathSummary
{
    /// <summary>
    /// Path identifier.
    /// </summary>
    [JsonPropertyName("path_id")]
    public string PathId { get; set; } = string.Empty;

    /// <summary>
    /// Scenes in this path.
    /// </summary>
    [JsonPropertyName("scenes")]
    public List<string> Scenes { get; set; } = new();

    /// <summary>
    /// Summary of the path.
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Outcome type.
    /// </summary>
    [JsonPropertyName("outcome")]
    public string? Outcome { get; set; }
}

/// <summary>
/// Statistics about a story.
/// </summary>
public class StoryStatistics
{
    /// <summary>
    /// Total number of scenes.
    /// </summary>
    [JsonPropertyName("scene_count")]
    public int SceneCount { get; set; }

    /// <summary>
    /// Number of characters.
    /// </summary>
    [JsonPropertyName("character_count")]
    public int CharacterCount { get; set; }

    /// <summary>
    /// Number of possible paths.
    /// </summary>
    [JsonPropertyName("path_count")]
    public int PathCount { get; set; }

    /// <summary>
    /// Total word count.
    /// </summary>
    [JsonPropertyName("word_count")]
    public int WordCount { get; set; }

    /// <summary>
    /// Number of choice scenes.
    /// </summary>
    [JsonPropertyName("choice_count")]
    public int ChoiceCount { get; set; }

    /// <summary>
    /// Number of ending scenes.
    /// </summary>
    [JsonPropertyName("ending_count")]
    public int EndingCount { get; set; }

    /// <summary>
    /// Average path length.
    /// </summary>
    [JsonPropertyName("avg_path_length")]
    public double AvgPathLength { get; set; }
}
