using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Implementation of scenario consistency evaluation service that orchestrates
/// parallel execution of path consistency checks and entity introduction validation.
/// </summary>
public class ScenarioConsistencyEvaluationService : IScenarioConsistencyEvaluationService
{
    private readonly ILlmConsistencyEvaluator _consistencyEvaluator;
    private readonly IEntityLlmClassificationService _entityClassifier;
    private readonly ILogger<ScenarioConsistencyEvaluationService> _logger;

    public ScenarioConsistencyEvaluationService(
        ILlmConsistencyEvaluator consistencyEvaluator,
        IEntityLlmClassificationService entityClassifier,
        ILogger<ScenarioConsistencyEvaluationService> logger)
    {
        _consistencyEvaluator = consistencyEvaluator ?? throw new ArgumentNullException(nameof(consistencyEvaluator));
        _entityClassifier = entityClassifier ?? throw new ArgumentNullException(nameof(entityClassifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ScenarioConsistencyEvaluationResult> EvaluateAsync(
        ScenarioGraph graph,
        string scenarioPathContent,
        Func<Scene, IEnumerable<SceneEntity>> getIntroduced,
        Func<Scene, IEnumerable<SceneEntity>> getRemoved,
        Func<Scene, IEnumerable<SceneEntity>> getUsed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(scenarioPathContent);
        ArgumentNullException.ThrowIfNull(getIntroduced);
        ArgumentNullException.ThrowIfNull(getRemoved);
        ArgumentNullException.ThrowIfNull(getUsed);

        try
        {
            _logger.LogInformation("Starting parallel scenario consistency evaluation");

            // Execute both evaluations in parallel
            var pathConsistencyTask = EvaluatePathConsistencyAsync(scenarioPathContent, cancellationToken);
            var entityIntroductionTask = ValidateEntityIntroductionAsync(graph, getIntroduced, getRemoved, getUsed, cancellationToken);

            await Task.WhenAll(pathConsistencyTask, entityIntroductionTask);

            var pathConsistencyResult = await pathConsistencyTask;
            var entityIntroductionResult = await entityIntroductionTask;

            var isSuccessful = pathConsistencyResult != null || entityIntroductionResult != null;

            _logger.LogInformation(
                "Scenario consistency evaluation completed. PathConsistency: {PathConsistencyPresent}, EntityIntroduction: {EntityIntroductionPresent}",
                pathConsistencyResult != null,
                entityIntroductionResult != null);

            return new ScenarioConsistencyEvaluationResult(
                pathConsistencyResult,
                entityIntroductionResult,
                isSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scenario consistency evaluation");
            throw;
        }
    }

    /// <summary>
    /// Evaluates consistency on the compressed/dominator-based scenario paths
    /// using the LLM-based consistency evaluator.
    /// </summary>
    private async Task<ConsistencyEvaluationResult?> EvaluatePathConsistencyAsync(
        string scenarioPathContent,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Starting path consistency evaluation");
            var result = await _consistencyEvaluator.EvaluateConsistencyAsync(scenarioPathContent, cancellationToken);

            if (result != null)
            {
                _logger.LogDebug(
                    "Path consistency evaluation completed with assessment: {Assessment}",
                    result.OverallAssessment);
            }
            else
            {
                _logger.LogDebug("Path consistency evaluation returned no result");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Path consistency evaluation failed, returning null result");
            return null;
        }
    }

    /// <summary>
    /// Validates entity introduction violations using graph-theoretic data-flow analysis.
    /// Also performs LLM-based entity classification to extract scene entities.
    /// </summary>
    private async Task<EntityIntroductionEvaluationResult?> ValidateEntityIntroductionAsync(
        ScenarioGraph graph,
        Func<Scene, IEnumerable<SceneEntity>> getIntroduced,
        Func<Scene, IEnumerable<SceneEntity>> getRemoved,
        Func<Scene, IEnumerable<SceneEntity>> getUsed,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Starting entity introduction validation");

            // Run data-flow analysis to find violations
            var violations = ScenarioEntityIntroductionValidator.FindIntroductionViolations(
                graph,
                getIntroduced,
                getRemoved,
                getUsed);

            _logger.LogDebug("Entity introduction analysis found {ViolationCount} violations", violations.Count);

            // Classify entities in each scene using LLM (in parallel)
            var sceneClassifications = await ClassifyScenesAsync(graph, cancellationToken);

            return new EntityIntroductionEvaluationResult(
                violations,
                sceneClassifications);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Entity introduction validation failed, returning null result");
            return null;
        }
    }

    /// <summary>
    /// Classifies entities in all scenes of the scenario using the entity classifier.
    /// Classification tasks are executed in parallel.
    /// </summary>
    private async Task<IReadOnlyDictionary<string, SceneEntityClassificationData>> ClassifyScenesAsync(
        ScenarioGraph graph,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Starting scene entity classification for {SceneCount} scenes", graph.Nodes.Count);

            // Create classification tasks for each scene
            var classificationTasks = graph.Nodes
                .Select(scene => ClassifySceneAsync(scene, cancellationToken))
                .ToList();

            // Execute all classifications in parallel
            var results = await Task.WhenAll(classificationTasks);

            // Convert to dictionary, filtering out null results
            var sceneClassifications = results
                .Where(r => r != null)
                .ToDictionary(r => r!.SceneId, r => r!);

            _logger.LogDebug("Scene entity classification completed for {ClassificationCount} scenes", sceneClassifications.Count);

            return sceneClassifications;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Scene entity classification failed, returning empty dictionary");
            return new Dictionary<string, SceneEntityClassificationData>();
        }
    }

    /// <summary>
    /// Classifies a single scene using the entity classifier.
    /// </summary>
    private async Task<SceneEntityClassificationData?> ClassifySceneAsync(
        Scene scene,
        CancellationToken cancellationToken)
    {
        try
        {
            // Serialize scene to content string for classification
            var sceneContent = SerializeSceneContent(scene);

            var classification = await _entityClassifier.ClassifyAsync(sceneContent, cancellationToken);

            if (classification == null)
            {
                _logger.LogDebug("Scene {SceneId} classification returned null", scene.Id);
                return null;
            }

            var data = new SceneEntityClassificationData(
                scene.Id,
                classification.TimeDelta,
                classification.IntroducedEntities.ToList(),
                classification.RemovedEntities.ToList());

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to classify scene {SceneId}", scene.Id);
            return null;
        }
    }

    /// <summary>
    /// Serializes a scene to a string format suitable for LLM entity classification.
    /// </summary>
    private static string SerializeSceneContent(Scene scene)
    {
        return $@"Scene ID: {scene.Id}
Title: {scene.Title}
Type: {scene.Type}
Description: {scene.Description}";
    }
}
