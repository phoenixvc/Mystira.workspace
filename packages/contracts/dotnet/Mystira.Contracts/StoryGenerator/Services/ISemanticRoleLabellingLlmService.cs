using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// LLM-based service for semantic role labelling.
/// </summary>
public interface ISemanticRoleLabellingLlmService
{
    /// <summary>
    /// Performs semantic role labelling on text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SRL classification result.</returns>
    Task<SemanticRoleLabellingClassification> LabelAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts predicate-argument structures from text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of predicate frames.</returns>
    Task<IReadOnlyList<PredicateFrame>> ExtractPredicatesAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares SRL structures between two texts for consistency.
    /// </summary>
    /// <param name="text1">First text.</param>
    /// <param name="text2">Second text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of semantic inconsistencies.</returns>
    Task<IReadOnlyList<SemanticRoleInconsistency>> CompareAsync(
        string text1,
        string text2,
        CancellationToken cancellationToken = default);
}
