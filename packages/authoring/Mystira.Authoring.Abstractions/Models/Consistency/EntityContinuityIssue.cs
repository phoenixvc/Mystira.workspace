using Mystira.Authoring.Abstractions.Models.Entities;

namespace Mystira.Authoring.Abstractions.Models.Consistency;

/// <summary>
/// Issue with entity continuity across scenes.
/// </summary>
public class EntityContinuityIssue
{
    /// <summary>
    /// The entity with the continuity issue.
    /// </summary>
    public SceneEntity Entity { get; set; } = new();

    /// <summary>
    /// Scene where the entity was introduced.
    /// </summary>
    public string IntroducedInSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Scene where the issue was detected.
    /// </summary>
    public string IssueInSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Type of continuity issue.
    /// </summary>
    public EntityContinuityIssueType IssueType { get; set; }

    /// <summary>
    /// Description of the issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Expected value (if applicable).
    /// </summary>
    public string? ExpectedValue { get; set; }

    /// <summary>
    /// Actual value found (if applicable).
    /// </summary>
    public string? ActualValue { get; set; }
}

/// <summary>
/// Type of entity continuity issue.
/// </summary>
public enum EntityContinuityIssueType
{
    Unknown = 0,
    NotIntroduced = 1,
    InconsistentAttribute = 2,
    UnexpectedAbsence = 3,
    UnexpectedPresence = 4,
    NameVariation = 5
}
