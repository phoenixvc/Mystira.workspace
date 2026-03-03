using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service for computing and managing prefix summaries within scenarios.
/// Orchestrates LLM-based summarization and caching of path prefixes.
/// </summary>
public interface IPrefixSummaryService
{
    /// <summary>
    /// Computes prefix summaries for all dominator paths in a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping scene IDs to their prefix summaries.</returns>
    Task<IReadOnlyDictionary<string, ScenarioPathPrefixSummary>> ComputeDominatorPrefixSummariesAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the prefix summary for a specific path within a scenario.
    /// </summary>
    /// <param name="scenario">The scenario containing the path.</param>
    /// <param name="sceneIds">The ordered scene IDs representing the path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The prefix summary for the specified path.</returns>
    Task<ScenarioPathPrefixSummary> GetPrefixSummaryAsync(
        Scenario scenario,
        IEnumerable<string> sceneIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes prefix summaries for multiple paths in a scenario.
    /// </summary>
    /// <param name="scenario">The scenario containing the paths.</param>
    /// <param name="paths">Collection of paths (each path is a sequence of scene IDs).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping path identifiers to prefix summaries.</returns>
    Task<IReadOnlyDictionary<string, ScenarioPathPrefixSummary>> ComputePrefixSummariesAsync(
        Scenario scenario,
        IEnumerable<IEnumerable<string>> paths,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cached prefix summaries for scenes that have been modified.
    /// </summary>
    /// <param name="scenarioId">The scenario ID.</param>
    /// <param name="modifiedSceneIds">The scene IDs that have been modified.</param>
    void InvalidateSummaries(string scenarioId, IEnumerable<string> modifiedSceneIds);
}
