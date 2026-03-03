using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Authoring.Abstractions.Llm;

/// <summary>
/// LLM service for generating prefix summaries of story paths.
/// Summarizes the story state (entities, time, narrative) at a point along a path.
/// </summary>
public interface IPrefixSummaryLlmService
{
    /// <summary>
    /// Generates a prefix summary for a story path up to a given point.
    /// </summary>
    /// <param name="prefixSceneIds">The scene IDs in the prefix path.</param>
    /// <param name="prefixContent">The concatenated story content for the prefix.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Prefix summary including entity states and narrative summary.</returns>
    Task<ScenarioPathPrefixSummary> SummarizePrefixAsync(
        IEnumerable<string> prefixSceneIds,
        string prefixContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates prefix summaries for multiple paths in batch.
    /// </summary>
    /// <param name="prefixes">Dictionary of identifier to (sceneIds, content) tuples.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of identifier to prefix summary results.</returns>
    Task<IReadOnlyDictionary<string, ScenarioPathPrefixSummary>> SummarizePrefixesAsync(
        IReadOnlyDictionary<string, (IEnumerable<string> SceneIds, string Content)> prefixes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing prefix summary with a new scene.
    /// More efficient than regenerating from scratch.
    /// </summary>
    /// <param name="existingSummary">The existing prefix summary to extend.</param>
    /// <param name="newSceneId">The new scene ID being added.</param>
    /// <param name="newSceneContent">The content of the new scene.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated prefix summary.</returns>
    Task<ScenarioPathPrefixSummary> ExtendSummaryAsync(
        ScenarioPathPrefixSummary existingSummary,
        string newSceneId,
        string newSceneContent,
        CancellationToken cancellationToken = default);
}
