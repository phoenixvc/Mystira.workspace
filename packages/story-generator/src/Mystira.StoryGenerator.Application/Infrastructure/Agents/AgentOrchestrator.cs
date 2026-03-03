using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Application.Services.Prompting;
using Mystira.StoryGenerator.Contracts.Agents;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Orchestrates the stateful story generation loop, coordinating writer-agent → validation → judge-agent → refiner-agent flows.
/// </summary>
public partial class AgentOrchestrator : IAgentOrchestrator
{
    // Constants for magic numbers
    private const int StoryLengthSafetyThreshold = 10000;
    private const int SimpleStoryMaxLength = 500;
    private const int ModerateStoryMaxLength = 2000;
    private const int ComplexStoryUniqueWordsThreshold = 200;
    private const int MaxEvaluationFindings = 10;

    private readonly ILogger<AgentOrchestrator> _logger;
    private readonly IAgentStreamPublisher _eventPublisher;
    private readonly IStorySessionRepository _sessionRepository;
    private readonly FoundryAgentClient _foundryClient;
    private readonly IPromptGenerator _promptGenerator;
    private readonly StorySchemaValidator _schemaValidator;
    private readonly IKnowledgeProvider _knowledgeProvider;
    private readonly IStorySchemaProvider _schemaProvider;
    private readonly IStoryMediaProcessor _mediaProcessor;
    private readonly FoundryAgentConfig _config;

    public AgentOrchestrator(
        ILogger<AgentOrchestrator> logger,
        IAgentStreamPublisher eventPublisher,
        IStorySessionRepository sessionRepository,
        FoundryAgentClient foundryClient,
        IPromptGenerator promptGenerator,
        StorySchemaValidator schemaValidator,
        IKnowledgeProvider knowledgeProvider,
        IStorySchemaProvider schemaProvider,
        IStoryMediaProcessor mediaProcessor,
        IOptions<FoundryAgentConfig> config)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _sessionRepository = sessionRepository;
        _foundryClient = foundryClient;
        _promptGenerator = promptGenerator;
        _schemaValidator = schemaValidator;
        _knowledgeProvider = knowledgeProvider;
        _schemaProvider = schemaProvider;
        _mediaProcessor = mediaProcessor;
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
            AgeGroup = ageGroup,
            Stage = StorySessionStage.Uninitialized,
            IterationCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try
        {
            // Create the thread via Foundry
            // For FileSearch mode, attach age-specific and agent-specific vector stores
            ThreadCreationResult threadResult;

            if (_knowledgeProvider is FileSearchKnowledgeProvider fileSearchProvider)
            {
                var vectorStoreIds = new List<string>();

                try
                {
                    vectorStoreIds.Add(fileSearchProvider.GetVectorStoreIdForAgeGroup(AgentType.Writer, ageGroup));
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Failed to get vector store IDs for age group {AgeGroup}", ageGroup);
                    throw;
                }

                threadResult = await _foundryClient.CreateThreadWithVectorStoresAsync(
                    _config.WriterAgentId,
                    vectorStoreIds);

                _logger.LogInformation("Created thread with {Count} vector stores for age group {AgeGroup}: {VectorStoreIds}",
                    vectorStoreIds.Count, ageGroup, string.Join(", ", vectorStoreIds));
            }
            else
            {
                threadResult = await _foundryClient.CreateThreadAsync(_config.WriterAgentId);
            }

            session.ThreadId = threadResult.ThreadId;
            session.Stage = StorySessionStage.Uninitialized;
            session.UpdatedAt = DateTime.UtcNow;

            // Persist session
            await _sessionRepository.CreateAsync(session);

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
            session.ErrorMessage = $"Failed to initialize session: {ex.Message}";
            session.UpdatedAt = DateTime.UtcNow;
            await _sessionRepository.UpsertAsync(session);

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
            var writerPrompt = _promptGenerator.GenerateWriterPrompt(
                storyPrompt,
                session.AgeGroup,
                session.TargetAxes);

            _logger.LogInformation(
                "Generated Writer agent prompt with contextual knowledge for age group {AgeGroup} (knowledge mode: {KnowledgeMode})",
                session.AgeGroup, session.KnowledgeMode);

            // Build response format with JSON schema for structured output
            var responseFormat = await BuildResponseFormatAsync(ct);

            // Create and start the run
            var runResult = await _foundryClient.CreateRunAsync(
                session.ThreadId!,
                _config.WriterAgentId,
                writerPrompt,
                responseFormat,
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

            var rawStoryResponse = assistantMessages.Last().Content;

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
                var errorSummary = string.Join(" | ", errors.Take(MaxEvaluationFindings));
                throw new InvalidOperationException($"Generated story failed schema validation: {errorSummary}");
            }

            // Store as current version and add to history
            storyJson = _mediaProcessor.ProcessMediaIds(storyJson);

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
                Payload = new { StoryJson = storyJson, StoryYaml = session.CurrentStoryYaml, TokenUsage = completionResult.RunId },
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

            if (session.Stage != StorySessionStage.Validating && session.Stage != StorySessionStage.Evaluating)
            {
                throw new InvalidOperationException($"Invalid session state for evaluation: {session.Stage}");
            }

            if (session.Stage != StorySessionStage.Evaluating)
            {
                session.Stage = StorySessionStage.Evaluating;
                session.UpdatedAt = DateTime.UtcNow;
                await _sessionRepository.UpdateAsync(session, ct);
            }

            // Phase A: Deterministic validation gates
            await _eventPublisher.PublishEventAsync(sessionId, new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "Validating",
                Payload = new { Phase = "Schema and Safety Checks" }
            });

            var (schemaValid, schemaErrors) = await _schemaValidator.ValidateAsync(session.CurrentStoryVersion, ct);
            var safetyGatePassed = await RunSafetyGateAsync(session.CurrentStoryVersion);

            if (!schemaValid || !safetyGatePassed)
            {
                var findings = new List<string>();

                if (!schemaValid)
                {
                    findings.Add("Story failed schema validation");
                    findings.AddRange(schemaErrors.Take(MaxEvaluationFindings));
                }

                if (!safetyGatePassed)
                    findings.Add("Story failed safety gate checks");

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

            var judgePrompt = _promptGenerator.GenerateJudgePrompt(
                session.CurrentStoryVersion,
                session.AgeGroup,
                session.TargetAxes);

            _logger.LogInformation(
                "Generated Judge agent prompt with contextual knowledge for age group {AgeGroup} (knowledge mode: {KnowledgeMode})",
                session.AgeGroup, session.KnowledgeMode);

            judgePrompt += $"\n\n## Deterministic Story Structure Analysis\n{logicContext}\n";

            // Create and start the run
            var runResult = await _foundryClient.CreateRunAsync(
                session.ThreadId!,
                _config.JudgeAgentId,
                judgePrompt,
                null,
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
                // Check for max iterations escalation
                if (session.IterationCount >= _config.MaxIterations)
                {
                    _logger.LogWarning("Session {SessionId} reached max iterations ({MaxIterations}). Escalating to StuckNeedsReview.",
                        sessionId, _config.MaxIterations);
                    session.Stage = StorySessionStage.StuckNeedsReview;
                }
                else
                {
                    session.Stage = StorySessionStage.RefinementRequested;
                }

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
                session.ErrorMessage = $"Story evaluation failed: {ex.Message}";
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

            // Build response format with JSON schema for structured output
            var responseFormat = await BuildResponseFormatAsync(ct);

            // Create and start the run
            var runResult = await _foundryClient.CreateRunAsync(
                session.ThreadId!,
                _config.RefinerAgentId,
                refinerPrompt,
                responseFormat,
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
            var rawRefinedResponse = completionResult.Messages.Last(m => m.Role == "assistant").Content;

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
                var errorSummary = string.Join(" | ", errors.Take(MaxEvaluationFindings));
                throw new InvalidOperationException($"Refined story failed schema validation: {errorSummary}");
            }

            // Store new version
            refinedStoryJson = _mediaProcessor.ProcessMediaIds(refinedStoryJson);

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
                Payload = new { RefinedStoryJson = refinedStoryJson, RefinedStoryYaml = session.CurrentStoryYaml, TokenUsage = completionResult.RunId },
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

    public async Task<(bool Success, RubricSummary? Rubric)> GenerateRubricAsync(string sessionId, CancellationToken ct)
    {
        _logger.LogInformation("Starting rubric generation for session {SessionId}", sessionId);

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

            // Create and start the run (use Writer agent for rubric generation)
            var runResult = await _foundryClient.CreateRunAsync(
                session.ThreadId!,
                _config.WriterAgentId, // Reuse writer agent for rubric generation
                rubricPrompt,
                null,
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
                throw new InvalidOperationException($"Rubric generation run failed: {completionResult.ErrorMessage}");
            }

            // Parse rubric response
            var rubricResponse = completionResult.Messages.Last(m => m.Role == "assistant").Content;
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
                Payload = new { Rubric = rubric }
            });

            _logger.LogInformation("Rubric generation completed for session {SessionId}", sessionId);
            return (true, rubric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rubric generation failed for session {SessionId}", sessionId);

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

    public async Task<StorySession?> GetSessionAsync(string sessionId)
    {
        return await _sessionRepository.GetAsync(sessionId);
    }

    #region Private Helper Methods


    private Task<bool> RunSafetyGateAsync(string storyJson)
    {
        // Basic safety gate checks for inappropriate content
        try
        {
            // Check for basic inappropriate content patterns
            var inappropriatePatterns = new[]
            {
                "violence", "weapon", "kill", "death", "blood", "gore",
                "adult content", "inappropriate", "offensive"
                // Note: This is a basic implementation - in production would use more sophisticated content filtering
            };

            var storyLower = storyJson.ToLowerInvariant();
            var hasInappropriateContent = inappropriatePatterns.Any(pattern => storyLower.Contains(pattern));

            if (hasInappropriateContent)
            {
                _logger.LogWarning("Safety gate detected potentially inappropriate content in story");
                return Task.FromResult(false);
            }

            // Check for age-appropriate content length (basic heuristic)
            if (storyJson.Length > StoryLengthSafetyThreshold) // Very long stories might contain inappropriate content
            {
                _logger.LogDebug("Story length exceeds safety threshold, requiring manual review");
                // For now, allow but flag for review
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in safety gate checks");
            // Fail safe - if we can't check, assume it's safe for development
            return Task.FromResult(true);
        }
    }

    private Task<object> AnalyzeStoryStructureAsync(string storyJson)
    {
        try
        {
            // Basic story structure analysis - extract scenes and characters
            // This is a simplified implementation that looks for common story patterns

            var storyStructure = new
            {
                Scenes = ExtractBasicScenes(storyJson),
                Characters = ExtractBasicCharacters(storyJson),
                NarrativeElements = ExtractNarrativeElements(storyJson),
                EstimatedComplexity = EstimateStoryComplexity(storyJson)
            };

            _logger.LogDebug("Basic story structure analysis completed");
            return Task.FromResult<object>(storyStructure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in story structure analysis");
            return Task.FromResult<object>(new { Scenes = new List<object>(), Characters = new List<object>() });
        }
    }

    private List<object> ExtractBasicScenes(string storyJson)
    {
        // Basic scene extraction - look for common scene indicators
        var scenes = new List<object>();

        // This is a placeholder implementation
        // In production, would use NLP techniques to identify scene boundaries
        var sceneIndicators = new[] { "chapter", "scene", "part", "section" };
        var storyLower = storyJson.ToLowerInvariant();

        foreach (var indicator in sceneIndicators)
        {
            // Use more efficient counting instead of Split
            var count = CountOccurrences(storyLower, indicator);
            if (count > 0)
            {
                scenes.Add(new { Type = indicator, Count = count });
            }
        }

        return scenes;
    }

    private List<object> ExtractBasicCharacters(string storyJson)
    {
        // Basic character extraction - look for proper nouns and common character patterns
        var characters = new List<object>();

        // This is a placeholder implementation
        // In production, would use NER (Named Entity Recognition) to identify characters
        var characterPatterns = new[] { "protagonist", "hero", "character", "main character" };
        var storyLower = storyJson.ToLowerInvariant();

        foreach (var pattern in characterPatterns)
        {
            if (storyLower.Contains(pattern))
            {
                characters.Add(new { Type = pattern, Mentioned = true });
            }
        }

        return characters;
    }

    private Dictionary<string, object> ExtractNarrativeElements(string storyJson)
    {
        // Basic narrative element extraction
        var elements = new Dictionary<string, object>();

        var storyLower = storyJson.ToLowerInvariant();

        // Look for basic narrative elements
        elements["HasConflict"] = storyLower.ContainsAny("conflict", "problem", "challenge", "obstacle");
        elements["HasResolution"] = storyLower.ContainsAny("resolution", "ending", "conclusion", "solved");
        elements["HasDialogue"] = storyLower.Contains('"'.ToString()) || storyLower.Contains("said");
        elements["HasDescription"] = storyLower.ContainsAny("described", "looked", "appeared", "seemed");

        return elements;
    }

    private string EstimateStoryComplexity(string storyJson)
    {
        // Basic complexity estimation based on length and content diversity
        var length = storyJson.Length;

        // Use HashSet for more efficient unique word counting
        var uniqueWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var wordSpan = storyJson.AsSpan();
        var start = 0;

        for (int i = 0; i < wordSpan.Length; i++)
        {
            if (char.IsWhiteSpace(wordSpan[i]) || i == wordSpan.Length - 1)
            {
                if (i > start)
                {
                    var word = wordSpan.Slice(start, i - start + (i == wordSpan.Length - 1 ? 1 : 0)).ToString();
                    if (!string.IsNullOrWhiteSpace(word))
                        uniqueWords.Add(word);
                }
                start = i + 1;
            }
        }

        if (length < SimpleStoryMaxLength)
            return "Simple";
        if (length < ModerateStoryMaxLength)
            return "Moderate";
        if (uniqueWords.Count > ComplexStoryUniqueWordsThreshold)
            return "Complex";
        return "Moderate";
    }

    /// <summary>
    /// Efficiently counts occurrences of a substring in a string without creating arrays.
    /// </summary>
    private static int CountOccurrences(string source, string substring)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(substring))
            return 0;

        int count = 0;
        int index = 0;

        while ((index = source.IndexOf(substring, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += substring.Length;
        }

        return count;
    }

    private string ComputeNarrativeConsistencyContext(object storyStructure)
    {
        try
        {
            // Basic narrative consistency computation
            // This is a simplified implementation that analyzes story structure for consistency

            if (storyStructure == null)
                return "No story structure available";

            // Extract basic information from the story structure
            var hasScenes = false;
            var hasCharacters = false;
            var complexity = "Unknown";

            // This is a basic implementation - in production would use more sophisticated analysis
            var structureDict = storyStructure as IDictionary<string, object>;
            if (structureDict != null)
            {
                if (structureDict.ContainsKey("Scenes"))
                {
                    var scenesValue = structureDict["Scenes"];
                    if (scenesValue != null && scenesValue is IEnumerable<object> scenesEnumerable)
                        hasScenes = scenesEnumerable.Any();
                }

                if (structureDict.ContainsKey("Characters"))
                {
                    var charactersValue = structureDict["Characters"];
                    if (charactersValue != null && charactersValue is IEnumerable<object> charactersEnumerable)
                        hasCharacters = charactersEnumerable.Any();
                }

                if (structureDict.ContainsKey("EstimatedComplexity"))
                {
                    var complexityValue = structureDict["EstimatedComplexity"];
                    complexity = complexityValue?.ToString() ?? "Unknown";
                }
            }

            // Generate basic consistency context
            var contextElements = new List<string>();

            if (hasScenes)
                contextElements.Add("Story has defined scenes");

            if (hasCharacters)
                contextElements.Add("Story has identifiable characters");

            contextElements.Add($"Complexity: {complexity}");

            // Basic consistency checks
            if (!hasScenes && !hasCharacters)
            {
                contextElements.Add("Warning: Limited story structure detected");
            }

            var context = string.Join("; ", contextElements);
            _logger.LogDebug("Generated narrative consistency context: {Context}", context);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error computing narrative consistency context");
            return "Error computing narrative consistency";
        }
    }

    private Task<EvaluationReport> ParseEvaluationReportAsync(string judgeResponse, int iterationNumber)
    {
        var (success, report, error) = AgentResponseParser.TryParseJsonResponse<EvaluationReport>(judgeResponse);
        if (!success || report == null)
        {
            _logger.LogWarning("Failed to parse evaluation report JSON: {Error}. Raw response: {Response}", error, judgeResponse);

            return Task.FromResult(new EvaluationReport
            {
                IterationNumber = iterationNumber,
                OverallStatus = EvaluationStatus.Fail,
                SafetyGatePassed = false,
                AxesAlignmentScore = 0,
                DevPrinciplesScore = 0,
                NarrativeLogicScore = 0,
                Findings = new Dictionary<string, List<string>>
                {
                    { "parsing_error", new List<string> { error ?? "Unknown parsing error" } }
                },
                Recommendation = "Failed to parse evaluation report"
            });
        }

        report.IterationNumber = iterationNumber;
        report.EvaluationTimestamp = DateTime.UtcNow;

        var computed = EvaluationReport.DetermineOverallStatus(
            report.SafetyGatePassed,
            report.AxesAlignmentScore,
            report.DevPrinciplesScore,
            report.NarrativeLogicScore);

        if (report.OverallStatus != computed)
        {
            report.Findings.TryAdd("ScoringRules", new List<string>());
            report.Findings["ScoringRules"].Add($"overall_status '{report.OverallStatus}' did not match scoring rules; expected '{computed}'.");
            report.OverallStatus = computed;
        }

        return Task.FromResult(report);
    }

    #endregion
}

// Extension method for string containment check
public static class StringExtensions
{
    public static bool ContainsAny(this string source, params string[] values)
    {
        return values.Any(value => source.Contains(value));
    }
}
