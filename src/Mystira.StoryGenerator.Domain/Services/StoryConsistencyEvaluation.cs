using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Represents the consistency evaluation outcome for a single scenario path.
/// </summary>
public class StoryConsistencyEvaluation
{
    /// <summary>
    /// The sequence of scene ids joined with " -> ", e.g., "scene_1 -> scene_2 -> scene_7".
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The LLM-produced consistency evaluation result for this path.
    /// </summary>
    [JsonPropertyName("result")]
    public ConsistencyEvaluationResult? Result { get; set; }
}
