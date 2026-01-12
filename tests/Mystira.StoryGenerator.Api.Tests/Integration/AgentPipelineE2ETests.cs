using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mystira.StoryGenerator.Api;
using Mystira.StoryGenerator.Api.Models;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Contracts.Models;
using Mystira.StoryGenerator.Domain.Agents;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using SessionStateResponse = Mystira.StoryGenerator.Contracts.Models.SessionStateResponse;

namespace Mystira.StoryGenerator.Api.Tests.Integration;

/// <summary>
/// Comprehensive end-to-end integration tests for the complete agent pipeline.
/// Tests demo scenarios from story initialization through generation, evaluation, and refinement.
/// </summary>
public class AgentPipelineE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AgentPipelineE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Use E2E mock orchestrator for complete pipeline testing
                services.AddSingleton<IAgentOrchestrator, E2EMockAgentOrchestrator>();
                services.AddSingleton<IAgentStreamPublisher, E2EMockStreamPublisher>();
                services.AddSingleton<IStorySessionRepository, E2EMockSessionRepository>();
            });
        });

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Demo Scenario #1: Happy Path
    /// Generate story → Evaluate (all gates pass) → Complete
    /// </summary>
    [Fact]
    public async Task DemoScenario1_HappyPath_SuccessfulGenerationWithPositiveEvaluation()
    {
        // Step 1: Initialize session
        var startRequest = new StartSessionRequest
        {
            StoryPrompt = "A brave knight helps villagers defend their town from a dragon",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", startRequest);
        Assert.Equal(System.Net.HttpStatusCode.Accepted, startResponse.StatusCode);

        var startContent = await startResponse.Content.ReadAsStringAsync();
        var startResult = JsonSerializer.Deserialize<SessionStartResponse>(startContent, _jsonOptions);
        Assert.NotNull(startResult);

        var sessionId = startResult.SessionId;
        Assert.NotEmpty(sessionId);
        Assert.NotEmpty(startResult.ThreadId);
        Assert.Equal("FileSearch", startResult.KnowledgeMode);

        // Step 2: Poll until generation complete
        await PollUntilStageAsync(sessionId, "Validating", maxAttempts: 20);

        // Step 3: Get session state to verify story was generated
        var stateResponse = await _client.GetAsync($"/api/story-agent/sessions/{sessionId}");
        Assert.Equal(System.Net.HttpStatusCode.OK, stateResponse.StatusCode);

        var stateContent = await stateResponse.Content.ReadAsStringAsync();
        var stateResult = JsonSerializer.Deserialize<SessionStateResponse>(stateContent, _jsonOptions);
        Assert.NotNull(stateResult);
        Assert.NotNull(stateResult.CurrentStoryJson);
        Assert.Contains("\"title\"", stateResult.CurrentStoryJson);

        // Step 4: Evaluate story
        var evaluateResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());
        Assert.Equal(System.Net.HttpStatusCode.OK, evaluateResponse.StatusCode);

        var evaluateContent = await evaluateResponse.Content.ReadAsStringAsync();
        var evaluateResult = JsonSerializer.Deserialize<EvaluateResponse>(evaluateContent, _jsonOptions);
        Assert.NotNull(evaluateResult);
        Assert.NotNull(evaluateResult.EvaluationReport);

        // Step 5: Verify evaluation passed
        Assert.Equal(EvaluationStatus.Pass, evaluateResult.EvaluationReport.OverallStatus);
        Assert.True(evaluateResult.EvaluationReport.SafetyGatePassed);
        Assert.True(evaluateResult.EvaluationReport.AxesAlignmentScore >= 0.7f);
        Assert.True(evaluateResult.EvaluationReport.DevPrinciplesScore >= 0.7f);
        Assert.True(evaluateResult.EvaluationReport.NarrativeLogicScore >= 0.7f);

        // Step 6: Verify final state
        var finalState = await GetSessionStateAsync(sessionId);
        Assert.Equal("Evaluated", finalState.Stage);
        Assert.Equal(0, finalState.IterationCount); // No refinements needed
        Assert.NotNull(finalState.LastEvaluationReport);
    }

    /// <summary>
    /// Demo Scenario #2: Failure + Targeted Refinement
    /// Generate → Evaluate (fail) → Refine with target_scene_ids → Re-evaluate (pass)
    /// </summary>
    [Fact]
    public async Task DemoScenario2_FailedEvaluation_ThenTargetedRefinement_Success()
    {
        // Step 1: Initialize and generate story
        var startRequest = new StartSessionRequest
        {
            StoryPrompt = "A story that will initially fail evaluation",
            KnowledgeMode = "AISearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", startRequest);
        var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
            await startResponse.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

        await PollUntilStageAsync(sessionId, "Validating");

        // Step 2: Evaluate - expect failure
        var firstEvalResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());
        var firstEvalResult = JsonSerializer.Deserialize<EvaluateResponse>(
            await firstEvalResponse.Content.ReadAsStringAsync(), _jsonOptions);

        Assert.NotNull(firstEvalResult);
        Assert.Equal(EvaluationStatus.Fail, firstEvalResult.EvaluationReport.OverallStatus);

        // Verify state transition to RefinementRequested
        var stateAfterEval = await GetSessionStateAsync(sessionId);
        Assert.Equal("RefinementRequested", stateAfterEval.Stage);

        // Step 3: Targeted refinement on specific scenes
        var refineRequest = new RefineRequest
        {
            TargetSceneIds = new List<string> { "scene_2", "scene_3" },
            Aspects = new List<string> { "dialogue", "tone" },
            Constraints = "Make the dialogue more age-appropriate and improve the tone"
        };

        var refineResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/refine", refineRequest);
        Assert.Equal(System.Net.HttpStatusCode.Accepted, refineResponse.StatusCode);

        // Wait for refinement to complete
        await PollUntilStageAsync(sessionId, "Validating");

        // Verify iteration count incremented
        var stateAfterRefine = await GetSessionStateAsync(sessionId);
        Assert.Equal(1, stateAfterRefine.IterationCount);

        // Step 4: Re-evaluate - expect pass
        var secondEvalResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());
        var secondEvalResult = JsonSerializer.Deserialize<EvaluateResponse>(
            await secondEvalResponse.Content.ReadAsStringAsync(), _jsonOptions);

        Assert.NotNull(secondEvalResult);
        Assert.Equal(EvaluationStatus.Pass, secondEvalResult.EvaluationReport.OverallStatus);
        Assert.Equal(1, secondEvalResult.EvaluationReport.IterationNumber);

        // Verify story versions history
        var finalState = await GetSessionStateAsync(sessionId);
        Assert.NotNull(finalState.StoryVersions);
        Assert.True(finalState.StoryVersions.Count >= 2); // Original + refined
    }

    /// <summary>
    /// Demo Scenario #3: Max Iterations Escalation
    /// Generate → Evaluate (fail) → Refine → Evaluate (fail) × 5 → State = StuckNeedsReview
    /// </summary>
    [Fact]
    public async Task DemoScenario3_MaxIterations_EscalatesAfterFiveAttempts()
    {
        // Step 1: Initialize session with perpetual failure mode
        var startRequest = new StartSessionRequest
        {
            StoryPrompt = "A story that will always fail evaluation",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", startRequest);
        var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
            await startResponse.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

        await PollUntilStageAsync(sessionId, "Validating");

        // Perform evaluation + refinement loop up to 5 times
        for (int i = 0; i < 5; i++)
        {
            // Evaluate
            var evalResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());
            var evalResult = JsonSerializer.Deserialize<EvaluateResponse>(
                await evalResponse.Content.ReadAsStringAsync(), _jsonOptions);

            Assert.NotNull(evalResult);

            if (i < 4)
            {
                // First 4 iterations should return Fail
                Assert.Equal(EvaluationStatus.Fail, evalResult.EvaluationReport.OverallStatus);

                var state = await GetSessionStateAsync(sessionId);
                Assert.Equal("RefinementRequested", state.Stage);
                Assert.Equal(i, state.IterationCount);

                // Refine
                var refineRequest = new RefineRequest
                {
                    TargetSceneIds = new List<string> { "scene_1" },
                    Aspects = new List<string> { "all" }
                };

                var refineResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/refine", refineRequest);
                Assert.Equal(System.Net.HttpStatusCode.Accepted, refineResponse.StatusCode);

                await PollUntilStageAsync(sessionId, "Validating");
            }
            else
            {
                // 5th iteration should escalate
                Assert.Equal(EvaluationStatus.Fail, evalResult.EvaluationReport.OverallStatus);

                var finalState = await GetSessionStateAsync(sessionId);
                Assert.Equal("StuckNeedsReview", finalState.Stage);
                Assert.Equal(5, finalState.IterationCount);
                Assert.Contains("maximum iterations", evalResult.EvaluationReport.Recommendation ?? "", StringComparison.OrdinalIgnoreCase);

                // Further refinement should be rejected
                var refineRequest = new RefineRequest { TargetSceneIds = new List<string> { "scene_1" } };
                var refineResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/refine", refineRequest);
                Assert.Equal(System.Net.HttpStatusCode.Conflict, refineResponse.StatusCode);
            }
        }
    }

    /// <summary>
    /// Complete pipeline test: UI to final story
    /// Tests all API calls working end-to-end with proper state transitions
    /// </summary>
    [Fact]
    public async Task CompleteAgentPipeline_FromUIToFinalStory()
    {
        // Simulate complete user workflow

        // 1. User starts session from UI
        var startRequest = new StartSessionRequest
        {
            StoryPrompt = "A wizard teaches a young apprentice about magic",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", startRequest);
        Assert.Equal(System.Net.HttpStatusCode.Accepted, startResponse.StatusCode);

        var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
            await startResponse.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

        // 2. UI polls for generation completion
        var stateAfterGen = await PollUntilStageAsync(sessionId, "Validating", maxAttempts: 30);
        Assert.NotNull(stateAfterGen.CurrentStoryJson);

        // 3. UI automatically triggers evaluation
        var evalResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());
        Assert.Equal(System.Net.HttpStatusCode.OK, evalResponse.StatusCode);

        var evalResult = JsonSerializer.Deserialize<EvaluateResponse>(
            await evalResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // 4. If failed, UI offers refinement option
        if (evalResult!.EvaluationReport.OverallStatus == EvaluationStatus.Fail)
        {
            var refineRequest = new RefineRequest
            {
                TargetSceneIds = new List<string> { "scene_1" },
                Aspects = new List<string> { "tone" }
            };

            var refineResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/refine", refineRequest);
            Assert.Equal(System.Net.HttpStatusCode.Accepted, refineResponse.StatusCode);

            await PollUntilStageAsync(sessionId, "Validating");

            // Re-evaluate
            evalResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());
            evalResult = JsonSerializer.Deserialize<EvaluateResponse>(
                await evalResponse.Content.ReadAsStringAsync(), _jsonOptions);
        }

        // 5. Verify final story is valid and ready to publish
        var finalState = await GetSessionStateAsync(sessionId);
        Assert.NotNull(finalState.CurrentStoryJson);
        Assert.NotNull(finalState.LastEvaluationReport);
        Assert.Contains(finalState.Stage, new[] { "Evaluated", "Complete" });

        // Verify story is valid JSON
        var storyJson = JsonSerializer.Deserialize<JsonDocument>(finalState.CurrentStoryJson);
        Assert.NotNull(storyJson);
    }

    /// <summary>
    /// Test switching knowledge modes between sessions
    /// </summary>
    [Fact]
    public async Task KnowledgeModes_CanSwitchBetweenSessions()
    {
        // Create session with FileSearch
        var fileSearchRequest = new StartSessionRequest
        {
            StoryPrompt = "Story with FileSearch mode",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var fileSearchResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", fileSearchRequest);
        var fileSearchSession = JsonSerializer.Deserialize<SessionStartResponse>(
            await fileSearchResponse.Content.ReadAsStringAsync(), _jsonOptions);

        Assert.Equal("FileSearch", fileSearchSession!.KnowledgeMode);

        // Create session with AISearch
        var aiSearchRequest = new StartSessionRequest
        {
            StoryPrompt = "Story with AISearch mode",
            KnowledgeMode = "AISearch",
            AgeGroup = "6-9"
        };

        var aiSearchResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", aiSearchRequest);
        var aiSearchSession = JsonSerializer.Deserialize<SessionStartResponse>(
            await aiSearchResponse.Content.ReadAsStringAsync(), _jsonOptions);

        Assert.Equal("AISearch", aiSearchSession!.KnowledgeMode);

        // Both sessions should work independently
        Assert.NotEqual(fileSearchSession.SessionId, aiSearchSession.SessionId);
        Assert.NotEqual(fileSearchSession.ThreadId, aiSearchSession.ThreadId);
    }

    // Helper methods

    private async Task<SessionStateResponse> GetSessionStateAsync(string sessionId)
    {
        var response = await _client.GetAsync($"/api/story-agent/sessions/{sessionId}");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SessionStateResponse>(content, _jsonOptions)!;
    }

    private async Task<SessionStateResponse> PollUntilStageAsync(string sessionId, string targetStage, int maxAttempts = 20)
    {
        SessionStateResponse? state = null;

        for (int i = 0; i < maxAttempts; i++)
        {
            state = await GetSessionStateAsync(sessionId);

            if (state.Stage == targetStage)
                return state;

            await Task.Delay(500);
        }

        Assert.Fail($"Session did not reach stage '{targetStage}' after {maxAttempts} attempts. Current stage: {state?.Stage}");
        return state!;
    }

    // Mock implementations for E2E testing

    private class E2EMockAgentOrchestrator : IAgentOrchestrator
    {
        private readonly Dictionary<string, StorySession> _sessions = new();
        private readonly Dictionary<string, string> _prompts = new();

        public async Task<StorySession> InitializeSessionAsync(string sessionId, string knowledgeMode, string ageGroup)
        {
            var session = new StorySession
            {
                SessionId = sessionId,
                KnowledgeMode = Enum.Parse<KnowledgeMode>(knowledgeMode),
                Stage = StorySessionStage.Uninitialized,
                IterationCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ThreadId = $"thread-{Guid.NewGuid():N}",
                StoryVersions = new List<StoryVersionSnapshot>()
            };

            _sessions[sessionId] = session;

            // Simulate async generation
            _ = Task.Run(async () =>
            {
                await Task.Delay(200);
                session.Stage = StorySessionStage.Generating;

                await Task.Delay(500);
                session.CurrentStoryVersion = GenerateMockStory();
                session.StoryVersions.Add(new StoryVersionSnapshot
                {
                    VersionNumber = 1,
                    StoryJson = session.CurrentStoryVersion,
                    CreatedAt = DateTime.UtcNow
                });
                session.Stage = StorySessionStage.Validating;
            });

            return await Task.FromResult(session);
        }

        public async Task<(bool Success, string Message)> GenerateStoryAsync(string sessionId, string storyPrompt, CancellationToken ct)
        {
            _prompts[sessionId] = storyPrompt;
            await Task.Delay(100, ct);
            return (true, "Generation started");
        }

        public async Task<(bool Success, EvaluationReport Report)> EvaluateStoryAsync(string sessionId, CancellationToken ct)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, null!);

            await Task.Delay(100, ct);

            var prompt = _prompts.GetValueOrDefault(sessionId, "");
            var shouldFailInitially = prompt.Contains("will initially fail");
            var maxIterMode = prompt.Contains("will always fail");

            EvaluationReport report;

            if (maxIterMode)
            {
                // Always fail until max iterations
                if (session.IterationCount >= 5)
                {
                    session.Stage = StorySessionStage.StuckNeedsReview;
                    report = new EvaluationReport
                    {
                        IterationNumber = session.IterationCount,
                        OverallStatus = EvaluationStatus.Fail,
                        SafetyGatePassed = false,
                        Recommendation = "Maximum iterations reached. Needs human review."
                    };
                }
                else
                {
                    session.Stage = StorySessionStage.RefinementRequested;
                    report = new EvaluationReport
                    {
                        IterationNumber = session.IterationCount,
                        OverallStatus = EvaluationStatus.Fail,
                        SafetyGatePassed = true,
                        AxesAlignmentScore = 0.5f,
                        DevPrinciplesScore = 0.5f,
                        NarrativeLogicScore = 0.5f
                    };
                }
            }
            else if (shouldFailInitially && session.IterationCount == 0)
            {
                // Fail on first evaluation, pass on subsequent
                session.Stage = StorySessionStage.RefinementRequested;
                report = new EvaluationReport
                {
                    IterationNumber = session.IterationCount,
                    OverallStatus = EvaluationStatus.Fail,
                    SafetyGatePassed = true,
                    AxesAlignmentScore = 0.5f,
                    DevPrinciplesScore = 0.6f,
                    NarrativeLogicScore = 0.55f,
                    Recommendation = "Story needs refinement"
                };
            }
            else
            {
                // Pass
                session.Stage = StorySessionStage.Evaluated;
                report = new EvaluationReport
                {
                    IterationNumber = session.IterationCount,
                    OverallStatus = EvaluationStatus.Pass,
                    SafetyGatePassed = true,
                    AxesAlignmentScore = 0.9f,
                    DevPrinciplesScore = 0.85f,
                    NarrativeLogicScore = 0.88f,
                    Recommendation = "Story is ready"
                };
            }

            session.LastEvaluationReport = report;
            return (true, report);
        }

        public async Task<(bool Success, string Message)> RefineStoryAsync(string sessionId, UserRefinementFocus focus, CancellationToken ct)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found");

            if (session.Stage == StorySessionStage.StuckNeedsReview)
                return (false, "Session stuck, cannot refine");

            session.IterationCount++;

            // Simulate refinement
            await Task.Delay(100, ct);

            var refinedStory = GenerateMockStory($" (refined v{session.IterationCount})");
            session.CurrentStoryVersion = refinedStory;
            session.StoryVersions.Add(new StoryVersionSnapshot
            {
                VersionNumber = session.IterationCount + 1,
                StoryJson = refinedStory,
                CreatedAt = DateTime.UtcNow
            });

            session.Stage = StorySessionStage.Validating;

            return (true, "Refinement completed");
        }

        public async Task<StorySession?> GetSessionAsync(string sessionId)
        {
            return await Task.FromResult(_sessions.TryGetValue(sessionId, out var session) ? session : null);
        }

        private string GenerateMockStory(string suffix = "")
        {
            return $$"""
            {
                "title": "Mock Story{{suffix}}",
                "scenes": [
                    {
                        "id": "scene_1",
                        "title": "The Beginning",
                        "content": "Once upon a time..."
                    },
                    {
                        "id": "scene_2",
                        "title": "The Middle",
                        "content": "Adventure happens here..."
                    },
                    {
                        "id": "scene_3",
                        "title": "The End",
                        "content": "And they lived happily ever after."
                    }
                ]
            }
            """;
        }
    }

    private class E2EMockStreamPublisher : IAgentStreamPublisher
    {
        public async Task PublishEventAsync(string sessionId, AgentStreamEvent evt)
        {
            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<AgentStreamEvent> SubscribeAsync(string sessionId, CancellationToken ct = default)
        {
            yield break;
        }
    }

    private class E2EMockSessionRepository : IStorySessionRepository
    {
        private readonly Dictionary<string, StorySession> _sessions = new();

        public Task<StorySession> CreateAsync(StorySession session, CancellationToken cancellationToken = default)
        {
            _sessions[session.SessionId] = session;
            return Task.FromResult(session);
        }

        public Task<StorySession?> GetAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_sessions.TryGetValue(sessionId, out var session) ? session : null);
        }

        public Task<StorySession> UpdateAsync(StorySession session, CancellationToken cancellationToken = default)
        {
            _sessions[session.SessionId] = session;
            return Task.FromResult(session);
        }

        public Task<StorySession> UpsertAsync(StorySession session, CancellationToken cancellationToken = default)
        {
            _sessions[session.SessionId] = session;
            return Task.FromResult(session);
        }

        public Task<StorySession?> GetByThreadIdAsync(string threadId, CancellationToken cancellationToken = default)
        {
            var session = _sessions.Values.FirstOrDefault(s => s.ThreadId == threadId);
            return Task.FromResult(session);
        }
    }
}
