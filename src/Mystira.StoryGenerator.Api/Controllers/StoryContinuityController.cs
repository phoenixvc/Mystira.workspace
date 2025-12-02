using Microsoft.AspNetCore.Mvc;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Api.Controllers;

/// <summary>
/// Controller for story continuity evaluation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StoryContinuityController : ControllerBase
{
    private readonly IStoryContinuityService _storyContinuityService;
    private readonly ILogger<StoryContinuityController> _logger;
    private readonly IScenarioFactory _scenarioFactory;

    public StoryContinuityController(
        IStoryContinuityService storyContinuityService,
        IScenarioFactory scenarioFactory,
        ILogger<StoryContinuityController> logger)
    {
        _storyContinuityService = storyContinuityService ?? throw new ArgumentNullException(nameof(storyContinuityService));
        _scenarioFactory = scenarioFactory ?? throw new ArgumentNullException(nameof(scenarioFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluate story continuity for a given scenario.
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<EvaluateStoryContinuityResponse>> Evaluate(
        [FromBody] EvaluateStoryContinuityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new EvaluateStoryContinuityResponse
                {
                    Success = false,
                    Issues = [],
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
                return BadRequest(new EvaluateStoryContinuityResponse
                {
                    Success = false,
                    Issues = [],
                    Error = "Either CurrentStory.Content (JSON) or Scenario must be provided"
                });
            }

            _logger.LogInformation("Evaluating story continuity for scenario {ScenarioId}", scenario.Id);

            var result = await _storyContinuityService.AnalyzeAsync(scenario, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating story continuity");
            return StatusCode(500, new EvaluateStoryContinuityResponse
            {
                Success = false,
                Issues = [],
                Error = "An internal error occurred during continuity evaluation"
            });
        }
    }
}
