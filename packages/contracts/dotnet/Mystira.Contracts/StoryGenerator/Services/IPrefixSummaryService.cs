using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for generating and managing prefix summaries of story paths.
/// Prefix summaries capture the narrative state at each point in a story path.
/// </summary>
public interface IPrefixSummaryService
{
    /// <summary>
    /// Generates a prefix summary for a scenario path.
    /// </summary>
    /// <param name="path">The scenario path to summarize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The prefix summary.</returns>
    Task<PrefixSummary> GenerateSummaryAsync(
        ScenarioPath path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates prefix summaries for all paths in a scenario.
    /// </summary>
    /// <param name="paths">The paths to summarize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Map of path ID to prefix summary.</returns>
    Task<Dictionary<string, PrefixSummary>> GenerateSummariesAsync(
        IEnumerable<ScenarioPath> paths,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a prefix summary incrementally with a new scene.
    /// </summary>
    /// <param name="currentSummary">The current prefix summary.</param>
    /// <param name="newSceneNarrative">The narrative of the new scene.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated prefix summary.</returns>
    Task<PrefixSummary> UpdateSummaryAsync(
        PrefixSummary currentSummary,
        string newSceneNarrative,
        CancellationToken cancellationToken = default);
}
