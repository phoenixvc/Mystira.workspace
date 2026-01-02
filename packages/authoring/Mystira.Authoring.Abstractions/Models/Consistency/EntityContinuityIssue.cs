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
    /// <summary>Unknown issue type.</summary>
    Unknown = 0,
    /// <summary>Entity referenced before being introduced.</summary>
    NotIntroduced = 1,
    /// <summary>Entity attribute changed inconsistently.</summary>
    InconsistentAttribute = 2,
    /// <summary>Entity unexpectedly absent from a scene.</summary>
    UnexpectedAbsence = 3,
    /// <summary>Entity unexpectedly present in a scene.</summary>
    UnexpectedPresence = 4,
    /// <summary>Entity name varies across scenes.</summary>
    NameVariation = 5
}
