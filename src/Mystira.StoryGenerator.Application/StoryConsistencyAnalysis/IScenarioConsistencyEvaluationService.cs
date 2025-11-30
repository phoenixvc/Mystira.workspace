using Mystira.StoryGenerator.Application.Scenarios;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Service that orchestrates scenario consistency evaluation using parallel execution.
/// Combines path-based consistency checking with entity introduction validation.
/// 
/// The service internally:
/// 1. Classifies all scenes using the LLM entity classifier to extract introduced/removed entities and time deltas
/// 2. Runs path-based consistency evaluation on compressed paths
/// 3. Validates entity introduction violations using the classified entities
/// </summary>
public interface IScenarioConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates scenario consistency in parallel by:
    /// 1. Classifying all scenes via LLM to extract entity introductions, removals, and time deltas
    /// 2. Running path-based consistency evaluation on compressed paths
    /// 3. Validating entity introduction violations in the scenario
    ///
    /// All three operations run concurrently where possible using Task.WhenAll.
    /// </summary>
    /// <param name="graph">The scenario graph to evaluate.</param>
    /// <param name="scenarioPathContent">
    /// Serialized content of the compressed/dominator-based scenario paths
    /// (used for path consistency evaluation).
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A complete evaluation result containing both path consistency
    /// and entity introduction findings, with detailed per-scene classifications.
    /// </returns>
    Task<ScenarioConsistencyEvaluationResult> EvaluateAsync(
        ScenarioGraph graph,
        string scenarioPathContent,
        CancellationToken cancellationToken = default);
}
