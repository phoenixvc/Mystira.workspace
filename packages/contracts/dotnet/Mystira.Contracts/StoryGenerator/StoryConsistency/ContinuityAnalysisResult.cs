using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Result of continuity analysis for a scenario.
/// </summary>
public class ContinuityAnalysisResult
{
    /// <summary>
    /// Whether the analysis passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    /// <summary>
    /// Overall continuity score (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }

    /// <summary>
    /// All continuity issues found.
    /// </summary>
    [JsonPropertyName("issues")]
    public List<ContinuityIssue> Issues { get; set; } = new();

    /// <summary>
    /// Count of critical issues.
    /// </summary>
    [JsonPropertyName("critical_count")]
    public int CriticalCount { get; set; }

    /// <summary>
    /// Count of high severity issues.
    /// </summary>
    [JsonPropertyName("high_count")]
    public int HighCount { get; set; }

    /// <summary>
    /// Count of medium severity issues.
    /// </summary>
    [JsonPropertyName("medium_count")]
    public int MediumCount { get; set; }

    /// <summary>
    /// Count of low severity issues.
    /// </summary>
    [JsonPropertyName("low_count")]
    public int LowCount { get; set; }

    /// <summary>
    /// Summary of the analysis.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Paths that were analyzed.
    /// </summary>
    [JsonPropertyName("paths_analyzed")]
    public int PathsAnalyzed { get; set; }

    /// <summary>
    /// Duration of analysis in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; set; }
}
