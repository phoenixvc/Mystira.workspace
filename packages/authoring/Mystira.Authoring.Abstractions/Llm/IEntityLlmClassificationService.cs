using Mystira.Authoring.Abstractions.Models.Entities;
using Mystira.Authoring.Abstractions.Models.Scenario;

namespace Mystira.Authoring.Abstractions.Llm;

/// <summary>
/// LLM service for classifying entities in story scenes.
/// Identifies introduced entities, removed entities, and time deltas.
/// </summary>
public interface IEntityLlmClassificationService
{
    /// <summary>
    /// Classifies entities in a scene using LLM analysis.
    /// </summary>
    /// <param name="scene">The scene to classify entities from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entity classification result.</returns>
    Task<EntityClassification> ClassifyAsync(
        Scene scene,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Classifies entities in raw text using LLM analysis.
    /// </summary>
    /// <param name="text">The text to classify entities from.</param>
    /// <param name="sceneId">Optional scene ID for context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Entity classification result.</returns>
    Task<EntityClassification> ClassifyTextAsync(
        string text,
        string? sceneId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Classifies entities in multiple scenes in batch.
    /// </summary>
    /// <param name="scenes">The scenes to classify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of scene ID to entity classification results.</returns>
    Task<IReadOnlyDictionary<string, EntityClassification>> ClassifyScenesAsync(
        IEnumerable<Scene> scenes,
        CancellationToken cancellationToken = default);
}
