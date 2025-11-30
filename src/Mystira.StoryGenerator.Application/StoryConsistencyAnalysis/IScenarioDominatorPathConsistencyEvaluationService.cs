using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Service that evaluates path consistency within a scenario using LLM-based analysis
/// on dominator-based compressed paths.
/// 
/// The service:
/// 1. Constructs the scenario graph from the scenario
/// 2. Generates dominator-based compressed paths
/// 3. Uses LLM to evaluate consistency across these paths
/// </summary>
public interface IScenarioDominatorPathConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates path consistency in a scenario by:
    /// 1. Constructing the scenario graph
    /// 2. Generating dominator-based compressed paths
    /// 3. Using LLM to evaluate consistency across these paths
    ///
    /// Returns overall assessment and detailed consistency issues if any.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate for path consistency.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A consistency evaluation result containing overall assessment and detailed issues.
    /// May return null if evaluation is not configured or failed gracefully.
    /// </returns>
    Task<ConsistencyEvaluationResult?> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
