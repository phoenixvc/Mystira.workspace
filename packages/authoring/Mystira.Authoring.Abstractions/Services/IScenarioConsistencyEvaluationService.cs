using Mystira.Authoring.Abstractions.Models.Consistency;
using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Main orchestrating service for evaluating scenario consistency.
/// Coordinates entity consistency, SRL analysis, and dominator path evaluation.
/// </summary>
public interface IScenarioConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates full story continuity for a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="request">Evaluation request parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Comprehensive continuity evaluation response.</returns>
    Task<EvaluateStoryContinuityResponse> EvaluateStoryContinuityAsync(
        Scenario scenario,
        EvaluateStoryContinuityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates consistency of a specific path within a scenario.
    /// </summary>
    /// <param name="scenario">The scenario containing the path.</param>
    /// <param name="path">Scene IDs representing the path to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Consistency evaluation result for the path.</returns>
    Task<PathConsistencyResult> EvaluatePathConsistencyAsync(
        Scenario scenario,
        IEnumerable<string> path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all continuity issues for a scenario, optionally filtered.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="filter">Optional filter criteria for issues.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of continuity issues matching the filter.</returns>
    Task<IReadOnlyList<StoryContinuityIssue>> GetContinuityIssuesAsync(
        Scenario scenario,
        StoryContinuityIssueFilter? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a quick validation check without full evaluation.
    /// </summary>
    /// <param name="scenario">The scenario to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if no critical issues are detected.</returns>
    Task<bool> ValidateQuickAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
