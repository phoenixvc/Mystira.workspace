using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Implementation of dominator-based path consistency evaluation service that evaluates
/// consistency across compressed scenario paths using LLM analysis.
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

    public async Task<ConsistencyEvaluationResult?> EvaluateAsync(
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
            var compressedPaths = graph.GetCompressedPaths().ToArray();

            _logger.LogDebug("Generated {PathCount} compressed paths for scenario {ScenarioId}", compressedPaths.Count(), scenario.Id);

            // Serialize paths to content for LLM evaluation
            var scenarioPathContent = SerializePathsToContent(compressedPaths);

            // Evaluate consistency on these paths
            var result = await _consistencyEvaluator.EvaluateConsistencyAsync(scenarioPathContent, cancellationToken);

            if (result != null)
            {
                _logger.LogInformation(
                    "Path consistency evaluation completed for scenario {ScenarioId} with assessment: {Assessment}",
                    scenario.Id,
                    result.OverallAssessment);
            }
            else
            {
                _logger.LogDebug("Path consistency evaluation returned no result for scenario {ScenarioId}", scenario.Id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Path consistency evaluation failed for scenario {ScenarioId}", scenario.Id);
            return null;
        }
    }

    /// <summary>
    /// Serializes compressed scenario paths to content suitable for LLM evaluation.
    /// </summary>
    private static string SerializePathsToContent(IEnumerable<ScenarioPath> paths)
    {
        var pathList = paths.ToList();

        if (pathList.Count == 0)
        {
            return "No paths found in scenario";
        }

        // Combine all paths with a separator
        return string.Join("\n---PATH SEPARATOR---\n", pathList.Select((p, i) => $"Path {i + 1}:\n{p.Story}"));
    }
}
