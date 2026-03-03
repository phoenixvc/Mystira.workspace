using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Comprehensive consistency report for a scenario.
/// </summary>
public class ScenarioConsistencyReport
{
    /// <summary>
    /// Report ID.
    /// </summary>
    [JsonPropertyName("report_id")]
    public string ReportId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Scenario ID being evaluated.
    /// </summary>
    [JsonPropertyName("scenario_id")]
    public string? ScenarioId { get; set; }

    /// <summary>
    /// When the report was generated.
    /// </summary>
    [JsonPropertyName("generated_at")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the scenario passed all consistency checks.
    /// </summary>
    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    /// <summary>
    /// Overall consistency score (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("overall_score")]
    public double OverallScore { get; set; }

    /// <summary>
    /// Entity consistency score.
    /// </summary>
    [JsonPropertyName("entity_score")]
    public double EntityScore { get; set; }

    /// <summary>
    /// Narrative consistency score.
    /// </summary>
    [JsonPropertyName("narrative_score")]
    public double NarrativeScore { get; set; }

    /// <summary>
    /// Structural consistency score.
    /// </summary>
    [JsonPropertyName("structure_score")]
    public double StructureScore { get; set; }

    /// <summary>
    /// Entity continuity issues.
    /// </summary>
    [JsonPropertyName("entity_issues")]
    public List<EntityContinuityIssue> EntityIssues { get; set; } = new();

    /// <summary>
    /// General continuity issues.
    /// </summary>
    [JsonPropertyName("continuity_issues")]
    public List<ContinuityIssue> ContinuityIssues { get; set; } = new();

    /// <summary>
    /// Entity introduction violations.
    /// </summary>
    [JsonPropertyName("introduction_violations")]
    public List<EntityIntroductionViolation> IntroductionViolations { get; set; } = new();

    /// <summary>
    /// Dominator path analysis result.
    /// </summary>
    [JsonPropertyName("dominator_analysis")]
    public DominatorPathAnalysisResult? DominatorAnalysis { get; set; }

    /// <summary>
    /// Prefix summaries for paths.
    /// </summary>
    [JsonPropertyName("prefix_summaries")]
    public List<PrefixSummary>? PrefixSummaries { get; set; }

    /// <summary>
    /// Summary of the report.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Recommendations for improvement.
    /// </summary>
    [JsonPropertyName("recommendations")]
    public List<string>? Recommendations { get; set; }

    /// <summary>
    /// Total duration of analysis in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; set; }
}
