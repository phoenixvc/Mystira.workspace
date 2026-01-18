using Mystira.Contracts.StoryGenerator.Stories;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for evaluating consistency along dominator paths in a scenario graph.
/// Dominator paths represent sequences of scenes that must be traversed.
/// </summary>
public interface IScenarioDominatorPathConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates consistency along all dominator paths in a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The dominator path analysis result.</returns>
    Task<DominatorPathAnalysisResult> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all dominator paths from the start scene to ending scenes.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of dominator paths.</returns>
    Task<IReadOnlyList<ScenarioPath>> GetDominatorPathsAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entity introduction violations in dominator paths.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of entity introduction violations.</returns>
    Task<IReadOnlyList<EntityIntroductionViolation>> FindEntityViolationsAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
