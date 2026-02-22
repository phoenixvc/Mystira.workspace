using Microsoft.AspNetCore.Mvc;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Api.Controllers;

/// <summary>
/// Controller for dominator-based path consistency evaluation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScenarioDominatorPathAnalysisController : ControllerBase
{
    private readonly IScenarioDominatorPathConsistencyEvaluationService _pathConsistencyService;
    private readonly ILogger<ScenarioDominatorPathAnalysisController> _logger;
    private readonly IScenarioFactory _scenarioFactory;

    public ScenarioDominatorPathAnalysisController(
        IScenarioDominatorPathConsistencyEvaluationService pathConsistencyService,
        IScenarioFactory scenarioFactory,
        ILogger<ScenarioDominatorPathAnalysisController> logger)
    {
        _pathConsistencyService = pathConsistencyService ?? throw new ArgumentNullException(nameof(pathConsistencyService));
        _scenarioFactory = scenarioFactory ?? throw new ArgumentNullException(nameof(scenarioFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluate dominator-based path consistency for a given scenario.
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<EvaluateDominatorPathConsistencyResponse>> Evaluate(
        [FromBody] EvaluateDominatorPathConsistencyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new EvaluateDominatorPathConsistencyResponse
                {
                    Success = false,
                    PathResults = [],
                    Error = "Request body is required"
                });
            }

            // Prefer the current chat story snapshot JSON if provided
            Scenario? scenario = null;
            var snapshotJson = request.CurrentStory?.Content;
            if (!string.IsNullOrWhiteSpace(snapshotJson))
            {
                scenario = await _scenarioFactory.CreateFromContentAsync(snapshotJson!, ScenarioContentFormat.Json, cancellationToken);
            }
            else if (request.Scenario != null)
            {
                scenario = request.Scenario;
            }

            if (scenario == null)
            {
                return BadRequest(new EvaluateDominatorPathConsistencyResponse
                {
                    Success = false,
                    PathResults = [],
                    Error = "Either CurrentStory.Content (JSON) or Scenario must be provided"
                });
            }

            _logger.LogInformation("Evaluating dominator path consistency for scenario {ScenarioId}", scenario.Id);

            var result = await _pathConsistencyService.EvaluateAsync(scenario, cancellationToken);

            if (result == null)
            {
                return StatusCode(500, new EvaluateDominatorPathConsistencyResponse
                {
                    Success = false,
                    PathResults = [],
                    Error = "Path consistency evaluation returned no result"
                });
            }

            // Map ConsistencyEvaluationResults to response DTOs
            var pathResults = (result.PathResults ?? new List<PathConsistencyEvaluationResult>())
                .Select((pr, idx) => new DominatorPathAnalysisResult
                {
                    Path = string.Join(" -> ", pr.SceneIds),
                    PathContent = pr.PathContent ?? string.Empty,
                    Result = pr.Result == null ? null : new DominatorPathEvaluationResult
                    {
                        OverallAssessment = pr.Result.OverallAssessment,
                        Issues = (pr.Result.Issues ?? new List<ConsistencyIssue>())
                            .Select(i => new DominatorPathAnalysisIssue
                            {
                                Severity = i.Severity,
                                Category = i.Category,
                                SceneIds = i.SceneIds ?? new List<string>(),
                                Summary = i.Summary,
                                SuggestedFix = i.SuggestedFix ?? string.Empty
                            })
                            .ToList()
                    }
                })
                .ToList();

            var response = new EvaluateDominatorPathConsistencyResponse
            {
                Success = true,
                PathResults = pathResults,
                Error = null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating dominator path consistency");
            return StatusCode(500, new EvaluateDominatorPathConsistencyResponse
            {
                Success = false,
                PathResults = [],
                Error = "An internal error occurred during path consistency evaluation"
            });
        }
    }
}
