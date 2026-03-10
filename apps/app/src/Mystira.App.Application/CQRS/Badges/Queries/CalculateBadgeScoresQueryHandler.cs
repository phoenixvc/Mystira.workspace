using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Exceptions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Responses.Badges;
using Mystira.Shared.Exceptions;
using NotFoundException = Mystira.Shared.Exceptions.NotFoundException;
using ValidationException = Mystira.Shared.Exceptions.ValidationException;

namespace Mystira.App.Application.CQRS.Badges.Queries;

/// <summary>
/// Wolverine handler for CalculateBadgeScoresQuery.
/// Calculates required badge scores per tier for all scenarios in a content bundle using depth-first traversal.
/// Graph traversal and percentile calculation are delegated to dedicated utility classes.
/// </summary>
public static class CalculateBadgeScoresQueryHandler
{
    /// <summary>
    /// Handles the CalculateBadgeScoresQuery by calculating per-axis percentile badge scores for all scenarios in a content bundle.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<List<CompassAxisScoreResult>> Handle(
        CalculateBadgeScoresQuery query,
        IContentBundleRepository bundleRepository,
        IScenarioRepository scenarioRepository,
        ILogger logger,
        CancellationToken ct)
    {
        ValidateQuery(query);

        var bundle = await bundleRepository.GetByIdAsync(query.ContentBundleId, ct);
        if (bundle == null)
        {
            throw new NotFoundException("ContentBundle", query.ContentBundleId);
        }

        logger.LogInformation(
            "Calculating badge scores for bundle {BundleId} with {ScenarioCount} scenarios",
            query.ContentBundleId,
            bundle.ScenarioIds.Count);

        var scenarios = await LoadScenariosAsync(bundle.ScenarioIds, scenarioRepository, query.ContentBundleId, logger, ct);

        if (!scenarios.Any())
        {
            logger.LogWarning("No scenarios found for bundle {BundleId}", query.ContentBundleId);
            return new List<CompassAxisScoreResult>();
        }

        var axisPathScores = CollectAxisPathScores(scenarios, logger);
        var results = CalculateAxisPercentiles(axisPathScores, query.Percentiles, logger);

        logger.LogInformation(
            "Badge score calculation complete for bundle {BundleId}: {AxisCount} axes processed",
            query.ContentBundleId,
            results.Count);

        return results;
    }

    private static void ValidateQuery(CalculateBadgeScoresQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.ContentBundleId))
        {
            throw new ValidationException("contentBundleId", "Content bundle ID cannot be null or empty");
        }

        if (query.Percentiles == null || !query.Percentiles.Any())
        {
            throw new ValidationException("percentiles", "Percentiles array cannot be null or empty");
        }

        if (query.Percentiles.Any(p => p < 0 || p > 100))
        {
            throw new ValidationException("percentiles", "Percentiles must be between 0 and 100");
        }
    }

    private static async Task<List<Scenario>> LoadScenariosAsync(
        IReadOnlyCollection<string> scenarioIds,
        IScenarioRepository scenarioRepository,
        string bundleId,
        ILogger logger,
        CancellationToken ct)
    {
        var scenarios = new List<Scenario>();
        foreach (var scenarioId in scenarioIds)
        {
            var scenario = await scenarioRepository.GetByIdAsync(scenarioId, ct);
            if (scenario != null)
            {
                scenarios.Add(scenario);
            }
            else
            {
                logger.LogWarning("Scenario {ScenarioId} not found in bundle {BundleId}", scenarioId, bundleId);
            }
        }
        return scenarios;
    }

    private static Dictionary<string, List<double>> CollectAxisPathScores(List<Scenario> scenarios, ILogger logger)
    {
        var axisPathScores = new Dictionary<string, List<double>>(StringComparer.OrdinalIgnoreCase);

        foreach (var scenario in scenarios)
        {
            logger.LogDebug("Processing scenario {ScenarioId}: {Title}", scenario.Id, scenario.Title);

            var scenarioPaths = ScenarioGraphTraversal.TraverseScenario(scenario);

            logger.LogDebug("Found {PathCount} paths in scenario {ScenarioId}", scenarioPaths.Count, scenario.Id);

            foreach (var path in scenarioPaths)
            {
                foreach (var (axis, score) in path)
                {
                    if (!axisPathScores.ContainsKey(axis))
                    {
                        axisPathScores[axis] = new List<double>();
                    }
                    axisPathScores[axis].Add(score);
                }
            }
        }

        return axisPathScores;
    }

    private static List<CompassAxisScoreResult> CalculateAxisPercentiles(
        Dictionary<string, List<double>> axisPathScores,
        List<double> percentiles,
        ILogger logger)
    {
        var results = new List<CompassAxisScoreResult>();

        foreach (var (axisName, scores) in axisPathScores)
        {
            if (!scores.Any())
            {
                continue;
            }

            var percentileScores = PercentileCalculator.CalculatePercentiles(scores, percentiles);

            results.Add(new CompassAxisScoreResult
            {
                AxisName = axisName,
                PercentileScores = percentileScores
            });

            logger.LogInformation(
                "Calculated percentiles for axis {AxisName}: {PathCount} paths analyzed",
                axisName,
                scores.Count);
        }

        return results;
    }
}
