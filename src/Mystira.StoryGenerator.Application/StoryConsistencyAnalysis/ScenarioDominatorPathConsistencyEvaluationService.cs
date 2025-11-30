using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Implementation of dominator-based path consistency evaluation service that evaluates
/// consistency across compressed scenario paths using LLM analysis, in parallel.
/// </summary>
public class ScenarioDominatorPathConsistencyEvaluationService : IScenarioDominatorPathConsistencyEvaluationService
{
    private readonly ILlmConsistencyEvaluator _consistencyEvaluator;
    private readonly ILogger<ScenarioDominatorPathConsistencyEvaluationService> _logger;

    public ScenarioDominatorPathConsistencyEvaluationService(
        ILlmConsistencyEvaluator consistencyEvaluator,
        ILogger<ScenarioDominatorPathConsistencyEvaluationService> logger)
    {
        _consistencyEvaluator = consistencyEvaluator ?? throw new ArgumentNullException(nameof(consistencyEvaluator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConsistencyEvaluationResults?> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        try
        {
            _logger.LogInformation("Starting dominator-based path consistency evaluation for scenario {ScenarioId}", scenario.Id);

            // Build the scenario graph
            var graph = ScenarioGraph.FromScenario(scenario);

            // Get compressed paths based on dominators
            var compressedPaths = graph.GetCompressedPaths().ToList();

            _logger.LogDebug("Generated {PathCount} compressed paths for scenario {ScenarioId}", compressedPaths.Count, scenario.Id);

            if (compressedPaths.Count == 0)
            {
                _logger.LogWarning("No paths generated for scenario {ScenarioId}", scenario.Id);
                return null;
            }

            // Evaluate each path in parallel
            var evaluationTasks = compressedPaths
                .Select((path, index) => EvaluatePathAsync(path, index + 1, cancellationToken))
                .ToList();

            var pathResults = await Task.WhenAll(evaluationTasks);

            _logger.LogInformation(
                "Path consistency evaluation completed for scenario {ScenarioId}. Evaluated {PathCount} paths",
                scenario.Id,
                pathResults.Length);

            return new ConsistencyEvaluationResults(pathResults);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Path consistency evaluation failed for scenario {ScenarioId}", scenario.Id);
            return null;
        }
    }

    /// <summary>
    /// Evaluates consistency for a single compressed path.
    /// </summary>
    private async Task<PathConsistencyEvaluationResult> EvaluatePathAsync(
        ScenarioPath path,
        int pathIndex,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Evaluating consistency for path {PathIndex} with {SceneCount} scenes", 
                pathIndex, path.SceneIds.Count);

            // Serialize this single path to content for LLM evaluation
            var pathContent = SerializePathContent(path, pathIndex);

            // Evaluate consistency on this path
            var result = await _consistencyEvaluator.EvaluateConsistencyAsync(pathContent, cancellationToken);

            if (result != null)
            {
                _logger.LogDebug(
                    "Path {PathIndex} consistency evaluation completed with assessment: {Assessment}",
                    pathIndex,
                    result.OverallAssessment);
            }
            else
            {
                _logger.LogDebug("Path {PathIndex} consistency evaluation returned no result", pathIndex);
            }

            return new PathConsistencyEvaluationResult(path.SceneIds, result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to evaluate consistency for path {PathIndex}", pathIndex);
            return new PathConsistencyEvaluationResult(path.SceneIds, null);
        }
    }

    /// <summary>
    /// Serializes a single scenario path to content suitable for LLM evaluation.
    /// </summary>
    private static string SerializePathContent(ScenarioPath path, int pathIndex)
    {
        return $"Path {pathIndex}:\n{path.Story}";
    }
}
