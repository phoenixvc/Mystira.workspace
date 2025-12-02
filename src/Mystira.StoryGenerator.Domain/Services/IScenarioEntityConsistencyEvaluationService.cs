using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Service that evaluates entity consistency within a scenario using LLM-based classification
/// and graph-theoretic data-flow analysis.
/// 
/// The service:
/// 1. Classifies all scenes via LLM to extract introduced/removed entities and time deltas
/// 2. Validates entity introduction violations using graph-theoretic data-flow analysis
/// </summary>
public interface IScenarioEntityConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates entity consistency in a scenario by:
    /// 1. Classifying all scenes via LLM to extract entity introductions, removals, and time deltas
    /// 2. Validating that entities are introduced on all paths before being used
    ///
    /// All scene classifications run in parallel using Task.WhenAll.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate for entity consistency.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An entity introduction evaluation result containing violations and per-scene classifications.
    /// May return null if evaluation is not configured or failed gracefully.
    /// </returns>
    Task<EntityIntroductionEvaluationResult?> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
