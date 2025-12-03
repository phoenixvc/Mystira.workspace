using Microsoft.AspNetCore.Mvc;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using Mystira.StoryGenerator.Api.Services.ContinuityAsync;

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
    private readonly IContinuityOperationStore _store;
    private readonly IContinuityBackgroundQueue _queue;

    public StoryContinuityController(
        IStoryContinuityService storyContinuityService,
        IScenarioFactory scenarioFactory,
        ILogger<StoryContinuityController> logger,
        IContinuityOperationStore store,
        IContinuityBackgroundQueue queue)
    {
        _storyContinuityService = storyContinuityService ?? throw new ArgumentNullException(nameof(storyContinuityService));
        _scenarioFactory = scenarioFactory ?? throw new ArgumentNullException(nameof(scenarioFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
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

            var issues = await _storyContinuityService.AnalyzeAsync(scenario, cancellationToken);

            // Map analysis issues (EntityContinuityIssue) to API contract issues (StoryContinuityIssue)
             var mappedIssues = (issues ?? Array.Empty<EntityContinuityIssue>())
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
                Issues = mappedIssues,
                Error = null
            };

            return Ok(response);
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

    /// <summary>
    /// Starts an asynchronous continuity evaluation and returns an operation id immediately.
    /// </summary>
    [HttpPost("evaluate-async")]
    public async Task<ActionResult<object>> EvaluateAsyncStart(
        [FromBody] EvaluateStoryContinuityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        // Create operation and queue a job
        var op = _store.CreateNew();
        await _queue.QueueAsync(new ContinuityJob
        {
            OperationId = op.OperationId,
            Request = request
        }, cancellationToken);

        // Return 202 Accepted with Location header pointing to the operation status endpoint
        return AcceptedAtAction(
            nameof(GetOperationStatus),
            new { id = op.OperationId },
            new { operationId = op.OperationId, status = op.Status }
        );
    }

    /// <summary>
    /// Gets the status (and result if available) of a previously started continuity evaluation.
    /// </summary>
    [HttpGet("operations/{id}")]
    public ActionResult<ContinuityOperationInfo> GetOperationStatus([FromRoute] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest();

        if (_store.TryGet(id, out var info) && info != null)
        {
            return Ok(info);
        }

        return NotFound();
    }
}
