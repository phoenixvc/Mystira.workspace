using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service for performing semantic role labelling (SRL) analysis at the scenario level.
/// Orchestrates LLM-based SRL analysis and aggregates results across scenes.
/// </summary>
public interface IScenarioSrlAnalysisService
{
    /// <summary>
    /// Analyzes all scenes in a scenario using semantic role labelling.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping scene IDs to their SRL classifications.</returns>
    Task<IReadOnlyDictionary<string, SemanticRoleLabellingClassification>> AnalyzeScenarioAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a specific path within a scenario using semantic role labelling.
    /// </summary>
    /// <param name="scenario">The scenario containing the path.</param>
    /// <param name="sceneIds">The ordered scene IDs representing the path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping scene IDs to their SRL classifications.</returns>
    Task<IReadOnlyDictionary<string, SemanticRoleLabellingClassification>> AnalyzePathAsync(
        Scenario scenario,
        IEnumerable<string> sceneIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or computes SRL classification for a specific scene.
    /// Uses caching when available.
    /// </summary>
    /// <param name="scenario">The scenario containing the scene.</param>
    /// <param name="sceneId">The ID of the scene to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SRL classification for the scene.</returns>
    Task<SemanticRoleLabellingClassification> GetSceneClassificationAsync(
        Scenario scenario,
        string sceneId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cached SRL analysis results for scenes that have been modified.
    /// </summary>
    /// <param name="scenarioId">The scenario ID.</param>
    /// <param name="modifiedSceneIds">The scene IDs that have been modified.</param>
    void InvalidateCache(string scenarioId, IEnumerable<string> modifiedSceneIds);
}
