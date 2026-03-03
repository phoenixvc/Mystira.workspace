using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Represents a continuity issue in a story.
/// </summary>
public class ContinuityIssue
{
    /// <summary>
    /// Unique identifier for the issue.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of continuity issue.
    /// </summary>
    [JsonPropertyName("type")]
    public ContinuityIssueType Type { get; set; }

    /// <summary>
    /// Severity of the issue.
    /// </summary>
    [JsonPropertyName("severity")]
    public ContinuityIssueSeverity Severity { get; set; }

    /// <summary>
    /// Scene ID where the issue was detected.
    /// </summary>
    [JsonPropertyName("scene_id")]
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// Related scene IDs.
    /// </summary>
    [JsonPropertyName("related_scene_ids")]
    public List<string>? RelatedSceneIds { get; set; }

    /// <summary>
    /// Description of the issue.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The conflicting or missing element.
    /// </summary>
    [JsonPropertyName("element")]
    public string? Element { get; set; }

    /// <summary>
    /// Expected value or state.
    /// </summary>
    [JsonPropertyName("expected")]
    public string? Expected { get; set; }

    /// <summary>
    /// Actual value or state found.
    /// </summary>
    [JsonPropertyName("actual")]
    public string? Actual { get; set; }

    /// <summary>
    /// Suggested fix for the issue.
    /// </summary>
    [JsonPropertyName("suggested_fix")]
    public string? SuggestedFix { get; set; }

    /// <summary>
    /// Confidence score for this detection (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; } = 1.0;
}

/// <summary>
/// Types of continuity issues.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContinuityIssueType
{
    /// <summary>
    /// Entity referenced before introduction.
    /// </summary>
    EntityNotIntroduced,

    /// <summary>
    /// Entity disappears without explanation.
    /// </summary>
    EntityDisappeared,

    /// <summary>
    /// Entity state conflicts across scenes.
    /// </summary>
    EntityStateConflict,

    /// <summary>
    /// Location continuity error.
    /// </summary>
    LocationInconsistency,

    /// <summary>
    /// Timeline inconsistency.
    /// </summary>
    TemporalInconsistency,

    /// <summary>
    /// Character behavior inconsistency.
    /// </summary>
    CharacterInconsistency,

    /// <summary>
    /// Plot contradiction.
    /// </summary>
    PlotContradiction,

    /// <summary>
    /// Logic error.
    /// </summary>
    LogicError,

    /// <summary>
    /// Cause and effect violation.
    /// </summary>
    CausalityViolation,

    /// <summary>
    /// Other continuity issue.
    /// </summary>
    Other
}

/// <summary>
/// Severity levels for continuity issues.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContinuityIssueSeverity
{
    /// <summary>
    /// Minor issue that doesn't significantly impact the story.
    /// </summary>
    Low,

    /// <summary>
    /// Medium issue that may confuse readers.
    /// </summary>
    Medium,

    /// <summary>
    /// High severity issue that breaks story logic.
    /// </summary>
    High,

    /// <summary>
    /// Critical issue that makes the story unplayable.
    /// </summary>
    Critical
}
