using System.Text.Json;
using Azure.AI.Projects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Orchestrates the stateful story generation loop, coordinating writer-agent → validation → judge-agent → refiner-agent flows.
/// </summary>
public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly ILogger<AgentOrchestrator> _logger;
    private readonly IAgentStreamPublisher _eventPublisher;
    private readonly IStorySessionRepository _sessionRepository;
    private readonly FoundryAgentClient _foundryClient;
    private readonly IKnowledgeProvider _knowledgeProvider;
    private readonly FoundryAgentConfig _config;

    public AgentOrchestrator(
        ILogger<AgentOrchestrator> logger,
        IAgentStreamPublisher eventPublisher,
        IStorySessionRepository sessionRepository,
        FoundryAgentClient foundryClient,
        IKnowledgeProvider knowledgeProvider,
        IOptions<FoundryAgentConfig> config)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _sessionRepository = sessionRepository;
        _foundryClient = foundryClient;
        _knowledgeProvider = knowledgeProvider;
        _config = config.Value;
    }

    public async Task<StorySession> InitializeSessionAsync(string sessionId, string knowledgeMode, string ageGroup)
    {
        _logger.LogInformation("Initializing session {SessionId} with knowledge mode {KnowledgeMode} for age group {AgeGroup}", 
            sessionId, knowledgeMode, ageGroup);

        // Create new StorySession
        var session = new StorySession
        {
            SessionId = sessionId,
            KnowledgeMode = Enum.Parse<KnowledgeMode>(knowledgeMode, true),
            Stage = StorySessionStage.Uninitialized,
            IterationCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            // Create Foundry thread with metadata
            var threadMetadata = new Dictionary<string, string>
            {
                { "session_id", sessionId },
                { "age_group", ageGroup },
                { "knowledge_mode", knowledgeMode }
            };

            // Attach knowledge provider to thread based on mode
            if (session.KnowledgeMode == KnowledgeMode.AISearch)
            {
                var searchContext = await _knowledgeProvider.SearchAsync("", new List<string> { ageGroup });
                // Store search context for later use
                session.CurrentStoryVersion = JsonSerializer.Serialize(searchContext);
            }
            else // FileSearch mode
            {
                // For FileSearch mode, we need to set up the vector store
                // This would be configured in the thread creation
            }

            // Create the thread via Foundry
            var threadResult = await _foundryClient.CreateThreadAsync(_config.WriterAgentId);

            session.ThreadId = threadResult.ThreadId;
            session.Stage = StorySessionStage.Uninitialized;
            session.UpdatedAt = DateTime.UtcNow;

            // Persist session
            await _sessionRepository.UpdateAsync(session);

            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Initializing",
                Payload = new { KnowledgeMode = knowledgeMode, AgeGroup = ageGroup },
                IterationNumber = 0
            });

            _logger.LogInformation("Session {SessionId} initialized successfully with thread ID {ThreadId}", 
                sessionId, session.ThreadId);

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize session {SessionId}", sessionId);
            
            session.Stage = StorySessionStage.Failed;
            await _sessionRepository.UpdateAsync(session);

            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.Error,
                Phase = "Initializing",
                Payload = new { Error = ex.Message }
            });

            throw;
        }
    }

    public async Task<(bool Success, string Message)> GenerateStoryAsync(string sessionId, string storyPrompt, CancellationToken ct)
    {
        _logger.LogInformation("Starting story generation for session {SessionId}", sessionId);

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
            var writerPrompt = BuildWriterPrompt(storyPrompt, session);

            // Create and start the run
            var runResult = await _foundryClient.CreateRunAsync(
                session.ThreadId!, 
                _config.WriterAgentId, 
                writerPrompt, 
                ct);

            // Wait for completion
            var completionResult = await _foundryClient.WaitForRunCompletionAsync(
                session.ThreadId!, 
                runResult.RunId, 
                pollInterval: TimeSpan.FromSeconds(2),
                maxWait: _config.RunTimeout,
                cancellationToken: ct);

            if (!completionResult.Completed)
            {
                throw new InvalidOperationException($"Writer agent run failed: {completionResult.ErrorMessage}");
            }

            // Extract assistant response
            var assistantMessages = completionResult.Messages.Where(m => m.Role == "assistant").ToList();
            if (!assistantMessages.Any())
            {
                throw new InvalidOperationException("No assistant response received from writer agent");
            }

            var storyJson = assistantMessages.Last().Content;

            // Validate JSON schema
            if (!await ValidateStorySchemaAsync(storyJson))
            {
                throw new InvalidOperationException("Generated story failed schema validation");
            }

            // Store as current version and add to history
            var versionSnapshot = new StoryVersionSnapshot
            {
                VersionNumber = session.StoryVersions.Count + 1,
                StoryJson = storyJson,
                CreatedAt = DateTime.UtcNow,
                StageWhenCreated = "Generating",
                IterationNumber = session.IterationCount
            };

            session.CurrentStoryVersion = storyJson;
            session.StoryVersions.Add(versionSnapshot);
            session.Stage = StorySessionStage.Validating;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, ct);

            // Emit generation complete event
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.GenerationComplete,
                Phase = "Writing",
                Payload = new { StoryJson = storyJson, TokenUsage = completionResult.RunId },
                IterationNumber = session.IterationCount
            });

            _logger.LogInformation("Story generation completed for session {SessionId}", sessionId);
            return (true, "Story generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Story generation failed for session {SessionId}", sessionId);

            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session != null)
            {
                session.Stage = StorySessionStage.Failed;
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

    public async Task<(bool Success, EvaluationReport Report)> EvaluateStoryAsync(string sessionId, CancellationToken ct)
    {
        _logger.LogInformation("Starting story evaluation for session {SessionId}", sessionId);

        try
        {
            // Load and validate session state
            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session == null)
            {
                throw new InvalidOperationException("Session not found");
            }

            if (session.Stage != StorySessionStage.Validating)
            {
                throw new InvalidOperationException($"Invalid session state for evaluation: {session.Stage}");
            }

            session.Stage = StorySessionStage.Evaluating;
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, ct);

            // Phase A: Deterministic validation gates
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Validating",
                Payload = new { Phase = "Schema and Safety Checks" }
            });

            var schemaValid = await ValidateStorySchemaAsync(session.CurrentStoryVersion);
            var safetyGatePassed = await RunSafetyGateAsync(session.CurrentStoryVersion);

            if (!schemaValid || !safetyGatePassed)
            {
                var findings = new List<string>();
                if (!schemaValid) findings.Add("Story failed schema validation");
                if (!safetyGatePassed) findings.Add("Story failed safety gate checks");

                session.Stage = StorySessionStage.RefinementRequested;
                session.UpdatedAt = DateTime.UtcNow;
                await _sessionRepository.UpdateAsync(session, ct);

                var report = new EvaluationReport
                {
                    IterationNumber = session.IterationCount,
                    OverallStatus = EvaluationStatus.Fail,
                    SafetyGatePassed = safetyGatePassed,
                    Findings = new Dictionary<string, List<string>> { { "validation", findings } }
                };

                await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
                {
                    Type = AgentStreamEvent.EventType.ValidationFailed,
                    Phase = "Validating",
                    Payload = report
                });

                return (false, report);
            }

            // Phase B: Local logic injection (SRL + path compression)
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "AnalyzingLogic",
                Payload = new { Phase = "Story Structure Analysis" }
            });

            var storyStructure = await AnalyzeStoryStructureAsync(session.CurrentStoryVersion);
            var logicContext = ComputeNarrativeConsistencyContext(storyStructure);

            // Phase C: LLM evaluation (Judge Agent)
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Evaluating",
                Payload = new { Phase = "Judge Agent Evaluation" }
            });

            var judgePrompt = BuildJudgePrompt(session, logicContext);

            // Create and start the run
            var runResult = await _foundryClient.CreateRunAsync(
                session.ThreadId!, 
                _config.JudgeAgentId, 
                judgePrompt, 
                ct);

            // Wait for completion
            var completionResult = await _foundryClient.WaitForRunCompletionAsync(
                session.ThreadId!, 
                runResult.RunId, 
                pollInterval: TimeSpan.FromSeconds(2),
                maxWait: _config.RunTimeout,
                cancellationToken: ct);

            if (!completionResult.Completed)
            {
                throw new InvalidOperationException($"Judge agent run failed: {completionResult.ErrorMessage}");
            }

            // Parse judge response to EvaluationReport
            var judgeResponse = completionResult.Messages.Last(m => m.Role == "assistant").Content;
            var evaluationReport = await ParseEvaluationReportAsync(judgeResponse, session.IterationCount);

            // Store evaluation report
            session.LastEvaluationReport = evaluationReport;

            // Gate decision
            if (evaluationReport.OverallStatus == EvaluationStatus.Pass)
            {
                session.Stage = StorySessionStage.Evaluated;
                
                await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
                {
                    Type = AgentStreamEvent.EventType.EvaluationPassed,
                    Phase = "Evaluating",
                    Payload = evaluationReport
                });
            }
            else
            {
                session.Stage = StorySessionStage.RefinementRequested;
                
                await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
                {
                    Type = AgentStreamEvent.EventType.EvaluationFailed,
                    Phase = "Evaluating",
                    Payload = evaluationReport
                });
            }

            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session, ct);

            _logger.LogInformation("Story evaluation completed for session {SessionId} with status {Status}", 
                sessionId, evaluationReport.OverallStatus);

            return (true, evaluationReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Story evaluation failed for session {SessionId}", sessionId);

            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session != null)
            {
                session.Stage = StorySessionStage.Failed;
                session.UpdatedAt = DateTime.UtcNow;
                await _sessionRepository.UpdateAsync(session, ct);
            }

            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.Error,
                Phase = "Evaluating",
                Payload = new { Error = ex.Message }
            });

            throw;
        }
    }

    public async Task<(bool Success, string Message)> RefineStoryAsync(string sessionId, UserRefinementFocus focus, CancellationToken ct)
    {
        _logger.LogInformation("Starting story refinement for session {SessionId}", sessionId);

        try
        {
            // Load and validate session state
            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session == null)
            {
                return (false, "Session not found");
            }

            if (session.Stage != StorySessionStage.RefinementRequested)
            {
                return (false, $"Invalid session state for refinement: {session.Stage}");
            }

            // Store user focus
            session.UserFocus = focus;
            session.IterationCount++;
            session.Stage = StorySessionStage.Refined;
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
            var refinerPrompt = BuildRefinerPrompt(session, focus);

            // Create and start the run
            var runResult = await _foundryClient.CreateRunAsync(
                session.ThreadId!, 
                _config.RefinerAgentId, 
                refinerPrompt, 
                ct);

            // Wait for completion
            var completionResult = await _foundryClient.WaitForRunCompletionAsync(
                session.ThreadId!, 
                runResult.RunId, 
                pollInterval: TimeSpan.FromSeconds(2),
                maxWait: _config.RunTimeout,
                cancellationToken: ct);

            if (!completionResult.Completed)
            {
                throw new InvalidOperationException($"Refiner agent run failed: {completionResult.ErrorMessage}");
            }

            // Parse response and validate schema
            var refinedStoryJson = completionResult.Messages.Last(m => m.Role == "assistant").Content;
            
            if (!await ValidateStorySchemaAsync(refinedStoryJson))
            {
                throw new InvalidOperationException("Refined story failed schema validation");
            }

            // Store new version
            var versionSnapshot = new StoryVersionSnapshot
            {
                VersionNumber = session.StoryVersions.Count + 1,
                StoryJson = refinedStoryJson,
                CreatedAt = DateTime.UtcNow,
                StageWhenCreated = "Refining",
                IterationNumber = session.IterationCount
            };

            session.CurrentStoryVersion = refinedStoryJson;
            session.StoryVersions.Add(versionSnapshot);
            session.Stage = StorySessionStage.Validating; // Re-evaluate after refinement
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

            await _sessionRepository.UpdateAsync(session, ct);

            // Emit refinement complete event
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.RefinementComplete,
                Phase = "Refining",
                Payload = new { RefinedStoryJson = refinedStoryJson, TokenUsage = completionResult.RunId },
                IterationNumber = session.IterationCount
            });

            _logger.LogInformation("Story refinement completed for session {SessionId}", sessionId);
            return (true, "Story refined successfully; re-evaluation required");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Story refinement failed for session {SessionId}", sessionId);

            var session = await _sessionRepository.GetAsync(sessionId, ct);
            if (session != null)
            {
                session.Stage = StorySessionStage.Failed;
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

    public async Task<StorySession?> GetSessionAsync(string sessionId)
    {
        return await _sessionRepository.GetAsync(sessionId);
    }

    #region Private Helper Methods

    private string BuildWriterPrompt(string storyPrompt, StorySession session)
    {
        var prompt = $@"You are a creative story writer for children's interactive stories. Generate a complete story that follows the provided schema.

Story Prompt: {storyPrompt}

Requirements:
1. Create a story suitable for the target age group
2. Follow the JSON schema exactly - all required fields must be present
3. Include engaging characters and scenes
4. Ensure age-appropriate content and difficulty
5. Make the story interactive with meaningful choices

Please generate a complete story in valid JSON format that matches the schema.";

        return prompt;
    }

    private async Task<bool> ValidateStorySchemaAsync(string storyJson)
    {
        try
        {
            // For now, basic JSON validation - in production this would use JSON Schema validation
            var doc = JsonDocument.Parse(storyJson);
            return doc.RootElement.EnumerateObject().Any();
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async Task<bool> RunSafetyGateAsync(string storyJson)
    {
        // TODO: Implement local safety gate checks
        // This would check for inappropriate content, age-appropriate language, etc.
        return true;
    }

    private async Task<object> AnalyzeStoryStructureAsync(string storyJson)
    {
        // TODO: Implement SRL (Semantic Role Labelling) and story structure analysis
        // This would extract scenes, characters, state transitions
        return new { Scenes = new List<object>(), Characters = new List<object>() };
    }

    private string ComputeNarrativeConsistencyContext(object storyStructure)
    {
        // TODO: Implement frontier-merged paths and narrative consistency computation
        return "Narrative consistency context";
    }

    private string BuildJudgePrompt(StorySession session, string logicContext)
    {
        var prompt = $@"You are an expert judge evaluating children's interactive stories for quality, consistency, and age-appropriateness.

Story to evaluate:
{session.CurrentStoryVersion}

Logic context:
{logicContext}

Please provide a comprehensive evaluation in the following JSON format:
{{
  ""iteration_number"": {session.IterationCount},
  ""overall_status"": ""Pass|Fail|ReviewRequired"",
  ""safety_gate_passed"": true,
  ""axes_alignment_score"": 0.0-1.0,
  ""dev_principles_score"": 0.0-1.0,
  ""narrative_logic_score"": 0.0-1.0,
  ""findings"": {{
    ""category1"": [""finding1"", ""finding2""],
    ""category2"": [""finding3""]
  }},
  ""recommendation"": ""Overall recommendation for improvement"",
  ""token_usage"": 0
}}

Evaluate based on:
1. Schema compliance and JSON validity
2. Age-appropriateness and safety
3. Character consistency and development
4. Narrative logic and flow
5. Educational value and engagement
6. Interactive elements quality";

        return prompt;
    }

    private async Task<EvaluationReport> ParseEvaluationReportAsync(string judgeResponse, int iterationNumber)
    {
        try
        {
            var doc = JsonDocument.Parse(judgeResponse);
            var root = doc.RootElement;

            return new EvaluationReport
            {
                IterationNumber = iterationNumber,
                OverallStatus = Enum.Parse<EvaluationStatus>(root.GetProperty("overall_status").GetString() ?? "Fail"),
                SafetyGatePassed = root.GetProperty("safety_gate_passed").GetBoolean(),
                AxesAlignmentScore = root.GetProperty("axes_alignment_score").GetSingle(),
                DevPrinciplesScore = root.GetProperty("dev_principles_score").GetSingle(),
                NarrativeLogicScore = root.GetProperty("narrative_logic_score").GetSingle(),
                Findings = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(root.GetProperty("findings").GetRawText()) ?? new(),
                Recommendation = root.GetProperty("recommendation").GetString() ?? "",
                TokenUsage = root.GetProperty("token_usage").GetInt32()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse evaluation report: {Response}", judgeResponse);
            
            return new EvaluationReport
            {
                IterationNumber = iterationNumber,
                OverallStatus = EvaluationStatus.Fail,
                SafetyGatePassed = false,
                AxesAlignmentScore = 0,
                DevPrinciplesScore = 0,
                NarrativeLogicScore = 0,
                Findings = new Dictionary<string, List<string>> { { "parsing_error", new List<string> { ex.Message } } },
                Recommendation = "Failed to parse evaluation report",
                TokenUsage = 0
            };
        }
    }

    private string BuildRefinerPrompt(StorySession session, UserRefinementFocus focus)
    {
        var prompt = $@"You are an expert story refiner. Refine the existing story based on the evaluation feedback and user focus areas.

Current Story:
{session.CurrentStoryVersion}

Evaluation Feedback:
{JsonSerializer.Serialize(session.LastEvaluationReport, new JsonSerializerOptions { WriteIndented = true })}

User Focus Areas:
- Target Scenes: {string.Join(", ", focus.TargetSceneIds)}
- Aspects to Focus On: {string.Join(", ", focus.Aspects)}
- Constraints: {focus.Constraints}
- Full Rewrite: {focus.IsFullRewrite}

Refinement Instructions:
";

        if (focus.IsFullRewrite)
        {
            prompt += "1. Rewrite the entire story incorporating all feedback\n";
            prompt += "2. Ensure all required schema fields are present and valid\n";
            prompt += "3. Address all issues identified in the evaluation\n";
        }
        else
        {
            prompt += $"1. ONLY edit scenes: {string.Join(", ", focus.TargetSceneIds)}\n";
            prompt += $"2. ONLY modify aspects: {string.Join(", ", focus.Aspects)}\n";
            prompt += "3. Preserve all other scenes and aspects without change\n";
            prompt += "4. Ensure schema validity is maintained\n";
        }

        if (!string.IsNullOrEmpty(focus.Constraints))
        {
            prompt += $"\nAdditional constraints:\n{focus.Constraints}\n";
        }

        prompt += "\nPlease provide the refined story as valid JSON matching the original schema.";

        return prompt;
    }

    #endregion
}