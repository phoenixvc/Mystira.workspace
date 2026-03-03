using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service for evaluating entity consistency within scenarios.
/// Analyzes entity introduction, usage, and removal patterns to detect continuity issues.
/// </summary>
public interface IScenarioEntityConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates entity consistency for an entire scenario.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entity introduction evaluation result with violations and classifications.</returns>
    Task<EntityIntroductionEvaluationResult> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates entity consistency for a specific path within a scenario.
    /// </summary>
    /// <param name="scenario">The scenario containing the path.</param>
    /// <param name="path">The scene IDs representing the path to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entity introduction evaluation result for the specified path.</returns>
    Task<EntityIntroductionEvaluationResult> EvaluatePathAsync(
        Scenario scenario,
        IEnumerable<string> path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes entity continuity issues for a scenario, returning detailed issue reports.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of entity continuity issues found.</returns>
    Task<IReadOnlyList<EntityContinuityIssue>> AnalyzeEntityContinuityAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
