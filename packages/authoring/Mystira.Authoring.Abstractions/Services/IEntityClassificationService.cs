using Mystira.Authoring.Abstractions.Models.Entities;
using Mystira.Authoring.Abstractions.Models.Scenario;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service for classifying entities in story scenes.
/// </summary>
public interface IEntityClassificationService
{
    /// <summary>
    /// Classifies entities in a scene's text.
    /// </summary>
    /// <param name="scene">The scene to classify entities from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Classification result with found entities.</returns>
    Task<EntityClassification> ClassifyAsync(Scene scene, CancellationToken cancellationToken = default);

    /// <summary>
    /// Classifies entities in raw text.
    /// </summary>
    /// <param name="text">The text to classify entities from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Classification result with found entities.</returns>
    Task<EntityClassification> ClassifyTextAsync(string text, CancellationToken cancellationToken = default);
}
