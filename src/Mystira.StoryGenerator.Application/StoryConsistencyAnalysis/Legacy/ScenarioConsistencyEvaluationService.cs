using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.Legacy;

/// <summary>
/// Orchestrator service that evaluates overall scenario consistency by combining
/// entity consistency and path consistency evaluations in parallel.
/// </summary>
public class ScenarioConsistencyEvaluationService : IScenarioConsistencyEvaluationService
{
    private readonly IScenarioEntityConsistencyEvaluationService _entityEvaluationService;
    private readonly IScenarioDominatorPathConsistencyEvaluationService _pathEvaluationService;
    private readonly ILogger<ScenarioConsistencyEvaluationService> _logger;

    public ScenarioConsistencyEvaluationService(
        IScenarioEntityConsistencyEvaluationService entityEvaluationService,
        IScenarioDominatorPathConsistencyEvaluationService pathEvaluationService,
        ILogger<ScenarioConsistencyEvaluationService> logger)
    {
        _entityEvaluationService = entityEvaluationService ?? throw new ArgumentNullException(nameof(entityEvaluationService));
        _pathEvaluationService = pathEvaluationService ?? throw new ArgumentNullException(nameof(pathEvaluationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ScenarioConsistencyEvaluationResult> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        try
        {
            _logger.LogInformation("Starting overall scenario consistency evaluation for {ScenarioId}", scenario.Id);

            // Execute both evaluations in parallel
            var entityEvaluationTask = _entityEvaluationService.EvaluateAsync(scenario, cancellationToken);
            var pathEvaluationTask = _pathEvaluationService.EvaluateAsync(scenario, cancellationToken);

            await Task.WhenAll(entityEvaluationTask, pathEvaluationTask);

            var entityIntroductionResult = await entityEvaluationTask;
            var pathConsistencyResults = await pathEvaluationTask;

            var isSuccessful = pathConsistencyResults != null || entityIntroductionResult != null;

            _logger.LogInformation(
                "Scenario consistency evaluation completed for {ScenarioId}. PathConsistency: {PathConsistencyPresent}, EntityIntroduction: {EntityIntroductionPresent}",
                scenario.Id,
                pathConsistencyResults != null,
                entityIntroductionResult != null);

            return new ScenarioConsistencyEvaluationResult(
                pathConsistencyResults,
                entityIntroductionResult,
                isSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scenario consistency evaluation for {ScenarioId}", scenario.Id);
            throw;
        }
    }
}
