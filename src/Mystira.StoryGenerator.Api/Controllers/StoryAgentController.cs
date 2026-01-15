using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Mystira.StoryGenerator.Api.Models;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Domain.Agents;
using System.Text.Json;
using System.Collections.Concurrent;
using Mystira.StoryGenerator.Contracts.Models;
using SessionStateResponse = Mystira.StoryGenerator.Api.Models.SessionStateResponse;

namespace Mystira.StoryGenerator.Api.Controllers;

/// <summary>
/// Controller for managing story generation sessions with Azure AI Foundry agents.
/// Provides REST endpoints and Server-Sent Events for real-time streaming.
/// </summary>
[ApiController]
[Route("api/story-agent")]
[Tags("Story Agent")]
public class StoryAgentController : ControllerBase
{
    private readonly IAgentOrchestrator _agentOrchestrator;
    private readonly IAgentStreamPublisher _streamPublisher;
    private readonly IStorySessionRepository _sessionRepository;
    private readonly ILogger<StoryAgentController> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public StoryAgentController(
        IAgentOrchestrator agentOrchestrator,
        IAgentStreamPublisher streamPublisher,
        IStorySessionRepository sessionRepository,
        ILogger<StoryAgentController> logger)
    {
        _agentOrchestrator = agentOrchestrator;
        _streamPublisher = streamPublisher;
        _sessionRepository = sessionRepository;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Start a new story generation session.
    /// </summary>
    /// <param name="request">The start session request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Session start response with session ID.</returns>
    [HttpPost("sessions/start")]
    [ProducesResponseType(typeof(SessionStartResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Starting story session {CorrelationId} with prompt: {Prompt}", correlationId, request.StoryPrompt);

        try
        {
            // Validate request manually
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                _logger.LogWarning("Session start {CorrelationId} validation failed: {Error}", correlationId, errorMessage);
                return BadRequest(new { error = errorMessage, correlationId });
            }

            // Validate knowledge mode enum
            if (!Enum.TryParse<KnowledgeMode>(request.KnowledgeMode, out var knowledgeMode))
            {
                return BadRequest(new { error = "KnowledgeMode must be either 'FileSearch' or 'AISearch'", correlationId });
            }

            // Generate unique session ID
            var sessionId = Guid.NewGuid().ToString();

            // Initialize session
            var session = await _agentOrchestrator.InitializeSessionAsync(sessionId, request.KnowledgeMode, request.AgeGroup);

            session.TargetAxes = request.TargetAxes ?? new List<string>();

            // Save story prompt in the session for later use
            session.UserFocus = new UserRefinementFocus
            {
                Constraints = request.StoryPrompt
            };
            await _sessionRepository.UpsertAsync(session, cancellationToken);

            _logger.LogInformation("Session {SessionId} created with thread {ThreadId}", sessionId, session.ThreadId);

            // Fire-and-forget: Start story generation in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _agentOrchestrator.GenerateStoryAsync(sessionId, request.StoryPrompt, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background story generation for session {SessionId}", sessionId);
                }
            }, cancellationToken);

            var response = new SessionStartResponse
            {
                SessionId = sessionId,
                ThreadId = session.ThreadId ?? string.Empty,
                KnowledgeMode = request.KnowledgeMode,
                Stage = session.Stage.ToString(),
                CreatedAt = session.CreatedAt
            };

            return Accepted(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting session {CorrelationId}", correlationId);
            return StatusCode(500, new { error = "Failed to start session", correlationId });
        }
    }

    /// <summary>
    /// Evaluate the current story in a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="request">The evaluation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Evaluation response with report and recommendations.</returns>
    [HttpPost("sessions/{sessionId}/evaluate")]
    [ProducesResponseType(typeof(EvaluateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> EvaluateStory(string sessionId, [FromBody] EvaluateRequest? request = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating story for session {SessionId}", sessionId);

        try
        {
            // Load session
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return NotFound(new { error = "Session not found", sessionId });
            }

            // Verify session is in correct state
            if (session.Stage != StorySessionStage.Validating)
            {
                return Conflict(new { error = "Session is not in Validating state", sessionId, currentStage = session.Stage.ToString() });
            }

            // Update session to Evaluating stage
            session.Stage = StorySessionStage.Evaluating;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpsertAsync(session, cancellationToken);

            // Publish Evaluating phase event
            await _streamPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Evaluating",
                Payload = new { },
                IterationNumber = session.IterationCount
            });

            _logger.LogInformation("Session {SessionId} entered Evaluating stage", sessionId);

            // Set timeout
            var timeoutSeconds = request?.TimeoutSeconds ?? 300;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            // Evaluate story
            var (success, evaluationReport) = await _agentOrchestrator.EvaluateStoryAsync(sessionId, cts.Token);

            if (!success)
            {
                return StatusCode(500, new { error = "Evaluation failed", sessionId });
            }

            if (cts.IsCancellationRequested)
            {
                return StatusCode(408, new { error = "Evaluation timed out", sessionId });
            }

            // Determine recommended action
            var recommendedAction = evaluationReport.OverallStatus switch
            {
                EvaluationStatus.Pass => "Continue",
                EvaluationStatus.Fail => "Refine",
                EvaluationStatus.ReviewRequired => "ReviewRequired",
                _ => "Continue"
            };

            var response = new EvaluateResponse
            {
                SessionId = sessionId,
                Stage = session.Stage.ToString(),
                EvaluationReport = evaluationReport,
                RecommendedAction = recommendedAction
            };

            _logger.LogInformation("Evaluation complete for session {SessionId}. Status: {Status}, Action: {Action}",
                sessionId, evaluationReport.OverallStatus, recommendedAction);

            return Ok(response);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(499, new { error = "Request was cancelled", sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An unexpected error occurred", sessionId });
        }
    }

    /// <summary>
    /// Refine the story based on user feedback and focus areas.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="request">The refinement request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Refinement response.</returns>
    [HttpPost("sessions/{sessionId}/refine")]
    [ProducesResponseType(typeof(RefineResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> RefineStory(string sessionId, [FromBody] RefineRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refining story for session {SessionId}", sessionId);

        try
        {
            // Load session
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return NotFound(new { error = "Session not found", sessionId });
            }

            // Verify session is in correct state
            if (session.Stage != StorySessionStage.RefinementRequested && session.Stage != StorySessionStage.Evaluated)
            {
                return Conflict(new { error = "Session is not in RefinementRequested or Evaluated state", sessionId, currentStage = session.Stage.ToString() });
            }

            // Check iteration limit (e.g., max 5 iterations)
            if (session.IterationCount >= 5)
            {
                return Conflict(new { error = "Maximum iterations reached", sessionId, iterationCount = session.IterationCount });
            }

            // Validate request
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errorMessage = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                return BadRequest(new { error = errorMessage, sessionId });
            }

            // Create user refinement focus
            var focus = new UserRefinementFocus
            {
                TargetSceneIds = request.TargetSceneIds ?? new List<string>(),
                Aspects = request.Aspects ?? new List<string>(),
                Constraints = request.Constraints ?? string.Empty,
                IsFullRewrite = request.TargetSceneIds.Count == 0
            };

            // Fire-and-forget: Start refinement in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _agentOrchestrator.RefineStoryAsync(sessionId, focus, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background story refinement for session {SessionId}", sessionId);
                }
            }, cancellationToken);

            var response = new RefineResponse
            {
                SessionId = sessionId,
                Stage = session.Stage.ToString(),
                IterationCount = session.IterationCount + 1,
                RefinedStoryPreview = session.CurrentStoryVersion.Length > 500
                    ? session.CurrentStoryVersion[..500] + "..."
                    : session.CurrentStoryVersion
            };

            _logger.LogInformation("Refinement started for session {SessionId} (iteration {Iteration})", sessionId, response.IterationCount);

            return Accepted(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { error = "Request was cancelled", sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refining session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An unexpected error occurred", sessionId });
        }
    }

    /// <summary>
    /// Generate a rubric for the story generation session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Session state response.</returns>
    [HttpPost("sessions/{sessionId}/rubric")]
    [ProducesResponseType(typeof(SessionStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> GenerateRubric(string sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating rubric for session {SessionId}", sessionId);

        try
        {
            // Load session
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return NotFound(new { error = "Session not found", sessionId });
            }

            // Verify session is in correct state
            if (session.Stage != StorySessionStage.RefinementRequested && session.Stage != StorySessionStage.Evaluated)
            {
                return Conflict(new { error = "Session is not in RefinementRequested or Evaluated state", sessionId, currentStage = session.Stage.ToString() });
            }

            // Mark session as generating rubric
            session.Stage = StorySessionStage.GeneratingRubric;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpsertAsync(session, cancellationToken);

            // Publish rubric generation start event
            await _streamPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Rubric",
                Payload = new { },
                IterationNumber = session.IterationCount
            });

            // TODO: Call rubric generator here
            // This should:
            // 1. Call IPromptGenerator.GenerateRubricPrompt(session.CurrentStoryVersion, session.LastEvaluationReport, session.IterationCount)
            // 2. Send the prompt to an LLM agent
            // 3. Parse and store the rubric response in the session
            // 4. Return the rubric to the frontend for display

            // For now, just mark as complete
            session.Stage = StorySessionStage.Complete;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpsertAsync(session, cancellationToken);

            // Publish completion event
            await _streamPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Complete",
                Payload = new { },
                IterationNumber = session.IterationCount
            });

            _logger.LogInformation("Rubric generated for session {SessionId}", sessionId);

            var response = new SessionStateResponse
            {
                SessionId = session.SessionId,
                ThreadId = session.ThreadId ?? string.Empty,
                Stage = session.Stage.ToString(),
                IterationCount = session.IterationCount,
                CostEstimate = Convert.ToDouble(session.CostEstimate),
                CurrentStoryJson = session.CurrentStoryVersion,
                CurrentStoryYaml = session.CurrentStoryYaml ?? string.Empty,
                LastEvaluationReport = session.LastEvaluationReport,
                StoryVersions = session.StoryVersions,
                ErrorMessage = session.ErrorMessage
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating rubric for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An unexpected error occurred", sessionId });
        }
    }

    /// <summary>
    /// Complete the story generation session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Session state response.</returns>
    [HttpPost("sessions/{sessionId}/complete")]
    [ProducesResponseType(typeof(SessionStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> CompleteSession(string sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing session {SessionId}", sessionId);

        try
        {
            // Load session
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return NotFound(new { error = "Session not found", sessionId });
            }

            // Verify session is in correct state
            if (session.Stage != StorySessionStage.RefinementRequested && session.Stage != StorySessionStage.Evaluated)
            {
                return Conflict(new { error = "Session is not in RefinementRequested or Evaluated state", sessionId, currentStage = session.Stage.ToString() });
            }

            // TODO: Call rubric generator here before completing
            // This should:
            // 1. Call IPromptGenerator.GenerateRubricPrompt(session.CurrentStoryVersion, session.LastEvaluationReport, session.IterationCount)
            // 2. Send the prompt to an LLM agent
            // 3. Parse and store the rubric response in the session
            // 4. Return the rubric to the frontend for display

            // Mark session as complete
            session.Stage = StorySessionStage.Complete;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpsertAsync(session, cancellationToken);

            // Publish completion event
            await _streamPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Complete",
                Payload = new { },
                IterationNumber = session.IterationCount
            });

            _logger.LogInformation("Session {SessionId} marked as complete", sessionId);

            var response = new SessionStateResponse
            {
                SessionId = session.SessionId,
                ThreadId = session.ThreadId ?? string.Empty,
                Stage = session.Stage.ToString(),
                IterationCount = session.IterationCount,
                CostEstimate = Convert.ToDouble(session.CostEstimate),
                CurrentStoryJson = session.CurrentStoryVersion,
                CurrentStoryYaml = session.CurrentStoryYaml ?? string.Empty,
                LastEvaluationReport = session.LastEvaluationReport,
                StoryVersions = session.StoryVersions,
                ErrorMessage = session.ErrorMessage
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing session {SessionId}", sessionId);
            return StatusCode(500, new { error = "An unexpected error occurred", sessionId });
        }
    }

    /// <summary>
    /// Get the current state of a story generation session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Session state response.</returns>
    [HttpGet("sessions/{sessionId}")]
    [ProducesResponseType(typeof(SessionStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<IActionResult> GetSessionState(string sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting session state for {SessionId}", sessionId);

        try
        {
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                return NotFound(new { error = "Session not found", sessionId });
            }

            var response = new SessionStateResponse
            {
                SessionId = session.SessionId,
                ThreadId = session.ThreadId ?? string.Empty,
                Stage = session.Stage.ToString(),
                IterationCount = session.IterationCount,
                CostEstimate = Convert.ToDouble(session.CostEstimate),
                CurrentStoryJson = session.CurrentStoryVersion,
                CurrentStoryYaml = session.CurrentStoryYaml ?? string.Empty,
                LastEvaluationReport = session.LastEvaluationReport,
                StoryVersions = session.StoryVersions,
                ErrorMessage = session.ErrorMessage
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session state for {SessionId}", sessionId);
            return StatusCode(500, new { error = "An unexpected error occurred", sessionId });
        }
    }

    /// <summary>
    /// Stream story generation events via Server-Sent Events.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Server-Sent Events stream.</returns>
    [HttpGet("sessions/{sessionId}/stream")]
    public async Task StreamEvents(string sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting SSE stream for session {SessionId}", sessionId);

        try
        {
            // Check if session exists
            var session = await _sessionRepository.GetAsync(sessionId, cancellationToken);
            if (session == null)
            {
                Response.StatusCode = 404;
                await Response.WriteAsync("Session not found");
                return;
            }

            // Check if session is already in terminal state
            var terminalStates = new[] { StorySessionStage.Complete, StorySessionStage.StuckNeedsReview, StorySessionStage.Failed };
            if (terminalStates.Contains(session.Stage))
            {
                Response.StatusCode = 204; // No Content
                return;
            }

            // Set SSE headers
            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("X-Accel-Buffering", "no"); // Disable nginx buffering

            // Start streaming events
            await foreach (var evt in _streamPublisher.SubscribeAsync(sessionId, cancellationToken))
            {
                try
                {
                    var json = JsonSerializer.Serialize(evt, _jsonOptions);
                    var sseData = $"event: {evt.Type}\ndata: {json}\n\n";

                    await Response.WriteAsync(sseData);
                    await Response.Body.FlushAsync();

                    _logger.LogDebug("SSE event streamed for session {SessionId}: {EventType}", sessionId, evt.Type);

                    // Check if this event indicates terminal state
                    if (terminalStates.Contains(session.Stage))
                    {
                        _logger.LogInformation("SSE stream ending due to terminal state for session {SessionId}: {Stage}", sessionId, session.Stage);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error streaming SSE event for session {SessionId}", sessionId);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SSE stream cancelled for session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SSE stream for session {SessionId}", sessionId);
        }
        finally
        {
            _logger.LogInformation("SSE stream ended for session {SessionId}", sessionId);
        }
    }
}
