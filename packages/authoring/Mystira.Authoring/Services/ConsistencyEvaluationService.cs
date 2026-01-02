using Microsoft.Extensions.Logging;
using Mystira.Authoring.Abstractions.Models.Consistency;
using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Authoring.Abstractions.Services;
using Mystira.Authoring.Graph;

namespace Mystira.Authoring.Services;

/// <summary>
/// Implementation of consistency evaluation for scenarios.
/// </summary>
public class ConsistencyEvaluationService : IConsistencyEvaluationService
{
    private readonly IEntityClassificationService? _entityClassifier;
    private readonly ILogger<ConsistencyEvaluationService> _logger;
    private readonly ScenarioGraphBuilder _graphBuilder;

    /// <summary>
    /// Creates a new consistency evaluation service.
    /// </summary>
    public ConsistencyEvaluationService(
        ILogger<ConsistencyEvaluationService> logger,
        IEntityClassificationService? entityClassifier = null)
    {
        _logger = logger;
        _entityClassifier = entityClassifier;
        _graphBuilder = new ScenarioGraphBuilder();
    }

    /// <inheritdoc />
    public async Task<ScenarioConsistencyResult> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        var result = new ScenarioConsistencyResult
        {
            ScenarioId = scenario.Id,
            EvaluatedAt = DateTime.UtcNow
        };

        try
        {
            // Enumerate all paths through the scenario
            var paths = _graphBuilder.EnumerateAllPaths(scenario);

            _logger.LogInformation("Evaluating {PathCount} paths in scenario {ScenarioId}",
                paths.Count, scenario.Id);

            // Evaluate each path
            foreach (var path in paths)
            {
                var pathResult = await EvaluatePathAsync(scenario, path, cancellationToken);
                result.PathResults.Add(pathResult);
            }

            // Calculate overall score
            if (result.PathResults.Count > 0)
            {
                result.OverallScore = result.PathResults.Average(r => r.Score);
                result.IsConsistent = result.PathResults.All(r => r.IsConsistent);
            }
            else
            {
                result.OverallScore = 1.0;
                result.IsConsistent = true;
            }

            // Aggregate entity issues
            var entityIssues = result.PathResults
                .SelectMany(r => r.Issues)
                .Where(i => i.Type == ConsistencyIssueType.EntityInconsistency)
                .Select(i => new EntityContinuityIssue
                {
                    IssueInSceneId = i.SceneId ?? string.Empty,
                    Description = i.Description,
                    IssueType = EntityContinuityIssueType.InconsistentAttribute
                })
                .Distinct()
                .ToList();

            result.EntityIssues = entityIssues;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating scenario consistency");
            result.IsConsistent = false;
            result.OverallScore = 0;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<ConsistencyEvaluationResult> EvaluatePathAsync(
        Scenario scenario,
        IEnumerable<string> path,
        CancellationToken cancellationToken = default)
    {
        var pathList = path.ToList();
        var result = new ConsistencyEvaluationResult
        {
            EvaluatedPath = pathList,
            EvaluatedAt = DateTime.UtcNow
        };

        var sceneMap = scenario.Scenes.ToDictionary(s => s.Id);
        var issues = new List<ConsistencyIssue>();

        // Basic structural validation
        foreach (var sceneId in pathList.Where(sceneId => !sceneMap.ContainsKey(sceneId)))
        {
            issues.Add(new ConsistencyIssue
            {
                Type = ConsistencyIssueType.PlotHole,
                Severity = IssueSeverity.Error,
                Description = $"Scene '{sceneId}' referenced in path but not found in scenario",
                SceneId = sceneId
            });
        }

        // Entity continuity check (if classifier available)
        if (_entityClassifier != null)
        {
            var entitiesIntroduced = new Dictionary<string, string>(); // entity name -> scene introduced

            foreach (var sceneId in pathList.Where(sceneId => sceneMap.ContainsKey(sceneId)))
            {
                var scene = sceneMap[sceneId];
                var classification = await _entityClassifier.ClassifyAsync(scene, cancellationToken);

                foreach (var entity in classification.Entities.Where(entity => !entitiesIntroduced.ContainsKey(entity.Name)))
                {
                    entitiesIntroduced[entity.Name] = sceneId;
                }
            }
        }

        result.Issues = issues;
        result.IsConsistent = !issues.Any(i => i.Severity >= IssueSeverity.Error);
        result.Score = issues.Count == 0 ? 1.0 :
            1.0 - (issues.Count(i => i.Severity >= IssueSeverity.Error) * 0.2);
        result.Score = Math.Max(0, result.Score);

        return result;
    }
}
