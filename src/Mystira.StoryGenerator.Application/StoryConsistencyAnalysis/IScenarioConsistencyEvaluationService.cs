using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Entities;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Service that orchestrates scenario consistency evaluation using parallel execution.
/// Combines path-based consistency checking with entity introduction validation.
/// </summary>
public interface IScenarioConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates scenario consistency in parallel by:
    /// 1. Running path-based consistency evaluation on compressed paths
    /// 2. Validating entity introduction violations in the scenario
    ///
    /// Both evaluations run concurrently using Task.WhenAll.
    /// </summary>
    /// <param name="graph">The scenario graph to evaluate.</param>
    /// <param name="scenarioPathContent">
    /// Serialized content of the compressed/dominator-based scenario paths
    /// (used for path consistency evaluation).
    /// </param>
    /// <param name="getIntroduced">
    /// Function to extract entities introduced in a given scene.
    /// </param>
    /// <param name="getRemoved">
    /// Function to extract entities removed/forgotten in a given scene.
    /// </param>
    /// <param name="getUsed">
    /// Function to extract entities used/referenced in a given scene.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A complete evaluation result containing both path consistency
    /// and entity introduction findings.
    /// </returns>
    Task<ScenarioConsistencyEvaluationResult> EvaluateAsync(
        ScenarioGraph graph,
        string scenarioPathContent,
        Func<Scene, IEnumerable<SceneEntity>> getIntroduced,
        Func<Scene, IEnumerable<SceneEntity>> getRemoved,
        Func<Scene, IEnumerable<SceneEntity>> getUsed,
        CancellationToken cancellationToken = default);
}
