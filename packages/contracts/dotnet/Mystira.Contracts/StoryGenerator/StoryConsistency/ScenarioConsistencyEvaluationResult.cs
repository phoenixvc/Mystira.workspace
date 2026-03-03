using Mystira.Contracts.StoryGenerator.Entities;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Contains the result of a complete scenario consistency evaluation,
/// including both path-based consistency checks and entity introduction validation.
/// </summary>
public sealed record ScenarioConsistencyEvaluationResult(
    /// <summary>
    /// Results from the path-based consistency evaluation using LLM on each compressed path.
    /// May be null if evaluation is not configured or failed gracefully.
    /// </summary>
    ConsistencyEvaluationResults? PathConsistencyResults,

    /// <summary>
    /// Result from entity introduction validation including LLM-based entity classification.
    /// May be null if validation is not configured or failed gracefully.
    /// </summary>
    EntityIntroductionEvaluationResult? EntityIntroductionResult,

    /// <summary>
    /// Indicates whether the evaluation completed successfully (both parts, if enabled, completed).
    /// </summary>
    bool IsSuccessful)
{
}

/// <summary>
/// Result of entity introduction validation using graph-theoretic data-flow analysis
/// combined with LLM entity classification.
/// </summary>
public sealed record EntityIntroductionEvaluationResult(
    /// <summary>
    /// List of violations where entities are used before being introduced.
    /// </summary>
    IReadOnlyList<SceneReferenceViolation> Violations,

    /// <summary>
    /// Map of scene IDs to their classified entities (introduced, removed, used).
    /// Populated from LLM classification if available.
    /// </summary>
    IReadOnlyDictionary<string, SceneEntityClassificationData> SceneClassifications)
{
}

/// <summary>
/// Holds the classification data for a particular scene extracted via LLM.
/// </summary>
public sealed record SceneEntityClassificationData(
    /// <summary>
    /// Scene identifier.
    /// </summary>
    string SceneId,

    /// <summary>
    /// Time delta in this scene (e.g., "none", "5_minutes", "1_hour").
    /// </summary>
    string TimeDelta,

    /// <summary>
    /// Entities introduced (newly brought into the story).
    /// </summary>
    IReadOnlyList<SceneEntity> IntroducedEntities,

    /// <summary>
    /// Entities removed or forgotten in this scene.
    /// </summary>
    IReadOnlyList<SceneEntity> RemovedEntities)
{
}

/// <summary>
/// Results from path-based consistency evaluation across multiple compressed paths.
/// Each path is evaluated independently in parallel.
/// </summary>
public sealed record ConsistencyEvaluationResults(
    /// <summary>
    /// Collection of per-path consistency evaluation results.
    /// Each result contains the scene IDs for that path and its consistency assessment.
    /// </summary>
    IReadOnlyList<PathConsistencyEvaluationResult> PathResults)
{
}

/// <summary>
/// Consistency evaluation result for a single compressed path.
/// </summary>
public sealed record PathConsistencyEvaluationResult(
    /// <summary>
    /// The sequence of scene IDs that make up this path.
    /// </summary>
    IReadOnlyList<string> SceneIds,

    /// <summary>
    /// The consistency evaluation result for this path.
    /// </summary>
    ConsistencyEvaluationResult? Result,

    /// <summary>
    /// The full story content for this path.
    /// </summary>
    string? PathContent = null)
{
}

/// <summary>
/// Represents a "used before introduced" violation:
/// an entity that is used in a scene but is not present
/// in the "must-have-been introduced" set for that scene.
/// </summary>
public sealed record SceneReferenceViolation(
    string SceneId,
    SceneEntity Entity);
