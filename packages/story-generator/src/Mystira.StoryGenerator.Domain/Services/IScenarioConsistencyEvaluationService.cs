using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Orchestrator service that evaluates overall scenario consistency by combining
/// entity consistency and path consistency evaluations in parallel.
///
/// The service delegates to specialized services:
/// 1. IScenarioEntityConsistencyEvaluationService - for entity validation
/// 2. IScenarioDominatorPathConsistencyEvaluationService - for path consistency
/// </summary>
public interface IScenarioConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates overall scenario consistency by running entity and path consistency
    /// evaluations in parallel.
    ///
    /// Both evaluations run concurrently using Task.WhenAll.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate for complete consistency.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A complete evaluation result containing both path consistency and entity introduction findings.
    /// </returns>
    Task<ScenarioConsistencyEvaluationResult> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
