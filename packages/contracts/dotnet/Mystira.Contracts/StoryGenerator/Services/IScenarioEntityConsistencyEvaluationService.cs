using Mystira.Contracts.StoryGenerator.Stories;
using Mystira.Contracts.StoryGenerator.StoryConsistency;
using Mystira.Contracts.StoryGenerator.Entities;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for evaluating entity consistency across a scenario.
/// </summary>
public interface IScenarioEntityConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates entity consistency across all paths in a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of entity continuity issues found.</returns>
    Task<IReadOnlyList<EntityContinuityIssue>> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates entity consistency for a specific path.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="path">The path to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of entity continuity issues found in the path.</returns>
    Task<IReadOnlyList<EntityContinuityIssue>> EvaluatePathAsync(
        Scenario scenario,
        ScenarioPath path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts entities from a scenario and builds an entity graph.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity graph for the scenario.</returns>
    Task<EntityGraph> ExtractEntityGraphAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
