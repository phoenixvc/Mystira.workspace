using Mystira.Contracts.StoryGenerator.Stories;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for evaluating overall scenario consistency.
/// </summary>
public interface IScenarioConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates the overall consistency of a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The consistency evaluation result.</returns>
    Task<ScenarioConsistencyReport> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates consistency for a specific path through the scenario.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="path">The path to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The consistency evaluation result for the path.</returns>
    Task<ContinuityAnalysisResult> EvaluatePathAsync(
        Scenario scenario,
        ScenarioPath path,
        CancellationToken cancellationToken = default);
}
