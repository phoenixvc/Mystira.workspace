using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Implementation of scenario entity consistency evaluation service that orchestrates
/// parallel execution of scene entity classification and entity introduction validation.
/// </summary>
public class ScenarioEntityConsistencyEvaluationService : IScenarioEntityConsistencyEvaluationService
{
    private readonly IEntityLlmClassificationService _entityClassifier;
    private readonly ILogger<ScenarioEntityConsistencyEvaluationService> _logger;

    public ScenarioEntityConsistencyEvaluationService(
        IEntityLlmClassificationService entityClassifier,
        ILogger<ScenarioEntityConsistencyEvaluationService> logger)
    {
        _entityClassifier = entityClassifier ?? throw new ArgumentNullException(nameof(entityClassifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EntityIntroductionEvaluationResult?> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        try
        {
            _logger.LogInformation("Starting entity consistency evaluation for scenario {ScenarioId}", scenario.Id);

            // Build the scenario graph
            var graph = ScenarioGraph.FromScenario(scenario);

            // Classify all scenes in parallel
            var sceneClassifications = await ClassifyScenesAsync(graph, cancellationToken);

            _logger.LogDebug("Scene classifications completed for {ClassificationCount} scenes", sceneClassifications.Count);

            // Validate entity introduction violations using the classifications
            var entityIntroductionResult = ValidateEntityIntroduction(graph, sceneClassifications);

            _logger.LogInformation(
                "Entity consistency evaluation completed with {ViolationCount} violations",
                entityIntroductionResult.Violations.Count);

            return entityIntroductionResult;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Entity consistency evaluation failed for scenario {ScenarioId}", scenario.Id);
            return null;
        }
    }

    /// <summary>
    /// Validates entity introduction violations using the LLM-based scene classifications.
    /// Uses graph-theoretic data-flow analysis combined with classified entity data.
    /// </summary>
    private EntityIntroductionEvaluationResult ValidateEntityIntroduction(
        ScenarioGraph graph,
        IReadOnlyDictionary<string, SceneEntityClassificationData> sceneClassifications)
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
                ExtractUsedEntitiesFromScene(scene, sceneClassifications, getRemoved);

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
            _logger.LogWarning(ex, "Entity introduction validation failed, returning empty result");
            return new EntityIntroductionEvaluationResult(
                new List<ScenarioEntityIntroductionValidator.SceneReferenceViolation>(),
                sceneClassifications);
        }
    }

    /// <summary>
    /// Classifies entities in all scenes of the scenario using the entity classifier.
    /// Classification tasks are executed in parallel.
    /// Returns a dictionary mapping scene IDs to classification results.
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
    private static IEnumerable<SceneEntity> ExtractUsedEntitiesFromScene(Scene scene,
        IReadOnlyDictionary<string, SceneEntityClassificationData> sceneClassifications,
        Func<Scene, IEnumerable<SceneEntity>> getRemoved)
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

        var removedEntities = getRemoved(scene).ToArray();
        // Find which entities are mentioned in this scene's text
        var usedEntities = new List<SceneEntity>();
        foreach (var entity in allEntities)
        {
            // If the entity was removed in this scene, it's not used'
            var wasRemoved = removedEntities.Any(e => e.Type == entity.Type && e.Name == entity.Name);
            if (wasRemoved)
                continue;
            // Otherwise, check if the entity name appears in the scene text
            if (sceneText.Contains(entity.Name.ToLowerInvariant()))
                usedEntities.Add(entity);
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
