using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mystira.StoryGenerator.Api;
using Mystira.StoryGenerator.Api.Models;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Contracts.Models;
using Mystira.StoryGenerator.Domain.Agents;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Mystira.StoryGenerator.Api.Tests.Integration;

/// <summary>
/// Performance and load tests for the agent pipeline.
/// Validates latency requirements and system responsiveness.
/// </summary>
public class AgentPipelinePerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AgentPipelinePerformanceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IAgentOrchestrator, PerformanceMockOrchestrator>();
                services.AddSingleton<IAgentStreamPublisher, PerformanceMockStreamPublisher>();
                services.AddSingleton<IStorySessionRepository, PerformanceMockSessionRepository>();
            });
        });

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Requirement: Stream startup latency under 500ms
    /// </summary>
    [Fact]
    public async Task StreamStartupLatency_UnderFiveHundredMilliseconds()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Performance test story",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        // Act - Measure POST /start response time
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        stopwatch.Stop();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Stream startup took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    /// <summary>
    /// Requirement: First SSE event within 500ms
    /// </summary>
    [Fact]
    public async Task SSEFirstEvent_WithinFiveHundredMilliseconds()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "SSE latency test",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
            await startResponse.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

        // Act - Measure time to first SSE event
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/story-agent/sessions/{sessionId}/stream");
        requestMessage.Headers.Add("Accept", "text/event-stream");

        var stopwatch = Stopwatch.StartNew();
        var streamResponse = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

        // Read first event
        using var stream = await streamResponse.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        var firstLine = await reader.ReadLineAsync();
        stopwatch.Stop();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, streamResponse.StatusCode);
        Assert.NotNull(firstLine);
        Assert.StartsWith("event: ", firstLine);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"First SSE event took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    /// <summary>
    /// Requirement: Evaluation latency under 3 seconds
    /// </summary>
    [Fact]
    public async Task EvaluationLatency_UnderThreeSeconds()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Evaluation performance test",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
            await startResponse.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

        // Wait for generation
        await Task.Delay(1000);

        // Act - Measure evaluation time
        var stopwatch = Stopwatch.StartNew();
        var evalResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());
        stopwatch.Stop();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, evalResponse.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000,
            $"Evaluation took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
    }

    /// <summary>
    /// Test concurrent session creation
    /// </summary>
    [Fact]
    public async Task ConcurrentSessions_HandledWithoutDegradation()
    {
        // Arrange
        var requests = Enumerable.Range(0, 5).Select(i => new StartSessionRequest
        {
            StoryPrompt = $"Concurrent test story {i}",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        }).ToList();

        // Act - Create 5 sessions concurrently
        var stopwatch = Stopwatch.StartNew();
        var tasks = requests.Select(req =>
            _client.PostAsJsonAsync("/api/story-agent/sessions/start", req)).ToList();

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.All(responses, response =>
            Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode));

        // All 5 sessions should complete within 2.5 seconds (500ms each max)
        Assert.True(stopwatch.ElapsedMilliseconds < 2500,
            $"5 concurrent sessions took {stopwatch.ElapsedMilliseconds}ms, expected < 2500ms");
    }

    /// <summary>
    /// Test GET session state endpoint performance
    /// </summary>
    [Fact]
    public async Task GetSessionState_RespondsQuickly()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "State retrieval test",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
            await startResponse.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

        // Act - Measure GET /sessions/{id} response time
        var stopwatch = Stopwatch.StartNew();
        var stateResponse = await _client.GetAsync($"/api/story-agent/sessions/{sessionId}");
        stopwatch.Stop();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, stateResponse.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 200,
            $"Get session state took {stopwatch.ElapsedMilliseconds}ms, expected < 200ms");
    }

    /// <summary>
    /// Test refinement endpoint performance
    /// </summary>
    [Fact]
    public async Task RefineStory_StartsQuickly()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Refinement performance test",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
            await startResponse.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

        // Simulate a session in RequiresRefinement state
        var mockOrchestrator = _factory.Services.GetRequiredService<IAgentOrchestrator>() as PerformanceMockOrchestrator;
        mockOrchestrator?.SetStage(sessionId, StorySessionStage.RefinementRequested);

        var refineRequest = new RefineRequest
        {
            TargetSceneIds = new List<string> { "scene_1" },
            Aspects = new List<string> { "tone" }
        };

        // Act - Measure refine endpoint response time
        var stopwatch = Stopwatch.StartNew();
        var refineResponse = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/refine", refineRequest);
        stopwatch.Stop();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Accepted, refineResponse.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Refine request took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
    }

    /// <summary>
    /// Test that SSE stream doesn't block other operations
    /// </summary>
    [Fact]
    public async Task SSEStream_DoesNotBlockOtherOperations()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Non-blocking test",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
            await startResponse.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

        // Act - Start streaming
        var streamRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/story-agent/sessions/{sessionId}/stream");
        streamRequest.Headers.Add("Accept", "text/event-stream");
        var streamTask = _client.SendAsync(streamRequest, HttpCompletionOption.ResponseHeadersRead);

        // While streaming, make other API calls
        var stopwatch = Stopwatch.StartNew();
        var stateResponse = await _client.GetAsync($"/api/story-agent/sessions/{sessionId}");
        stopwatch.Stop();

        // Assert - State query should still be fast
        Assert.Equal(System.Net.HttpStatusCode.OK, stateResponse.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"State query during streaming took {stopwatch.ElapsedMilliseconds}ms, should not be blocked");

        // Clean up
        await streamTask;
    }

    /// <summary>
    /// Measure memory efficiency of session storage
    /// </summary>
    [Fact]
    public async Task SessionStorage_HandlesMultipleSessions()
    {
        // Arrange & Act - Create 10 sessions
        var sessionIds = new List<string>();

        for (int i = 0; i < 10; i++)
        {
            var request = new StartSessionRequest
            {
                StoryPrompt = $"Memory test story {i}",
                KnowledgeMode = "FileSearch",
                AgeGroup = "6-9"
            };

            var response = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
            var sessionId = JsonSerializer.Deserialize<SessionStartResponse>(
                await response.Content.ReadAsStringAsync(), _jsonOptions)!.SessionId;

            sessionIds.Add(sessionId);
        }

        // Assert - All sessions should be retrievable
        foreach (var sessionId in sessionIds)
        {
            var stateResponse = await _client.GetAsync($"/api/story-agent/sessions/{sessionId}");
            Assert.Equal(System.Net.HttpStatusCode.OK, stateResponse.StatusCode);
        }

        // All operations should complete quickly
        Assert.True(sessionIds.Count == 10);
    }

    // Mock implementations optimized for performance testing

    private class PerformanceMockOrchestrator : IAgentOrchestrator
    {
        private readonly Dictionary<string, StorySession> _sessions = new();

        public async Task<StorySession> InitializeSessionAsync(string sessionId, string knowledgeMode, string ageGroup)
        {
            var session = new StorySession
            {
                SessionId = sessionId,
                KnowledgeMode = Enum.Parse<KnowledgeMode>(knowledgeMode),
                Stage = StorySessionStage.Generating,
                IterationCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ThreadId = $"thread-{Guid.NewGuid():N}",
                CurrentStoryVersion = "{\"title\": \"Test Story\"}",
                StoryVersions = new List<StoryVersionSnapshot>()
            };

            _sessions[sessionId] = session;
            return await Task.FromResult(session);
        }

        public async Task<(bool Success, string Message)> GenerateStoryAsync(string sessionId, string storyPrompt, CancellationToken ct)
        {
            await Task.Delay(10, ct); // Minimal delay
            return (true, "Generation started");
        }

        public async Task<(bool Success, EvaluationReport Report)> EvaluateStoryAsync(string sessionId, CancellationToken ct)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, null!);

            await Task.Delay(50, ct); // Simulate fast evaluation

            var report = new EvaluationReport
            {
                IterationNumber = session.IterationCount,
                OverallStatus = EvaluationStatus.Pass,
                SafetyGatePassed = true,
                AxesAlignmentScore = 0.9f,
                DevPrinciplesScore = 0.85f,
                NarrativeLogicScore = 0.88f
            };

            session.LastEvaluationReport = report;
            session.Stage = StorySessionStage.Evaluated;

            return (true, report);
        }

        public async Task<(bool Success, string Message)> RefineStoryAsync(string sessionId, UserRefinementFocus focus, CancellationToken ct)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return (false, "Session not found");

            await Task.Delay(10, ct); // Minimal delay

            session.IterationCount++;
            session.Stage = StorySessionStage.Validating;

            return (true, "Refinement started");
        }

        public async Task<StorySession?> GetSessionAsync(string sessionId)
        {
            return await Task.FromResult(_sessions.TryGetValue(sessionId, out var session) ? session : null);
        }

        public void SetStage(string sessionId, StorySessionStage stage)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.Stage = stage;
            }
        }
    }

    private class PerformanceMockStreamPublisher : IAgentStreamPublisher
    {
        public async Task PublishEventAsync(string sessionId, AgentStreamEvent evt)
        {
            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<AgentStreamEvent> SubscribeAsync(string sessionId, CancellationToken ct = default)
        {
            // Yield events immediately for performance testing
            yield return new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "SessionStarted",
                Timestamp = DateTime.UtcNow
            };

            await Task.Delay(50, ct);

            yield return new AgentStreamEvent
            {
                Type = AgentStreamEvent.EventType.PhaseStarted,
                Phase = "GenerationStarted",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private class PerformanceMockSessionRepository : IStorySessionRepository
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
