using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Contracts.Stories;

/// <summary>
/// Represents a continuity issue found in a story scenario.
/// </summary>
public class StoryContinuityIssue
{
    /// <summary>
    /// The ID of the scene where the issue was detected.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the entity involved in the issue.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// The type of entity (e.g., "character", "location", "item").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The type of continuity issue.
    /// </summary>
    public string IssueType { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the issue.
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// The text span from the story that evidences the issue.
    /// </summary>
    public string EvidenceSpan { get; set; } = string.Empty;

    /// <summary>
    /// Whether the entity is a pronoun.
    /// </summary>
    public bool IsPronoun { get; set; }

    /// <summary>
    /// The confidence level of the issue detection.
    /// </summary>
    public string Confidence { get; set; } = "medium";

    /// <summary>
    /// Semantic roles assigned to this entity in the context.
    /// </summary>
    public string[] SemanticRoles { get; set; } = [];
}

/// <summary>
/// Request to evaluate story continuity for a scenario.
/// </summary>
public class EvaluateStoryContinuityRequest
{
    /// <summary>
    /// The scenario to evaluate.
    /// </summary>
    public Scenario Scenario { get; set; } = new();

    /// <summary>
    /// Optional current chat story snapshot. If provided, the API will prefer parsing the
    /// scenario from <c>CurrentStory.Content</c> (expected to be JSON) and ignore <see cref="Scenario"/>.
    /// </summary>
    public Mystira.StoryGenerator.Contracts.Chat.StorySnapshot? CurrentStory { get; set; }

    /// <summary>
    /// LLM provider for the prefix summary service (optional).
    /// </summary>
    public string? PrefixSummaryProvider { get; set; }

    /// <summary>
    /// LLM model for the prefix summary service (optional).
    /// </summary>
    public string? PrefixSummaryModel { get; set; }

    /// <summary>
    /// LLM provider for the semantic role labelling service (optional).
    /// </summary>
    public string? SemanticRoleProvider { get; set; }

    /// <summary>
    /// LLM model for the semantic role labelling service (optional).
    /// </summary>
    public string? SemanticRoleModel { get; set; }
}

/// <summary>
/// Response containing the continuity issues found.
/// </summary>
public class EvaluateStoryContinuityResponse
{
    /// <summary>
    /// Whether the evaluation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// List of continuity issues found.
    /// </summary>
    public List<StoryContinuityIssue> Issues { get; set; } = [];

    /// <summary>
    /// Error message if the evaluation failed.
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Filter options for continuity issues.
/// </summary>
public class StoryContinuityIssueFilter
{
    /// <summary>
    /// Confidence levels to include (e.g., "high", "medium", "low").
    /// </summary>
    public string[] IncludedConfidences { get; set; } = [];

    /// <summary>
    /// Entity types to include (e.g., "character", "location", "item").
    /// </summary>
    public string[] IncludedEntityTypes { get; set; } = [];

    /// <summary>
    /// If true, only include issues with pronouns.
    /// </summary>
    public bool PronounsOnly { get; set; }
}
