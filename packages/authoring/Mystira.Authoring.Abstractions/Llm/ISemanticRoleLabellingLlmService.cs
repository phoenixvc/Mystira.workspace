using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Authoring.Abstractions.Llm;

/// <summary>
/// LLM service for semantic role labelling (SRL) analysis of story scenes.
/// Classifies entities in scenes based on their semantic roles (agent, patient, experiencer, etc.)
/// and tracks their introduction/removal status.
/// </summary>
public interface ISemanticRoleLabellingLlmService
{
    /// <summary>
    /// Analyzes a scene's text to classify entities using semantic role labelling.
    /// </summary>
    /// <param name="sceneId">The ID of the scene being analyzed.</param>
    /// <param name="sceneContent">The text content of the scene.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SRL classification result containing entity classifications.</returns>
    Task<SemanticRoleLabellingClassification> AnalyzeSceneAsync(
        string sceneId,
        string sceneContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes multiple scenes in batch for semantic role labelling.
    /// </summary>
    /// <param name="scenes">Dictionary of scene ID to scene content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of scene ID to SRL classification results.</returns>
    Task<IReadOnlyDictionary<string, SemanticRoleLabellingClassification>> AnalyzeScenesAsync(
        IReadOnlyDictionary<string, string> scenes,
        CancellationToken cancellationToken = default);
}
