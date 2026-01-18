using Mystira.Contracts.StoryGenerator.Entities;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// LLM-based service for classifying and extracting entities from text.
/// </summary>
public interface IEntityLlmClassificationService
{
    /// <summary>
    /// Classifies entities in the given text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity classification result.</returns>
    Task<EntityClassification> ClassifyAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts entities from text with their relationships.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity extraction result.</returns>
    Task<EntityExtractionResult> ExtractEntitiesAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Classifies the type of a specific entity.
    /// </summary>
    /// <param name="entityName">The entity name.</param>
    /// <param name="context">Surrounding context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity type.</returns>
    Task<EntityType> ClassifyEntityTypeAsync(
        string entityName,
        string context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if two entity mentions refer to the same entity (coreference resolution).
    /// </summary>
    /// <param name="mention1">First mention.</param>
    /// <param name="mention2">Second mention.</param>
    /// <param name="context">Surrounding context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the mentions are coreferent.</returns>
    Task<bool> AreCoreferentAsync(
        string mention1,
        string mention2,
        string context,
        CancellationToken cancellationToken = default);
}
