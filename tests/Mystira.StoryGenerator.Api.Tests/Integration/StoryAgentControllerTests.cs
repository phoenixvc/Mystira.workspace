using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Mystira.StoryGenerator.Api;
using Mystira.StoryGenerator.Api.Models;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Domain.Agents;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mystira.StoryGenerator.Api.Tests.Integration;

/// <summary>
/// Integration tests for StoryAgentController endpoints.
/// </summary>
public class StoryAgentControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StoryAgentControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Mock the dependencies
                services.AddSingleton<IAgentOrchestrator, MockAgentOrchestrator>();
                services.AddSingleton<IAgentStreamPublisher, MockStreamPublisher>();
                services.AddSingleton<IStorySessionRepository, MockSessionRepository>();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task StartSession_Returns202Accepted_WithSessionId()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Tell me a story about a brave little mouse",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SessionStartResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.SessionId);
        Assert.NotEmpty(result.ThreadId);
        Assert.Equal("FileSearch", result.KnowledgeMode);
        Assert.Equal("Uninitialized", result.Stage);
    }

    [Fact]
    public async Task StartSession_ReturnsBadRequest_OnInvalidKnowledgeMode()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Tell me a story",
            KnowledgeMode = "InvalidMode",
            AgeGroup = "6-9"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StartSession_ReturnsBadRequest_OnMissingRequiredFields()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "",
            KnowledgeMode = "FileSearch",
            AgeGroup = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_Returns404_OnSessionNotFound()
    {
        // Arrange
        var sessionId = "non-existent-session";

        // Act
        var response = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_Returns409_OnInvalidState()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Tell me a story",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };
        
        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        var startContent = await startResponse.Content.ReadAsStringAsync();
        var startResult = JsonSerializer.Deserialize<SessionStartResponse>(startContent, _jsonOptions);
        
        var sessionId = startResult!.SessionId;

        // Act - Try to evaluate when session is in wrong state
        var response = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/evaluate", new EvaluateRequest());

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Refine_Returns404_OnSessionNotFound()
    {
        // Arrange
        var sessionId = "non-existent-session";
        var request = new RefineRequest
        {
            TargetSceneIds = new List<string> { "scene1" },
            Aspects = new List<string> { "dialogue" }
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/refine", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Refine_Returns409_OnInvalidState()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Tell me a story",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };
        
        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        var startContent = await startResponse.Content.ReadAsStringAsync();
        var startResult = JsonSerializer.Deserialize<SessionStartResponse>(startContent, _jsonOptions);
        
        var sessionId = startResult!.SessionId;

        var refineRequest = new RefineRequest
        {
            TargetSceneIds = new List<string> { "scene1" },
            Aspects = new List<string> { "dialogue" }
        };

        // Act - Try to refine when session is in wrong state
        var response = await _client.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/refine", refineRequest);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetSessionState_ReturnsCompleteSessionData()
    {
        // Arrange
        var request = new StartSessionRequest
        {
            StoryPrompt = "Tell me a story",
            KnowledgeMode = "FileSearch",
            AgeGroup = "6-9"
        };
        
        var startResponse = await _client.PostAsJsonAsync("/api/story-agent/sessions/start", request);
        var startContent = await startResponse.Content.ReadAsStringAsync();
        var startResult = JsonSerializer.Deserialize<SessionStartResponse>(startContent, _jsonOptions);
        
        var sessionId = startResult!.SessionId;

        // Act
        var response = await _client.GetAsync($"/api/story-agent/sessions/{sessionId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<SessionStateResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.SessionId);
        Assert.NotNull(result.Stage);
        Assert.True(result.IterationCount >= 0);
    }

    [Fact]
    public async Task GetSessionState_Returns404_OnSessionNotFound()
    {
        // Arrange
        var sessionId = "non-existent-session";

        // Act
        var response = await _client.GetAsync($"/api/story-agent/sessions/{sessionId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    // Mock implementations for testing
    private class MockAgentOrchestrator : IAgentOrchestrator
    {
        private readonly Dictionary<string, StorySession> _sessions = new();

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
                ThreadId = $"thread-{Guid.NewGuid():N}"
            };
            
            _sessions[sessionId] = session;
            return await Task.FromResult(session);
        }

        public async Task<(bool Success, string Message)> GenerateStoryAsync(string sessionId, string storyPrompt, CancellationToken ct)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.Stage = StorySessionStage.Generating;
                session.CurrentStoryVersion = "{\"title\": \"Generated Story\", \"content\": \"Story content here...\"}";
                await Task.Delay(100, ct); // Simulate work
                session.Stage = StorySessionStage.Validating;
                return (true, "Story generation started");
            }
            return (false, "Session not found");
        }

        public async Task<(bool Success, EvaluationReport Report)> EvaluateStoryAsync(string sessionId, CancellationToken ct)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.Stage = StorySessionStage.Evaluated;
                var report = new EvaluationReport
                {
                    IterationNumber = session.IterationCount,
                    OverallStatus = EvaluationStatus.Pass,
                    SafetyGatePassed = true,
                    AxesAlignmentScore = 0.9f,
                    DevPrinciplesScore = 0.8f,
                    NarrativeLogicScore = 0.85f,
                    Recommendation = "Story is ready"
                };
                session.LastEvaluationReport = report;
                await Task.Delay(50, ct);
                return (true, report);
            }
            return (false, null!);
        }

        public async Task<(bool Success, string Message)> RefineStoryAsync(string sessionId, UserRefinementFocus focus, CancellationToken ct)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.IterationCount++;
                session.Stage = StorySessionStage.Refined;
                await Task.Delay(50, ct);
                return (true, "Refinement completed");
            }
            return (false, "Session not found");
        }

        public async Task<StorySession?> GetSessionAsync(string sessionId)
        {
            return await Task.FromResult(_sessions.TryGetValue(sessionId, out var session) ? session : null);
        }
    }

    private class MockStreamPublisher : IAgentStreamPublisher
    {
        public async Task PublishEventAsync(string sessionId, AgentStreamEvent evt)
        {
            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<AgentStreamEvent> SubscribeAsync(string sessionId, CancellationToken ct = default)
        {
            yield break; // No events for mock
        }
    }

    private class MockSessionRepository : IStorySessionRepository
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

        public Task<StorySession?> GetByThreadIdAsync(string threadId, CancellationToken cancellationToken = default)
        {
            var session = _sessions.Values.FirstOrDefault(s => s.ThreadId == threadId);
            return Task.FromResult(session);
        }
    }
}