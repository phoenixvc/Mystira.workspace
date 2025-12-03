using Mystira.StoryGenerator.Api.Services.ContinuityAsync;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Api.Services;

/// <summary>
/// Background worker that executes long-running Story Continuity analyses.
/// </summary>
public class ContinuityWorker : BackgroundService
{
    private readonly ILogger<ContinuityWorker> _logger;
    private readonly IContinuityBackgroundQueue _queue;
    private readonly IContinuityOperationStore _store;
    private readonly IStoryContinuityService _continuityService;
    private readonly IScenarioFactory _scenarioFactory;

    public ContinuityWorker(
        ILogger<ContinuityWorker> logger,
        IContinuityBackgroundQueue queue,
        IContinuityOperationStore store,
        IStoryContinuityService continuityService,
        IScenarioFactory scenarioFactory)
    {
        _logger = logger;
        _queue = queue;
        _store = store;
        _continuityService = continuityService;
        _scenarioFactory = scenarioFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ContinuityWorker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            ContinuityJob job;
            try
            {
                job = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            _store.MarkRunning(job.OperationId);
            try
            {
                var req = job.Request;

                // Build scenario, preferring CurrentStory JSON
                Scenario? scenario = null;
                var snapshotJson = req.CurrentStory?.Content;
                if (!string.IsNullOrWhiteSpace(snapshotJson))
                {
                    scenario = await _scenarioFactory.CreateFromContentAsync(snapshotJson!, ScenarioContentFormat.Json, stoppingToken);
                }
                else if (req.Scenario != null)
                {
                    scenario = req.Scenario;
                }

                if (scenario == null)
                {
                    _store.MarkFailed(job.OperationId, "Either CurrentStory.Content (JSON) or Scenario must be provided");
                    continue;
                }

                // Run analysis
                var issues = await _continuityService.AnalyzeAsync(scenario, stoppingToken);
                var mapped = (issues ?? Array.Empty<EntityContinuityIssue>())
                    .Select(i => new StoryContinuityIssue
                    {
                        SceneId = i.SceneId,
                        EntityName = i.EntityName,
                        EntityType = i.EntityType,
                        IssueType = i.IssueType.ToString(),
                        Detail = i.Detail,
                        EvidenceSpan = i.EvidenceSpan ?? string.Empty,
                        IsProperNoun = i.IsProperNoun,
                        Confidence = i.Confidence,
                        SemanticRoles = i.SemanticRoles.ToArray()
                    })
                    .ToList();

                var response = new EvaluateStoryContinuityResponse
                {
                    Success = true,
                    Issues = mapped
                };

                _store.MarkSucceeded(job.OperationId, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Continuity job {OperationId} failed", job.OperationId);
                _store.MarkFailed(job.OperationId, ex.Message);
            }
        }

        _logger.LogInformation("ContinuityWorker stopping");
    }
}
