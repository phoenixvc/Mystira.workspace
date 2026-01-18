using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// LLM-based service for generating prefix summaries.
/// </summary>
public interface IPrefixSummaryLlmService
{
    /// <summary>
    /// Generates a prefix summary for a narrative.
    /// </summary>
    /// <param name="narrative">The narrative text to summarize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The prefix summary.</returns>
    Task<PrefixSummary> GenerateAsync(
        string narrative,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a prefix summary with additional narrative.
    /// </summary>
    /// <param name="currentSummary">The current summary.</param>
    /// <param name="additionalNarrative">New narrative to incorporate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated prefix summary.</returns>
    Task<PrefixSummary> UpdateAsync(
        PrefixSummary currentSummary,
        string additionalNarrative,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges multiple prefix summaries into one.
    /// </summary>
    /// <param name="summaries">Summaries to merge.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The merged summary.</returns>
    Task<PrefixSummary> MergeAsync(
        IEnumerable<PrefixSummary> summaries,
        CancellationToken cancellationToken = default);
}
