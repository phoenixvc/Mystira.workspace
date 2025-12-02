using Microsoft.AspNetCore.Mvc;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Contracts.Stories;

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

    public StoryContinuityController(
        IStoryContinuityService storyContinuityService,
        ILogger<StoryContinuityController> logger)
    {
        _storyContinuityService = storyContinuityService ?? throw new ArgumentNullException(nameof(storyContinuityService));
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
            if (request?.Scenario == null)
            {
                return BadRequest(new EvaluateStoryContinuityResponse
                {
                    Success = false,
                    Issues = [],
                    Error = "Scenario is required"
                });
            }

            _logger.LogInformation("Evaluating story continuity for scenario {ScenarioId}", request.Scenario.Id);

            var result = await _storyContinuityService.EvaluateAsync(request.Scenario, cancellationToken);

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
