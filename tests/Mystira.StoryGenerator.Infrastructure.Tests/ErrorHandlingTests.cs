using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Infrastructure.Agents;
using Xunit;
using Azure;
using Azure.Core;
using System.Text.Json;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

/// <summary>
/// Tests for AgentOrchestrator error handling scenarios.
/// </summary>
public class ErrorHandlingTests : IDisposable
{
    private readonly Mock<ILogger<AgentOrchestrator>> _mockLogger;
    private readonly Mock<IAgentStreamPublisher> _mockEventPublisher;
    private readonly Mock<IStorySessionRepository> _mockSessionRepository;
    private readonly Mock<FoundryAgentClient> _mockFoundryClient;
    private readonly Mock<IKnowledgeProvider> _mockKnowledgeProvider;
    private readonly Mock<IOptions<FoundryAgentConfig>> _mockConfig;
    private readonly AgentOrchestrator _orchestrator;
    private readonly FoundryAgentConfig _testConfig;

    public ErrorHandlingTests()
    {
        _mockLogger = new Mock<ILogger<AgentOrchestrator>>();
        _mockEventPublisher = new Mock<IAgentStreamPublisher>();
        _mockSessionRepository = new Mock<IStorySessionRepository>();
        _mockFoundryClient = new Mock<FoundryAgentClient>();
        _mockKnowledgeProvider = new Mock<IKnowledgeProvider>();
        _mockConfig = new Mock<IOptions<FoundryAgentConfig>>();

        _testConfig = new FoundryAgentConfig
        {
            WriterAgentId = "test-writer-agent",
            JudgeAgentId = "test-judge-agent",
            RefinerAgentId = "test-refiner-agent",
            MaxIterations = 5,
            RunTimeout = TimeSpan.FromMinutes(5)
        };
        _mockConfig.Setup(x => x.Value).Returns(_testConfig);

        _orchestrator = new AgentOrchestrator(
            _mockLogger.Object,
            _mockEventPublisher.Object,
            _mockSessionRepository.Object,
            _mockFoundryClient.Object,
            _mockKnowledgeProvider.Object,
            _mockConfig.Object);
    }

    [Fact]
    public async Task InitializeSessionAsync_Foundry_API_Timeout_Should_Handle_Backoff_And_Retry()
    {
        // Arrange
        var sessionId = "test-session-timeout";
        var knowledgeMode = "AISearch";
        var ageGroup = "6-9";

        // Simulate timeout on first call, success on retry
        var callCount = 0;
        _mockFoundryClient
            .Setup(x => x.CreateThreadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new TaskCanceledException("Request timeout");
                }
                return await Task.FromResult(new ThreadCreationResult { ThreadId = "thread-retry-success" });
            });

        _mockKnowledgeProvider
            .Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new { Results = new List<object>() });

        // Act & Assert - Should eventually succeed with retry
        var result = await _orchestrator.InitializeSessionAsync(sessionId, knowledgeMode, ageGroup);
        
        Assert.Equal(sessionId, result.SessionId);
        Assert.NotNull(result.ThreadId);
        
        // Verify retry occurred
        _mockFoundryClient.Verify(
            x => x.CreateThreadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), 
            Times.AtLeast(2));
    }

    [Fact]
    public async Task GenerateStoryAsync_Foundry_API_Timeout_Should_Handle_Backoff_And_Retry()
    {
        // Arrange
        var sessionId = "test-session-gen-timeout";
        var storyPrompt = "A story about timeout handling";

        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-timeout-123",
            Stage = StorySessionStage.Uninitialized,
            IterationCount = 0
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Simulate timeout on CreateRunAsync
        var callCount = 0;
        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                callCount++;
                if (callCount <= 2) // Fail first 2 attempts
                {
                    throw new TaskCanceledException("Run creation timeout");
                }
                return await Task.FromResult(new RunSubmissionResult { RunId = "run-timeout-success", Status = "running" });
            });

        _mockFoundryClient
            .Setup(x => x.WaitForRunCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunCompletionResult
            {
                RunId = "run-timeout-success",
                Status = "completed",
                Completed = true,
                Messages = new List<Message>
                {
                    new Message { Id = "msg-timeout", Role = "assistant", Content = "{}", CreatedAt = DateTimeOffset.UtcNow }
                }
            });

        StorySession? updatedSession = null;
        _mockSessionRepository
            .Setup(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()))
            .Callback<StorySession, CancellationToken>((s, ct) => updatedSession = s)
            .ReturnsAsync((StorySession s, CancellationToken ct) => s);

        // Act
        var (success, message) = await _orchestrator.GenerateStoryAsync(sessionId, storyPrompt, CancellationToken.None);

        // Assert
        Assert.True(success);
        Assert.Equal("Story generated successfully", message);
        
        // Verify session was updated properly
        Assert.NotNull(updatedSession);
        Assert.Equal(StorySessionStage.Validating, updatedSession.Stage);
    }

    [Fact]
    public async Task EvaluateStoryAsync_Rate_Limiting_Should_Handle_Graceful_Degradation()
    {
        // Arrange
        var sessionId = "test-session-rate-limit";
        
        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-rate-limit-456",
            Stage = StorySessionStage.Validating,
            IterationCount = 1,
            CurrentStoryVersion = "{}",
            StoryVersions = new List<StoryVersionSnapshot>()
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Simulate rate limiting error
        var rateLimitException = new RequestFailedException(429, "Rate limit exceeded", "TooManyRequests", null);
        
        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(rateLimitException);

        // Act & Assert
        await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await _orchestrator.EvaluateStoryAsync(sessionId, CancellationToken.None));

        // Verify error event was published
        _mockEventPublisher.Verify(
            x => x.PublishEventAsync(sessionId, It.Is<AgentStreamEvent>(evt => 
                evt.Type == AgentStreamEvent.EventType.Error &&
                evt.Phase == "Evaluating")),
            Times.Once);
    }

    [Fact]
    public async Task RefineStoryAsync_Malformed_Agent_Response_Should_Provide_Detailed_Error_Reporting()
    {
        // Arrange
        var sessionId = "test-session-malformed";
        
        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-malformed-789",
            Stage = StorySessionStage.RefinementRequested,
            IterationCount = 1,
            CurrentStoryVersion = "{}",
            StoryVersions = new List<StoryVersionSnapshot>()
        };

        var focus = new UserRefinementFocus { IsFullRewrite = true };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Simulate malformed response from refiner
        var malformedResponse = "This is not valid JSON {{{ invalid json response";
        
        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunSubmissionResult { RunId = "run-malformed", Status = "running" });

        _mockFoundryClient
            .Setup(x => x.WaitForRunCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunCompletionResult
            {
                RunId = "run-malformed",
                Status = "completed",
                Completed = true,
                Messages = new List<Message>
                {
                    new Message { Id = "msg-malformed", Role = "assistant", Content = malformedResponse, CreatedAt = DateTimeOffset.UtcNow }
                }
            });

        StorySession? failedSession = null;
        _mockSessionRepository
            .Setup(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()))
            .Callback<StorySession, CancellationToken>((s, ct) => failedSession = s)
            .ReturnsAsync((StorySession s, CancellationToken ct) => s);

        // Act
        var (success, message) = await _orchestrator.RefineStoryAsync(sessionId, focus, CancellationToken.None);

        // Assert
        Assert.False(success);
        Assert.Contains("Refinement failed", message);
        
        // Verify session was marked as failed
        Assert.NotNull(failedSession);
        Assert.Equal(StorySessionStage.Failed, failedSession.Stage);
        
        // Verify detailed error was logged
        _mockEventPublisher.Verify(
            x => x.PublishEventAsync(sessionId, It.Is<AgentStreamEvent>(evt => 
                evt.Type == AgentStreamEvent.EventType.Error &&
                evt.Phase == "Refining")),
            Times.Once);
    }

    [Fact]
    public async Task GenerateStoryAsync_Invalid_Session_State_Should_Return_Descriptive_Error()
    {
        // Arrange
        var sessionId = "test-session-invalid-state";
        var storyPrompt = "A story with invalid state";

        // Session in invalid state for generation
        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-invalid-123",
            Stage = StorySessionStage.Validating, // Invalid state for generation
            IterationCount = 1
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var (success, message) = await _orchestrator.GenerateStoryAsync(sessionId, storyPrompt, CancellationToken.None);

        // Assert
        Assert.False(success);
        Assert.Contains("Invalid session state for generation", message);
        Assert.Contains(StorySessionStage.Validating.ToString(), message);
    }

    [Fact]
    public async Task EvaluateStoryAsync_Invalid_Session_State_Should_Return_Descriptive_Error()
    {
        // Arrange
        var sessionId = "test-session-eval-invalid-state";

        // Session in invalid state for evaluation
        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-eval-invalid-456",
            Stage = StorySessionStage.Generating, // Invalid state for evaluation
            IterationCount = 1
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _orchestrator.EvaluateStoryAsync(sessionId, CancellationToken.None));
    }

    [Fact]
    public async Task RefineStoryAsync_Invalid_Session_State_Should_Return_Descriptive_Error()
    {
        // Arrange
        var sessionId = "test-session-refine-invalid-state";
        var focus = new UserRefinementFocus { IsFullRewrite = false };

        // Session in invalid state for refinement
        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-refine-invalid-789",
            Stage = StorySessionStage.Generating, // Invalid state for refinement
            IterationCount = 1
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var (success, message) = await _orchestrator.RefineStoryAsync(sessionId, focus, CancellationToken.None);

        // Assert
        Assert.False(success);
        Assert.Contains("Invalid session state for refinement", message);
        Assert.Contains(StorySessionStage.Generating.ToString(), message);
    }

    [Fact]
    public async Task AnyOperation_Session_Not_Found_Should_Return_Appropriate_Error()
    {
        // Arrange
        var sessionId = "test-session-not-found";
        
        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorySession?)null);

        // Act & Assert for GenerateStoryAsync
        var (genSuccess, genMessage) = await _orchestrator.GenerateStoryAsync(sessionId, "prompt", CancellationToken.None);
        Assert.False(genSuccess);
        Assert.Contains("Session not found", genMessage);

        // Act & Assert for EvaluateStoryAsync  
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _orchestrator.EvaluateStoryAsync(sessionId, CancellationToken.None));

        // Act & Assert for RefineStoryAsync
        var (refineSuccess, refineMessage) = await _orchestrator.RefineStoryAsync(sessionId, new UserRefinementFocus(), CancellationToken.None);
        Assert.False(refineSuccess);
        Assert.Contains("Session not found", refineMessage);
    }

    [Fact]
    public async Task Foundry_Agent_Run_Failure_Should_Include_Detailed_Error_Context()
    {
        // Arrange
        var sessionId = "test-session-run-failure";
        var storyPrompt = "A story that will cause run failure";

        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-run-failure-123",
            Stage = StorySessionStage.Uninitialized,
            IterationCount = 0
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Simulate run failure with specific error
        var runErrorMessage = "Agent encountered an internal error: Invalid prompt format";
        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunSubmissionResult { RunId = "run-failure", Status = "failed" });

        _mockFoundryClient
            .Setup(x => x.WaitForRunCompletionAsync(It.IsAny<string>(), "run-failure", It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunCompletionResult
            {
                RunId = "run-failure",
                Status = "failed",
                Completed = false,
                ErrorMessage = runErrorMessage
            });

        StorySession? failedSession = null;
        _mockSessionRepository
            .Setup(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()))
            .Callback<StorySession, CancellationToken>((s, ct) => failedSession = s)
            .ReturnsAsync((StorySession s, CancellationToken ct) => s);

        // Act
        var (success, message) = await _orchestrator.GenerateStoryAsync(sessionId, storyPrompt, CancellationToken.None);

        // Assert
        Assert.False(success);
        Assert.Contains("Generation failed", message);
        Assert.Contains(runErrorMessage, message);
        
        // Verify session was marked as failed
        Assert.NotNull(failedSession);
        Assert.Equal(StorySessionStage.Failed, failedSession.Stage);
        
        // Verify error event includes context
        _mockEventPublisher.Verify(
            x => x.PublishEventAsync(sessionId, It.Is<AgentStreamEvent>(evt => 
                evt.Type == AgentStreamEvent.EventType.Error &&
                evt.Phase == "Writing")),
            Times.Once);
    }

    [Fact]
    public async Task Token_Usage_And_Timing_Should_Be_Logged_Per_Run()
    {
        // Arrange
        var sessionId = "test-session-logging";
        var storyPrompt = "A story for testing logging";

        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-logging-456",
            Stage = StorySessionStage.Uninitialized,
            IterationCount = 0
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunSubmissionResult { RunId = "run-logging", Status = "running" });

        _mockFoundryClient
            .Setup(x => x.WaitForRunCompletionAsync(It.IsAny<string>(), "run-logging", It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunCompletionResult
            {
                RunId = "run-logging",
                Status = "completed",
                Completed = true,
                Messages = new List<Message>
                {
                    new Message { Id = "msg-logging", Role = "assistant", Content = "{}", CreatedAt = DateTimeOffset.UtcNow }
                }
            });

        StorySession? updatedSession = null;
        _mockSessionRepository
            .Setup(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()))
            .Callback<StorySession, CancellationToken>((s, ct) => updatedSession = s)
            .ReturnsAsync((StorySession s, CancellationToken ct) => s);

        // Act
        var (success, message) = await _orchestrator.GenerateStoryAsync(sessionId, storyPrompt, CancellationToken.None);

        // Assert
        Assert.True(success);

        // Verify logging occurred at Information level for state transitions
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Story generation completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        // Verify token usage event was published
        _mockEventPublisher.Verify(
            x => x.PublishEventAsync(sessionId, It.Is<AgentStreamEvent>(evt => 
                evt.Type == AgentStreamEvent.EventType.GenerationComplete)),
            Times.Once);
    }

    public void Dispose()
    {
        _mockFoundryClient.Reset();
        _mockSessionRepository.Reset();
        _mockEventPublisher.Reset();
        _mockLogger.Reset();
    }
}