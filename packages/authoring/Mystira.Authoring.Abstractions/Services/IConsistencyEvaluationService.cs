using Mystira.Authoring.Abstractions.Models.Consistency;
using Mystira.Authoring.Abstractions.Models.Scenario;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service for evaluating story consistency across paths.
/// </summary>
public interface IConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates consistency of a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Evaluation results for all paths.</returns>
    Task<ScenarioConsistencyResult> EvaluateAsync(Scenario scenario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates consistency of a specific path.
    /// </summary>
    /// <param name="scenario">The scenario containing the path.</param>
    /// <param name="path">Scene IDs representing the path to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Evaluation result for the path.</returns>
    Task<ConsistencyEvaluationResult> EvaluatePathAsync(
        Scenario scenario,
        IEnumerable<string> path,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of scenario-wide consistency evaluation.
/// </summary>
public class ScenarioConsistencyResult
{
    /// <summary>
    /// The scenario that was evaluated.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the scenario is overall consistent.
    /// </summary>
    public bool IsConsistent { get; set; }

    /// <summary>
    /// Overall consistency score (0.0 to 1.0).
    /// </summary>
    public double OverallScore { get; set; }

    /// <summary>
    /// Results for each path evaluated.
    /// </summary>
    public List<ConsistencyEvaluationResult> PathResults { get; set; } = new();

    /// <summary>
    /// Entity continuity issues found.
    /// </summary>
    public List<EntityContinuityIssue> EntityIssues { get; set; } = new();

    /// <summary>
    /// When the evaluation was performed.
    /// </summary>
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}
