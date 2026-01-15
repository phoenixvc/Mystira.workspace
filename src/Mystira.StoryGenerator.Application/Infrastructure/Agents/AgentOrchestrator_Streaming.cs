using System.Text;
using System.Text.Json;
using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Models;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Partial class containing streaming methods for real-time agent updates.
/// </summary>
public partial class AgentOrchestrator
{
    /// <summary>
    /// Generates a story with streaming updates from OpenAI.
    /// </summary>
    public async Task<(bool Success, string Message)> GenerateStoryStreamingAsync(
        string sessionId,
        string storyPrompt,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting streaming story generation for session {SessionId}", sessionId);

        try
        {
            // Load and validate session state
            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session == null)
            {
                return (false, "Session not found");
            }

            if (session.Stage != StorySessionStage.Uninitialized && session.Stage != StorySessionStage.RefinementRequested)
            {
                return (false, $"Invalid session state for generation: {session.Stage}");
            }

            session.IterationCount++;
            session.Stage = StorySessionStage.Generating;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, ct);

            // Emit phase started event
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Writing",
                Payload = new { StoryPrompt = storyPrompt },
                IterationNumber = session.IterationCount
            });

            // Build writer-agent prompt
            var writerPrompt = _promptGenerator.GenerateWriterPrompt(
                storyPrompt,
                session.AgeGroup,
                session.TargetAxes);

            _logger.LogInformation(
                "Generated Writer agent prompt with contextual knowledge for age group {AgeGroup} (knowledge mode: {KnowledgeMode})",
                session.AgeGroup, session.KnowledgeMode);

            // Stream the run and collect the response
            var responseBuilder = new StringBuilder();
            var runId = string.Empty;

            await foreach (var update in _foundryClient.StreamRunAsync(
                session.ThreadId!,
                _config.WriterAgentId,
                writerPrompt,
                ct))
            {
                var (updateType, content, currentRunId) = ProcessStreamingUpdate(update);

                if (!string.IsNullOrEmpty(currentRunId))
                {
                    runId = currentRunId;
                }

                if (!string.IsNullOrEmpty(content))
                {
                    responseBuilder.Append(content);

                    // Publish streaming update event
                    await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
                    {
                        Type = AgentStreamEvent.EventType.StreamingUpdate,
                        Phase = "Writing",
                        Message = content,
                        Payload = new { UpdateType = updateType, RunId = runId },
                        IterationNumber = session.IterationCount
                    });
                }
            }

            var rawStoryResponse = responseBuilder.ToString();

            if (string.IsNullOrWhiteSpace(rawStoryResponse))
            {
                throw new InvalidOperationException("No response received from writer agent");
            }

            // Parse and validate the response
            var (storyParseOk, storyDoc, storyParseError) = AgentResponseParser.TryParseJsonResponse<JsonDocument>(rawStoryResponse);
            if (!storyParseOk || storyDoc == null)
            {
                throw new InvalidOperationException($"Writer agent returned invalid JSON: {storyParseError}");
            }

            string storyJson;
            using (storyDoc)
            {
                storyJson = storyDoc.RootElement.GetRawText();
            }

            // Validate JSON schema
            var (isValid, errors) = await _schemaValidator.ValidateAsync(storyJson, ct);
            if (!isValid)
            {
                var errorSummary = string.Join(" | ", errors.Take(10));
                throw new InvalidOperationException($"Generated story failed schema validation: {errorSummary}");
            }

            // Store as current version and add to history
            var versionSnapshot = new StoryVersionSnapshot
            {
                VersionNumber = session.StoryVersions.Count + 1,
                StoryJson = storyJson,
                StoryYaml = JsonToYamlConverter.ToYaml(storyJson),
                CreatedAt = DateTime.UtcNow,
                StageWhenCreated = "Generating",
                IterationNumber = session.IterationCount
            };

            session.CurrentStoryVersion = storyJson;
            session.CurrentStoryYaml = JsonToYamlConverter.ToYaml(storyJson);
            session.StoryVersions.Add(versionSnapshot);
            session.Stage = StorySessionStage.Validating;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, ct);

            // Emit generation complete event
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.GenerationComplete,
                Phase = "Writing",
                Payload = new { StoryJson = storyJson, RunId = runId },
                IterationNumber = session.IterationCount
            });

            _logger.LogInformation("Streaming story generation completed for session {SessionId}", sessionId);
            return (true, "Story generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming story generation failed for session {SessionId}", sessionId);

            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session != null)
            {
                session.Stage = StorySessionStage.Failed;
                session.ErrorMessage = $"Story generation failed: {ex.Message}";
                session.UpdatedAt = DateTime.UtcNow;
                await _sessionRepository.UpdateAsync(session, ct);
            }

            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.Error,
                Phase = "Writing",
                Payload = new { Error = ex.Message }
            });

            return (false, $"Generation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Refines a story with streaming updates from OpenAI.
    /// </summary>
    public async Task<(bool Success, string Message)> RefineStoryStreamingAsync(
        string sessionId,
        UserRefinementFocus focus,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting streaming story refinement for session {SessionId}", sessionId);

        try
        {
            // Load and validate session state
            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session == null)
            {
                return (false, "Session not found");
            }

            // Accept Refining, RefinementRequested, or Evaluated stages
            if (session.Stage != StorySessionStage.Refining &&
                session.Stage != StorySessionStage.RefinementRequested &&
                session.Stage != StorySessionStage.Evaluated)
            {
                return (false, $"Invalid session state for refinement: {session.Stage}");
            }

            // Store user focus
            session.UserFocus = focus;
            session.IterationCount++;
            session.Stage = StorySessionStage.Refining;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, ct);

            // Emit phase started event
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Refining",
                Payload = new { UserFocus = focus, IsFullRewrite = focus.IsFullRewrite },
                IterationNumber = session.IterationCount
            });

            // Build refiner-agent prompt
            if (session.LastEvaluationReport == null)
            {
                return (false, "Cannot refine without an evaluation report");
            }

            var refinerPrompt = _promptGenerator.GenerateRefinerPrompt(
                session.CurrentStoryVersion,
                session.LastEvaluationReport,
                focus);

            _logger.LogInformation(
                "Generated Refiner agent prompt with contextual knowledge for age group {AgeGroup} (knowledge mode: {KnowledgeMode})",
                session.AgeGroup, session.KnowledgeMode);

            // Stream the run and collect the response
            var responseBuilder = new StringBuilder();
            var runId = string.Empty;

            await foreach (var update in _foundryClient.StreamRunAsync(
                session.ThreadId!,
                _config.RefinerAgentId,
                refinerPrompt,
                ct))
            {
                var (updateType, content, currentRunId) = ProcessStreamingUpdate(update);

                if (!string.IsNullOrEmpty(currentRunId))
                {
                    runId = currentRunId;
                }

                if (!string.IsNullOrEmpty(content))
                {
                    responseBuilder.Append(content);

                    // Publish streaming update event
                    await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
                    {
                        Type = AgentStreamEvent.EventType.StreamingUpdate,
                        Phase = "Refining",
                        Message = content,
                        Payload = new { UpdateType = updateType, RunId = runId },
                        IterationNumber = session.IterationCount
                    });
                }
            }

            var rawRefinedResponse = responseBuilder.ToString();

            if (string.IsNullOrWhiteSpace(rawRefinedResponse))
            {
                throw new InvalidOperationException("No response received from refiner agent");
            }

            // Parse response and validate schema
            var (refinedParseOk, refinedDoc, refinedParseError) = AgentResponseParser.TryParseJsonResponse<JsonDocument>(rawRefinedResponse);
            if (!refinedParseOk || refinedDoc == null)
            {
                throw new InvalidOperationException($"Refiner agent returned invalid JSON: {refinedParseError}");
            }

            string refinedStoryJson;
            using (refinedDoc)
            {
                refinedStoryJson = refinedDoc.RootElement.GetRawText();
            }

            var (isValid, errors) = await _schemaValidator.ValidateAsync(refinedStoryJson, ct);
            if (!isValid)
            {
                var errorSummary = string.Join(" | ", errors.Take(10));
                throw new InvalidOperationException($"Refined story failed schema validation: {errorSummary}");
            }

            // Store new version
            var versionSnapshot = new StoryVersionSnapshot
            {
                VersionNumber = session.StoryVersions.Count + 1,
                StoryJson = refinedStoryJson,
                StoryYaml = JsonToYamlConverter.ToYaml(refinedStoryJson),
                CreatedAt = DateTime.UtcNow,
                StageWhenCreated = "Refining",
                IterationNumber = session.IterationCount
            };

            session.CurrentStoryVersion = refinedStoryJson;
            session.CurrentStoryYaml = JsonToYamlConverter.ToYaml(refinedStoryJson);
            session.StoryVersions.Add(versionSnapshot);
            session.UpdatedAt = DateTime.UtcNow;

            // Check max iterations
            if (session.IterationCount >= _config.MaxIterations)
            {
                session.Stage = StorySessionStage.StuckNeedsReview;
                await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
                {
                    Type = AgentStreamEvent.EventType.MaxIterationsReached,
                    Phase = "Refining",
                    Payload = new { MaxIterations = _config.MaxIterations }
                });

                await _sessionRepository.UpdateAsync(session, ct);
                return (false, $"Maximum iterations ({_config.MaxIterations}) reached");
            }

            // Emit refinement complete event
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.RefinementComplete,
                Phase = "Refining",
                Payload = new { RefinedStoryJson = refinedStoryJson, RunId = runId },
                IterationNumber = session.IterationCount
            });

            // Transition to Validating stage and publish event
            session.Stage = StorySessionStage.Validating;
            await _sessionRepository.UpdateAsync(session, ct);

            // Emit phase started event for validating
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Validating",
                Payload = new { IterationNumber = session.IterationCount },
                IterationNumber = session.IterationCount
            });

            _logger.LogInformation("Streaming story refinement completed for session {SessionId}", sessionId);
            return (true, "Story refined successfully; re-evaluation required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming story refinement failed for session {SessionId}", sessionId);

            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session != null)
            {
                session.Stage = StorySessionStage.Failed;
                session.ErrorMessage = $"Story refinement failed: {ex.Message}";
                session.UpdatedAt = DateTime.UtcNow;
                await _sessionRepository.UpdateAsync(session, ct);
            }

            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.Error,
                Phase = "Refining",
                Payload = new { Error = ex.Message }
            });

            return (false, $"Refinement failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a rubric summary with streaming updates from OpenAI.
    /// </summary>
    public async Task<(bool Success, RubricSummary? Rubric)> GenerateRubricStreamingAsync(
        string sessionId,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting streaming rubric generation for session {SessionId}", sessionId);

        try
        {
            // Load and validate session state
            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session == null)
            {
                return (false, null);
            }

            if (session.LastEvaluationReport == null)
            {
                _logger.LogWarning("Cannot generate rubric without an evaluation report for session {SessionId}", sessionId);
                return (false, null);
            }

            session.Stage = StorySessionStage.GeneratingRubric;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, ct);

            // Emit phase started event
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Rubric",
                Payload = new { IterationNumber = session.IterationCount }
            });

            // Build rubric prompt
            var rubricPrompt = _promptGenerator.GenerateRubricPrompt(
                session.CurrentStoryVersion,
                session.LastEvaluationReport,
                session.IterationCount);

            // Stream the run and collect the response
            var responseBuilder = new StringBuilder();
            var runId = string.Empty;

            await foreach (var update in _foundryClient.StreamRunAsync(
                session.ThreadId!,
                _config.WriterAgentId, // Reuse writer agent for rubric generation
                rubricPrompt,
                ct))
            {
                var (updateType, content, currentRunId) = ProcessStreamingUpdate(update);

                if (!string.IsNullOrEmpty(currentRunId))
                {
                    runId = currentRunId;
                }

                if (!string.IsNullOrEmpty(content))
                {
                    responseBuilder.Append(content);

                    // Publish streaming update event
                    await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
                    {
                        Type = AgentStreamEvent.EventType.StreamingUpdate,
                        Phase = "Rubric",
                        Message = content,
                        Payload = new { UpdateType = updateType, RunId = runId },
                        IterationNumber = session.IterationCount
                    });
                }
            }

            var rubricResponse = responseBuilder.ToString();

            if (string.IsNullOrWhiteSpace(rubricResponse))
            {
                throw new InvalidOperationException("No response received from rubric generation");
            }

            // Parse rubric response
            var (success, rubric, error) = AgentResponseParser.TryParseJsonResponse<RubricSummary>(rubricResponse);

            if (!success || rubric == null)
            {
                _logger.LogWarning("Failed to parse rubric JSON: {Error}. Raw response: {Response}", error, rubricResponse);
                return (false, null);
            }

            // Store rubric in session
            session.RubricSummary = rubric;
            session.Stage = StorySessionStage.Complete;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, ct);

            // Emit rubric generation complete event
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.RubricGenerated,
                Phase = "Rubric",
                Payload = new { Rubric = rubric, RunId = runId }
            });

            _logger.LogInformation("Streaming rubric generation completed for session {SessionId}", sessionId);
            return (true, rubric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming rubric generation failed for session {SessionId}", sessionId);

            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session != null)
            {
                session.Stage = StorySessionStage.Failed;
                session.ErrorMessage = $"Rubric generation failed: {ex.Message}";
                session.UpdatedAt = DateTime.UtcNow;
                await _sessionRepository.UpdateAsync(session, ct);
            }

            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.Error,
                Phase = "Rubric",
                Payload = new { Error = ex.Message }
            });

            return (false, null);
        }
    }

    /// <summary>
    /// Processes a streaming update from Azure AI Foundry and extracts content.
    /// </summary>
    /// <returns>Tuple of (update type, content, run ID)</returns>
    private (string UpdateType, string Content, string RunId) ProcessStreamingUpdate(StreamingUpdate update)
    {
        try
        {
            // Handle different streaming update types
            if (update is MessageDeltaUpdate messageDelta)
            {
                var content = new StringBuilder();
                foreach (var contentItem in messageDelta.Delta.ContentItems)
                {
                    if (contentItem is MessageDeltaTextContent textContent)
                    {
                        content.Append(textContent.Text);
                    }
                }
                return ("MessageDelta", content.ToString(), messageDelta.RunId ?? string.Empty);
            }

            if (update is RunUpdate runUpdate)
            {
                return ("RunUpdate", string.Empty, runUpdate.Value.Id);
            }

            if (update is MessageUpdate messageUpdate)
            {
                return ("MessageUpdate", string.Empty, string.Empty);
            }

            if (update is RunStepUpdate runStepUpdate)
            {
                return ("RunStepUpdate", string.Empty, runStepUpdate.Value.RunId);
            }

            return ("Unknown", string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to process streaming update: {UpdateType}", update.GetType().Name);
            return ("Error", string.Empty, string.Empty);
        }
    }
}
