using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Implementation of scenario consistency evaluation service that orchestrates
/// parallel execution of scene entity classification, path consistency checks,
/// and entity introduction validation.
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
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(scenarioPathContent);

        try
        {
            _logger.LogInformation("Starting parallel scenario consistency evaluation");

            // Step 1: Classify all scenes in parallel
            var classificationTask = ClassifyScenesAsync(graph, cancellationToken);

            // Step 2: Evaluate path consistency in parallel with classification
            var pathConsistencyTask = EvaluatePathConsistencyAsync(scenarioPathContent, cancellationToken);

            // Wait for both to complete
            await Task.WhenAll(classificationTask, pathConsistencyTask);

            var sceneClassifications = await classificationTask;
            var pathConsistencyResult = await pathConsistencyTask;

            _logger.LogDebug("Scene classifications and path consistency evaluation completed");

            // Step 3: Validate entity introduction violations using the classifications
            var entityIntroductionResult = await ValidateEntityIntroductionAsync(
                graph,
                sceneClassifications,
                cancellationToken);

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
    /// Validates entity introduction violations using the LLM-based scene classifications.
    /// Uses graph-theoretic data-flow analysis combined with classified entity data.
    /// </summary>
    private async Task<EntityIntroductionEvaluationResult?> ValidateEntityIntroductionAsync(
        ScenarioGraph graph,
        IReadOnlyDictionary<string, SceneEntityClassificationData> sceneClassifications,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Starting entity introduction validation with {ClassificationCount} scene classifications", 
                sceneClassifications.Count);

            // Create lookup functions that use the classified data
            Func<Scene, IEnumerable<SceneEntity>> getIntroduced = scene =>
                sceneClassifications.TryGetValue(scene.Id, out var data)
                    ? data.IntroducedEntities
                    : Enumerable.Empty<SceneEntity>();

            Func<Scene, IEnumerable<SceneEntity>> getRemoved = scene =>
                sceneClassifications.TryGetValue(scene.Id, out var data)
                    ? data.RemovedEntities
                    : Enumerable.Empty<SceneEntity>();

            Func<Scene, IEnumerable<SceneEntity>> getUsed = scene =>
                ExtractUsedEntitiesFromScene(scene, sceneClassifications);

            // Run data-flow analysis to find violations
            var violations = ScenarioEntityIntroductionValidator.FindIntroductionViolations(
                graph,
                getIntroduced,
                getRemoved,
                getUsed);

            _logger.LogDebug("Entity introduction analysis found {ViolationCount} violations", violations.Count);

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
    /// Extracts entities that are used/referenced in a scene.
    /// Uses the classified entities as a source of truth - any entity that appears in the scene
    /// but wasn't introduced or removed in the same scene is considered "used".
    /// </summary>
    private static IEnumerable<SceneEntity> ExtractUsedEntitiesFromScene(
        Scene scene,
        IReadOnlyDictionary<string, SceneEntityClassificationData> sceneClassifications)
    {
        // Combine the scene's text content
        var sceneText = $"{scene.Title} {scene.Description}".ToLowerInvariant();

        // Get all entities mentioned across all scenes' classifications
        var allEntities = new HashSet<SceneEntity>(new SceneEntityComparer());
        foreach (var classification in sceneClassifications.Values)
        {
            foreach (var entity in classification.IntroducedEntities)
                allEntities.Add(entity);
            foreach (var entity in classification.RemovedEntities)
                allEntities.Add(entity);
        }

        // Find which entities are mentioned in this scene's text
        var usedEntities = new List<SceneEntity>();
        foreach (var entity in allEntities)
        {
            if (sceneText.Contains(entity.Name.ToLowerInvariant()))
            {
                usedEntities.Add(entity);
            }
        }

        return usedEntities;
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

    /// <summary>
    /// Equality comparer for SceneEntity based on Type and Name (case-insensitive).
    /// </summary>
    private sealed class SceneEntityComparer : IEqualityComparer<SceneEntity>
    {
        public bool Equals(SceneEntity? x, SceneEntity? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return x.Type == y.Type &&
                   string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(SceneEntity obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + obj.Type.GetHashCode();
                hash = hash * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
                return hash;
            }
        }
    }
}
