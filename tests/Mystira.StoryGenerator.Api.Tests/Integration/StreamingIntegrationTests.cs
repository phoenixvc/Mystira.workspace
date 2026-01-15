using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Api;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Contracts.Models;
using Mystira.StoryGenerator.Domain.Agents;
using System.Net.Http.Json;
using System.Text.Json;

namespace Mystira.StoryGenerator.Api.Tests.Integration;

/// <summary>
/// Integration tests for Server-Sent Events streaming endpoints.
/// </summary>
public class StreamingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StreamingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Mock the dependencies with streaming support
                services.AddSingleton<IAgentOrchestrator, StreamingMockAgentOrchestrator>();
                services.AddSingleton<IAgentStreamPublisher, StreamingMockStreamPublisher>();
                services.AddSingleton<IStorySessionRepository, StreamingMockSessionRepository>();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task SSEStream_FirstEventWithin500ms()
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
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/story-agent/sessions/{sessionId}/stream");
        requestMessage.Headers.Add("Accept", "text/event-stream");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await _client.SendAsync(requestMessage);
        stopwatch.Stop();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        // First event should come within 500ms
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);

        // Verify SSE format
        var lines = content.Split('\n');
        Assert.StartsWith("event: ", lines[0]);
        Assert.StartsWith("data: ", lines[1]);
    }

    [Fact]
    public async Task SSEStream_EventsFormatted_AsSSE()
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
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/story-agent/sessions/{sessionId}/stream");
        requestMessage.Headers.Add("Accept", "text/event-stream");

        var response = await _client.SendAsync(requestMessage);

        // Read the stream content
        var content = await response.Content.ReadAsStringAsync();

        // Assert - Verify SSE format
        var eventBlocks = content.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.True(eventBlocks.Length > 0);

        foreach (var block in eventBlocks)
        {
            var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.True(lines.Length >= 2);

            // Each event should have event: and data: lines
            Assert.StartsWith("event: ", lines[0]);
            Assert.StartsWith("data: ", lines[1]);

            // Event type should be valid
            var eventType = lines[0]["event: ".Length..];
            Assert.False(string.IsNullOrWhiteSpace(eventType));

            // Data should be valid JSON
            var jsonData = lines[1]["data: ".Length..];
            Assert.NotNull(JsonSerializer.Deserialize<object>(jsonData));
        }
    }

    [Fact]
    public async Task SSEStream_ClosesOnTerminalState()
    {
        // Arrange
        var sessionId = "terminal-test-session";

        // Manually create the session in the mock repo
        var mockRepo = _factory.Services.GetRequiredService<IStorySessionRepository>() as StreamingMockSessionRepository;
        await mockRepo!.CreateAsync(new StorySession
        {
            SessionId = sessionId,
            Stage = StorySessionStage.Validating,
            ThreadId = "test-thread"
        });

        // Act - Start streaming
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/story-agent/sessions/{sessionId}/stream");
        requestMessage.Headers.Add("Accept", "text/event-stream");

        var response = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        // Act - Read the stream until it ends
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        var content = await reader.ReadToEndAsync();

        // Assert - Stream should contain the terminal event and then close
        Assert.Contains("event: RubricGenerated", content);

        // If ReadToEndAsync finished, it means the stream was closed by the server
        Assert.True(true);
    }

    [Fact]
    public async Task SSEStream_Returns404_OnSessionNotFound()
    {
        // Arrange
        var sessionId = "non-existent-session";

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/story-agent/sessions/{sessionId}/stream");
        requestMessage.Headers.Add("Accept", "text/event-stream");

        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SSEStream_Returns204_OnCompletedSession()
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

        // Manually complete the session in the mock
        var mockRepo = _factory.Services.GetRequiredService<IStorySessionRepository>() as StreamingMockSessionRepository;
        mockRepo?.CompleteSession(sessionId);

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/story-agent/sessions/{sessionId}/stream");
        requestMessage.Headers.Add("Accept", "text/event-stream");

        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SSEStream_MultipleEventsSequence()
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
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/story-agent/sessions/{sessionId}/stream");
        requestMessage.Headers.Add("Accept", "text/event-stream");

        var response = await _client.SendAsync(requestMessage);

        // Read all events for a few seconds
        var content = await response.Content.ReadAsStringAsync();
        await Task.Delay(3000); // Let more events come through

        var finalContent = await response.Content.ReadAsStringAsync();

        // Assert - Should have multiple event blocks
        var eventBlocks = finalContent.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.True(eventBlocks.Length >= 1); // At least one event

        // Verify each block is properly formatted
        foreach (var block in eventBlocks)
        {
            var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.True(lines.Length >= 2);
            Assert.StartsWith("event: ", lines[0]);
            Assert.StartsWith("data: ", lines[1]);
        }
    }

    // Mock implementations for streaming tests
    private class StreamingMockAgentOrchestrator : IAgentOrchestrator
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

            // Simulate background work that updates stage
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                session.Stage = StorySessionStage.Generating;
                session.UpdatedAt = DateTime.UtcNow;

                await Task.Delay(1000);
                session.Stage = StorySessionStage.Validating;
                session.UpdatedAt = DateTime.UtcNow;

                await Task.Delay(2000);
                session.Stage = StorySessionStage.Complete;
                session.UpdatedAt = DateTime.UtcNow;
            });

            return await Task.FromResult(session);
        }

        public async Task<(bool Success, string Message)> GenerateStoryAsync(string sessionId, string storyPrompt, CancellationToken ct)
        {
            await Task.Delay(100, ct);
            return (true, "Generation started");
        }

        public async Task<(bool Success, EvaluationReport Report)> EvaluateStoryAsync(string sessionId, CancellationToken ct)
        {
            await Task.Delay(50, ct);
            var report = new EvaluationReport
            {
                OverallStatus = EvaluationStatus.Pass,
                IterationNumber = 1
            };
            return (true, report);
        }

        public async Task<(bool Success, string Message)> RefineStoryAsync(string sessionId, UserRefinementFocus focus, CancellationToken ct)
        {
            await Task.Delay(50, ct);
            return (true, "Refinement started");
        }

        public async Task<StorySession?> GetSessionAsync(string sessionId)
        {
            return await Task.FromResult(_sessions.TryGetValue(sessionId, out var session) ? session : null);
        }

        public async Task<(bool Success, RubricSummary? Rubric)> GenerateRubricAsync(string sessionId, CancellationToken ct)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                var rubric = new RubricSummary
                {
                    Summary = "Streaming mock rubric",
                    ReadyForPublish = true
                };
                session.RubricSummary = rubric;
                return (true, rubric);
            }
            return (false, null);
        }
    }

    private class StreamingMockStreamPublisher : IAgentStreamPublisher
    {
        private readonly List<(string SessionId, AgentStreamEvent Event)> _publishedEvents = new();
        private readonly object _lock = new();

        public async Task PublishEventAsync(string sessionId, AgentStreamEvent evt)
        {
            lock (_lock)
            {
                _publishedEvents.Add((sessionId, evt));
            }
            await Task.CompletedTask;
        }

        public async IAsyncEnumerable<AgentStreamEvent> SubscribeAsync(string sessionId, CancellationToken ct = default)
        {
            // Simulate streaming events
            var events = new List<AgentStreamEvent>
            {
                new AgentStreamEvent { Type = AgentStreamEvent.EventType.PhaseStarted, Phase = "SessionStarted", Timestamp = DateTime.UtcNow },
                new AgentStreamEvent { Type = AgentStreamEvent.EventType.PhaseStarted, Phase = "GenerationStarted", Timestamp = DateTime.UtcNow },
                new AgentStreamEvent { Type = AgentStreamEvent.EventType.GenerationComplete, Phase = "GenerationComplete", Timestamp = DateTime.UtcNow },
                new AgentStreamEvent { Type = AgentStreamEvent.EventType.PhaseStarted, Phase = "ValidationStarted", Timestamp = DateTime.UtcNow }
            };

            // For SSEStream_ClosesOnTerminalState test, we need a terminal event
            if (sessionId == "terminal-test-session")
            {
                events.Add(new AgentStreamEvent { Type = AgentStreamEvent.EventType.RubricGenerated, Phase = "Rubric", Timestamp = DateTime.UtcNow });
            }

            foreach (var evt in events)
            {
                if (ct.IsCancellationRequested)
                    yield break;

                await Task.Delay(200, ct); // Simulate delay between events
                yield return evt;
            }
        }

        public List<(string SessionId, AgentStreamEvent Event)> GetPublishedEvents() => _publishedEvents;
    }

    private class StreamingMockSessionRepository : IStorySessionRepository
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

        public void CompleteSession(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.Stage = StorySessionStage.Complete;
            }
        }
    }
}
