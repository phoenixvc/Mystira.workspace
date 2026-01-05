using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Infrastructure.Agents;
using Xunit;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

/// <summary>
/// Integration tests for the AgentOrchestrator using mocked Foundry SDK responses.
/// </summary>
public class AgentOrchestratorIntegrationTests : IDisposable
{
    private readonly Mock<ILogger<AgentOrchestrator>> _mockLogger;
    private readonly Mock<IAgentStreamPublisher> _mockEventPublisher;
    private readonly Mock<IStorySessionRepository> _mockSessionRepository;
    private readonly Mock<FoundryAgentClient> _mockFoundryClient;
    private readonly Mock<IKnowledgeProvider> _mockKnowledgeProvider;
    private readonly Mock<IOptions<FoundryAgentConfig>> _mockConfig;
    private readonly AgentOrchestrator _orchestrator;
    private readonly FoundryAgentConfig _testConfig;

    public AgentOrchestratorIntegrationTests()
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
    public async Task InitializeSessionAsync_Should_Create_Valid_Foundry_Thread_And_Persist_Session()
    {
        // Arrange
        var sessionId = "test-session-123";
        var knowledgeMode = "AISearch";
        var ageGroup = "6-9";
        var expectedThreadId = "thread-abc-123";

        _mockFoundryClient
            .Setup(x => x.CreateThreadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ThreadCreationResult { ThreadId = expectedThreadId });

        _mockKnowledgeProvider
            .Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new { Results = new List<object>() });

        StorySession? savedSession = null;
        _mockSessionRepository
            .Setup(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()))
            .Callback<StorySession, CancellationToken>((session, ct) => savedSession = session)
            .ReturnsAsync((StorySession session, CancellationToken ct) => session);

        // Act
        var result = await _orchestrator.InitializeSessionAsync(sessionId, knowledgeMode, ageGroup);

        // Assert
        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal(Enum.Parse<KnowledgeMode>(knowledgeMode), result.KnowledgeMode);
        Assert.Equal(StorySessionStage.Uninitialized, result.Stage);
        Assert.Equal(expectedThreadId, result.ThreadId);
        Assert.Equal(0, result.IterationCount);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        Assert.True(result.UpdatedAt <= DateTime.UtcNow);

        _mockFoundryClient.Verify(
            x => x.CreateThreadAsync(_testConfig.WriterAgentId, It.IsAny<CancellationToken>()), 
            Times.Once);
        _mockSessionRepository.Verify(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(
            x => x.PublishEventAsync(sessionId, It.IsAny<AgentStreamEvent>()), Times.Once);
    }

    [Fact]
    public async Task GenerateStoryAsync_Produces_Valid_Schema_Compliant_JSON_Story()
    {
        // Arrange
        var sessionId = "test-session-456";
        var storyPrompt = "A story about a brave little mouse";
        
        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-xyz-789",
            Stage = StorySessionStage.Uninitialized,
            IterationCount = 0,
            CurrentStoryVersion = "",
            StoryVersions = new List<StoryVersionSnapshot>()
        };

        var validStoryJson = """
        {
            "title": "The Brave Little Mouse",
            "description": "A tale of courage",
            "image": "mouse-hero.jpg",
            "tags": ["adventure", "courage"],
            "difficulty": "Easy",
            "session_length": "Short",
            "age_group": "6-9",
            "minimum_age": 6,
            "core_axes": ["bravery", "friendship"],
            "archetypes": ["hero", "mentor"],
            "characters": [
                {
                    "id": "little_mouse",
                    "name": "Cheese",
                    "image": "mouse.jpg",
                    "audio": null,
                    "metadata": {
                        "role": ["protagonist"],
                        "archetype": ["hero"],
                        "species": "mouse",
                        "age": 3,
                        "traits": ["brave", "curious"],
                        "backstory": "A small mouse with a big heart"
                    }
                }
            ],
            "scenes": [
                {
                    "id": "introduction",
                    "title": "Meet Cheese",
                    "type": "narrative",
                    "active_character": "little_mouse",
                    "description": "Cheese lives in a cozy hole in the wall",
                    "next_scene": "challenge_appears",
                    "difficulty": null,
                    "media": {
                        "image": "mouse_hole.jpg",
                        "audio": null,
                        "video": null
                    },
                    "branches": [],
                    "echo_reveals": []
                }
            ]
        }
        """;

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunSubmissionResult { RunId = "run-123", Status = "running" });

        _mockFoundryClient
            .Setup(x => x.WaitForRunCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunCompletionResult
            {
                RunId = "run-123",
                Status = "completed",
                Completed = true,
                Messages = new List<Message>
                {
                    new Message { Id = "msg-1", Role = "assistant", Content = validStoryJson, CreatedAt = DateTimeOffset.UtcNow }
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
        
        Assert.NotNull(updatedSession);
        Assert.Equal(StorySessionStage.Validating, updatedSession.Stage);
        Assert.Equal(1, updatedSession.IterationCount);
        Assert.Equal(validStoryJson, updatedSession.CurrentStoryVersion);
        Assert.Single(updatedSession.StoryVersions);
        Assert.Equal(1, updatedSession.StoryVersions[0].VersionNumber);
        Assert.Equal("Generating", updatedSession.StoryVersions[0].StageWhenCreated);

        _mockFoundryClient.Verify(
            x => x.CreateRunAsync(session.ThreadId, _testConfig.WriterAgentId, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateStoryAsync_Deterministic_Gates_Catch_Schema_Safety_Violations()
    {
        // Arrange
        var sessionId = "test-session-789";
        var invalidStoryJson = "{ invalid json }";
        
        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-invalid-123",
            Stage = StorySessionStage.Validating,
            IterationCount = 1,
            CurrentStoryVersion = invalidStoryJson,
            StoryVersions = new List<StoryVersionSnapshot>
            {
                new StoryVersionSnapshot
                {
                    VersionNumber = 1,
                    StoryJson = invalidStoryJson,
                    StageWhenCreated = "Generating",
                    IterationNumber = 1
                }
            }
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var (success, report) = await _orchestrator.EvaluateStoryAsync(sessionId, CancellationToken.None);

        // Assert
        Assert.False(success);
        Assert.NotNull(report);
        Assert.Equal(EvaluationStatus.Fail, report.OverallStatus);
        Assert.False(report.SafetyGatePassed);
        Assert.True(report.Findings.ContainsKey("validation"));
        Assert.Contains("Story failed schema validation", report.Findings["validation"]);

        // Verify session was updated to require refinement
        _mockSessionRepository.Verify(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RefineStoryAsync_With_UserFocus_TargetSceneIds_Preserves_Other_Scenes()
    {
        // Arrange
        var sessionId = "test-session-refine";
        var originalStory = """
        {
            "title": "Original Story",
            "description": "Original description",
            "image": "cover.jpg",
            "tags": ["original"],
            "difficulty": "Easy",
            "session_length": "Short", 
            "age_group": "6-9",
            "minimum_age": 6,
            "core_axes": ["test"],
            "archetypes": ["hero"],
            "characters": [
                {
                    "id": "hero",
                    "name": "Hero",
                    "image": "hero.jpg",
                    "audio": null,
                    "metadata": {
                        "role": ["protagonist"],
                        "archetype": ["hero"],
                        "species": "human",
                        "age": 10,
                        "traits": ["brave"],
                        "backstory": "A brave hero"
                    }
                }
            ],
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "Scene 1",
                    "type": "narrative",
                    "active_character": "hero",
                    "description": "Scene 1 description - should be preserved",
                    "next_scene": "scene_2",
                    "difficulty": null,
                    "media": {"image": null, "audio": null, "video": null},
                    "branches": [],
                    "echo_reveals": []
                },
                {
                    "id": "scene_2",
                    "title": "Scene 2 - Target",
                    "type": "narrative", 
                    "active_character": "hero",
                    "description": "Scene 2 description - should be modified",
                    "next_scene": null,
                    "difficulty": null,
                    "media": {"image": null, "audio": null, "video": null},
                    "branches": [],
                    "echo_reveals": []
                }
            ]
        }
        """;

        var refinedStory = """
        {
            "title": "Original Story",
            "description": "Original description",
            "image": "cover.jpg",
            "tags": ["original"],
            "difficulty": "Easy",
            "session_length": "Short",
            "age_group": "6-9", 
            "minimum_age": 6,
            "core_axes": ["test"],
            "archetypes": ["hero"],
            "characters": [
                {
                    "id": "hero",
                    "name": "Hero",
                    "image": "hero.jpg",
                    "audio": null,
                    "metadata": {
                        "role": ["protagonist"],
                        "archetype": ["hero"],
                        "species": "human",
                        "age": 10,
                        "traits": ["brave"],
                        "backstory": "A brave hero"
                    }
                }
            ],
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "Scene 1",
                    "type": "narrative",
                    "active_character": "hero",
                    "description": "Scene 1 description - should be preserved",
                    "next_scene": "scene_2",
                    "difficulty": null,
                    "media": {"image": null, "audio": null, "video": null},
                    "branches": [],
                    "echo_reveals": []
                },
                {
                    "id": "scene_2",
                    "title": "Scene 2 - Target",
                    "type": "narrative",
                    "active_character": "hero", 
                    "description": "Scene 2 description - refined and improved",
                    "next_scene": null,
                    "difficulty": null,
                    "media": {"image": null, "audio": null, "video": null},
                    "branches": [],
                    "echo_reveals": []
                }
            ]
        }
        """;

        var focus = new UserRefinementFocus
        {
            TargetSceneIds = new List<string> { "scene_2" },
            Aspects = new List<string> { "description", "dialogue" },
            IsFullRewrite = false
        };

        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-refine-456",
            Stage = StorySessionStage.RefinementRequested,
            IterationCount = 1,
            CurrentStoryVersion = originalStory,
            StoryVersions = new List<StoryVersionSnapshot>
            {
                new StoryVersionSnapshot
                {
                    VersionNumber = 1,
                    StoryJson = originalStory,
                    StageWhenCreated = "Generating",
                    IterationNumber = 1
                }
            },
            LastEvaluationReport = new EvaluationReport
            {
                IterationNumber = 1,
                OverallStatus = EvaluationStatus.Fail,
                Findings = new Dictionary<string, List<string>> { { "general", new List<string> { "Improve scene descriptions" } } }
            }
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunSubmissionResult { RunId = "run-refine-123", Status = "running" });

        _mockFoundryClient
            .Setup(x => x.WaitForRunCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunCompletionResult
            {
                RunId = "run-refine-123",
                Status = "completed",
                Completed = true,
                Messages = new List<Message>
                {
                    new Message { Id = "msg-refine", Role = "assistant", Content = refinedStory, CreatedAt = DateTimeOffset.UtcNow }
                }
            });

        StorySession? updatedSession = null;
        _mockSessionRepository
            .Setup(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()))
            .Callback<StorySession, CancellationToken>((s, ct) => updatedSession = s)
            .ReturnsAsync((StorySession s, CancellationToken ct) => s);

        // Act
        var (success, message) = await _orchestrator.RefineStoryAsync(sessionId, focus, CancellationToken.None);

        // Assert
        Assert.True(success);
        Assert.Equal("Story refined successfully; re-evaluation required", message);
        
        Assert.NotNull(updatedSession);
        Assert.Equal(StorySessionStage.Validating, updatedSession.Stage); // Should go back to validating
        Assert.Equal(2, updatedSession.IterationCount);
        Assert.Equal(focus, updatedSession.UserFocus);
        Assert.Equal(2, updatedSession.StoryVersions.Count);
        
        var refinedVersion = updatedSession.StoryVersions[1];
        Assert.Equal(2, refinedVersion.VersionNumber);
        Assert.Equal("Refining", refinedVersion.StageWhenCreated);
        Assert.Equal(2, refinedVersion.IterationNumber);

        // Verify refiner was called with targeted instructions
        _mockFoundryClient.Verify(
            x => x.CreateRunAsync(session.ThreadId, _testConfig.RefinerAgentId, It.Is<string>(prompt => 
                prompt.Contains("ONLY edit scenes: scene_2") && 
                prompt.Contains("ONLY modify aspects: description, dialogue")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Loop_Correctly_Re_Evaluates_After_Refinement_Until_Pass_Or_Max_Iterations()
    {
        // Arrange
        var sessionId = "test-session-loop";
        
        var initialStory = """
        {
            "title": "Test Story",
            "description": "Test description",
            "image": "test.jpg",
            "tags": ["test"],
            "difficulty": "Easy",
            "session_length": "Short",
            "age_group": "6-9",
            "minimum_age": 6,
            "core_axes": ["test"],
            "archetypes": ["hero"],
            "characters": [
                {
                    "id": "hero",
                    "name": "Hero",
                    "image": "hero.jpg",
                    "audio": null,
                    "metadata": {
                        "role": ["protagonist"],
                        "archetype": ["hero"],
                        "species": "human",
                        "age": 10,
                        "traits": ["brave"],
                        "backstory": "A brave hero"
                    }
                }
            ],
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "Scene 1",
                    "type": "narrative",
                    "active_character": "hero",
                    "description": "A simple scene",
                    "next_scene": null,
                    "difficulty": null,
                    "media": {"image": null, "audio": null, "video": null},
                    "branches": [],
                    "echo_reveals": []
                }
            ]
        }
        """;

        var failingEvaluationReport = """
        {
            "iteration_number": 1,
            "overall_status": "Fail",
            "safety_gate_passed": true,
            "axes_alignment_score": 0.3,
            "dev_principles_score": 0.4,
            "narrative_logic_score": 0.2,
            "findings": {
                "narrative_logic": ["Story lacks emotional depth", "Character development insufficient"]
            },
            "recommendation": "Enhance character emotions and develop relationships",
            "token_usage": 1500
        }
        """;

        var passingEvaluationReport = """
        {
            "iteration_number": 2,
            "overall_status": "Pass", 
            "safety_gate_passed": true,
            "axes_alignment_score": 0.9,
            "dev_principles_score": 0.8,
            "narrative_logic_score": 0.9,
            "findings": {},
            "recommendation": "Story meets all quality standards",
            "token_usage": 1200
        }
        """;

        var refinedStory = """
        {
            "title": "Test Story",
            "description": "Enhanced test description with more depth",
            "image": "test.jpg",
            "tags": ["test", "enhanced"],
            "difficulty": "Easy",
            "session_length": "Short",
            "age_group": "6-9",
            "minimum_age": 6,
            "core_axes": ["test", "emotion"],
            "archetypes": ["hero"],
            "characters": [
                {
                    "id": "hero",
                    "name": "Hero",
                    "image": "hero.jpg",
                    "audio": null,
                    "metadata": {
                        "role": ["protagonist"],
                        "archetype": ["hero"],
                        "species": "human",
                        "age": 10,
                        "traits": ["brave", "empathetic"],
                        "backstory": "A brave hero who learns about friendship and courage"
                    }
                }
            ],
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "Scene 1",
                    "type": "narrative",
                    "active_character": "hero",
                    "description": "A simple scene where the hero discovers their inner strength and learns the importance of friendship",
                    "next_scene": null,
                    "difficulty": null,
                    "media": {"image": null, "audio": null, "video": null},
                    "branches": [],
                    "echo_reveals": []
                }
            ]
        }
        """;

        var focus = new UserRefinementFocus
        {
            Aspects = new List<string> { "emotional_depth", "character_development" },
            IsFullRewrite = false
        };

        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-loop-789",
            Stage = StorySessionStage.RefinementRequested,
            IterationCount = 1,
            CurrentStoryVersion = initialStory,
            StoryVersions = new List<StoryVersionSnapshot>
            {
                new StoryVersionSnapshot
                {
                    VersionNumber = 1,
                    StoryJson = initialStory,
                    StageWhenCreated = "Generating",
                    IterationNumber = 1
                }
            },
            LastEvaluationReport = JsonSerializer.Deserialize<EvaluationReport>(failingEvaluationReport)!
        };

        var callCount = 0;
        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunSubmissionResult { RunId = $"run-{callCount}", Status = "running" });

        _mockFoundryClient
            .Setup(x => x.WaitForRunCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string threadId, string runId, TimeSpan? pollInterval, TimeSpan? maxWait, CancellationToken ct) =>
            {
                callCount++;
                var content = callCount switch
                {
                    1 => refinedStory, // Refiner response
                    2 => passingEvaluationReport, // Judge response  
                    _ => "{}"
                };

                return Task.FromResult(new RunCompletionResult
                {
                    RunId = runId,
                    Status = "completed",
                    Completed = true,
                    Messages = new List<Message>
                    {
                        new Message { Id = $"msg-{callCount}", Role = "assistant", Content = content, CreatedAt = DateTimeOffset.UtcNow }
                    }
                });
            });

        StorySession? finalSession = null;
        _mockSessionRepository
            .Setup(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()))
            .Callback<StorySession, CancellationToken>((s, ct) => finalSession = s)
            .ReturnsAsync((StorySession s, CancellationToken ct) => s);

        // Act - Refine first
        var (refineSuccess, refineMessage) = await _orchestrator.RefineStoryAsync(sessionId, focus, CancellationToken.None);

        // Then Evaluate
        var (evalSuccess, evalReport) = await _orchestrator.EvaluateStoryAsync(sessionId, CancellationToken.None);

        // Assert
        Assert.True(refineSuccess);
        Assert.True(evalSuccess);
        Assert.Equal(EvaluationStatus.Pass, evalReport.OverallStatus);
        
        Assert.NotNull(finalSession);
        Assert.Equal(StorySessionStage.Evaluated, finalSession.Stage); // Should be evaluated (pass)
        Assert.Equal(2, finalSession.IterationCount); // Iteration count should be incremented
        Assert.Equal(2, finalSession.StoryVersions.Count); // Should have original + refined version
    }

    [Fact]
    public async Task Max_Iteration_Limit_Reached_Should_Set_State_StuckNeedsReview()
    {
        // Arrange
        var sessionId = "test-session-max-iterations";
        var maxIterations = 3; // Set low for testing

        var testConfig = new FoundryAgentConfig
        {
            WriterAgentId = "test-writer-agent",
            JudgeAgentId = "test-judge-agent",
            RefinerAgentId = "test-refiner-agent",
            MaxIterations = maxIterations,
            RunTimeout = TimeSpan.FromMinutes(5)
        };
        _mockConfig.Setup(x => x.Value).Returns(testConfig);

        var focus = new UserRefinementFocus { IsFullRewrite = false };

        var session = new StorySession
        {
            SessionId = sessionId,
            ThreadId = "thread-max-iter",
            Stage = StorySessionStage.RefinementRequested,
            IterationCount = maxIterations - 1, // One away from max
            CurrentStoryVersion = "{}",
            StoryVersions = new List<StoryVersionSnapshot>()
        };

        _mockSessionRepository
            .Setup(x => x.GetAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _mockFoundryClient
            .Setup(x => x.CreateRunAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunSubmissionResult { RunId = "run-max", Status = "running" });

        _mockFoundryClient
            .Setup(x => x.WaitForRunCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RunCompletionResult
            {
                RunId = "run-max",
                Status = "completed", 
                Completed = true,
                Messages = new List<Message>
                {
                    new Message { Id = "msg-max", Role = "assistant", Content = "{}", CreatedAt = DateTimeOffset.UtcNow }
                }
            });

        StorySession? finalSession = null;
        _mockSessionRepository
            .Setup(x => x.UpdateAsync(It.IsAny<StorySession>(), It.IsAny<CancellationToken>()))
            .Callback<StorySession, CancellationToken>((s, ct) => finalSession = s)
            .ReturnsAsync((StorySession s, CancellationToken ct) => s);

        // Act
        var (success, message) = await _orchestrator.RefineStoryAsync(sessionId, focus, CancellationToken.None);

        // Assert
        Assert.False(success);
        Assert.Contains($"Maximum iterations ({maxIterations}) reached", message);
        
        Assert.NotNull(finalSession);
        Assert.Equal(StorySessionStage.StuckNeedsReview, finalSession.Stage);
        Assert.Equal(maxIterations, finalSession.IterationCount);

        // Verify max iterations event was published
        _mockEventPublisher.Verify(
            x => x.PublishEventAsync(sessionId, It.Is<AgentStreamEvent>(evt => 
                evt.Type == AgentStreamEvent.EventType.MaxIterationsReached)),
            Times.Once);
    }

    public void Dispose()
    {
        _mockFoundryClient.Reset();
        _mockSessionRepository.Reset();
        _mockEventPublisher.Reset();
    }
}