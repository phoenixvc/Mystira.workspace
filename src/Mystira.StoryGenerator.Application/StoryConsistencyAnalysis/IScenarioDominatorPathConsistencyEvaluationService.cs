using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Service that evaluates path consistency within a scenario using LLM-based analysis
/// on dominator-based compressed paths.
/// 
/// The service:
/// 1. Constructs the scenario graph from the scenario
/// 2. Generates dominator-based compressed paths
/// 3. Evaluates consistency for each path in parallel using LLM
/// 4. Returns detailed per-path results
/// </summary>
public interface IScenarioDominatorPathConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates path consistency in a scenario by:
    /// 1. Constructing the scenario graph
    /// 2. Generating dominator-based compressed paths
    /// 3. Evaluating consistency for each path independently in parallel
    ///
    /// Returns detailed per-path assessment and consistency issues.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate for path consistency.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A consistency evaluation results object containing per-path consistency assessments.
    /// May return null if evaluation is not configured or failed gracefully.
    /// </returns>
    Task<ConsistencyEvaluationResults?> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
