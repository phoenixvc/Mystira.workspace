using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Contains the result of a complete scenario consistency evaluation,
/// including both path-based consistency checks and entity introduction validation.
/// </summary>
public sealed record ScenarioConsistencyEvaluationResult(
    /// <summary>
    /// Result from the path-based consistency evaluation using LLM on compressed paths.
    /// May be null if evaluation is not configured or failed gracefully.
    /// </summary>
    ConsistencyEvaluationResult? PathConsistencyResult,

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
    IReadOnlyList<ScenarioEntityIntroductionValidator.SceneReferenceViolation> Violations,

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
