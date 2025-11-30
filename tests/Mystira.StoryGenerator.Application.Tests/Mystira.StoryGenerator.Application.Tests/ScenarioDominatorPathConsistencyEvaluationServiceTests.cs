using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Tests;

public class ScenarioDominatorPathConsistencyEvaluationServiceTests
{
    private readonly Mock<ILlmConsistencyEvaluator> _mockEvaluator;
    private readonly Mock<ILogger<ScenarioDominatorPathConsistencyEvaluationService>> _mockLogger;
    private readonly ScenarioDominatorPathConsistencyEvaluationService _service;

    public ScenarioDominatorPathConsistencyEvaluationServiceTests()
    {
        _mockEvaluator = new Mock<ILlmConsistencyEvaluator>();
        _mockLogger = new Mock<ILogger<ScenarioDominatorPathConsistencyEvaluationService>>();

        _service = new ScenarioDominatorPathConsistencyEvaluationService(
            _mockEvaluator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task EvaluateAsync_GeneratesCompressedPathsAndEvaluates()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var consistencyResult = new ConsistencyEvaluationResult
        {
            OverallAssessment = "ok",
            Issues = new List<ConsistencyIssue>()
        };

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(consistencyResult);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ok", result.OverallAssessment);
        Assert.Empty(result.Issues);

        // Verify evaluator was called with serialized paths
        _mockEvaluator.Verify(
            e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsNullWhenEvaluatorReturnsNull()
    {
        // Arrange
        var scenario = CreateTestScenario();

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsistencyEvaluationResult?)null);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EvaluateAsync_PassesSerializedPathsToEvaluator()
    {
        // Arrange
        var scenario = CreateTestScenario();

        string capturedContent = string.Empty;

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback((string content, CancellationToken ct) => capturedContent = content)
            .ReturnsAsync(new ConsistencyEvaluationResult { OverallAssessment = "ok", Issues = new List<ConsistencyIssue>() });

        // Act
        await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotEmpty(capturedContent);
        Assert.Contains("Path", capturedContent);
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsWhenScenarioIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.EvaluateAsync(null!));
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsResultWithIssuesWhenEvaluatorFindsProblems()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var consistencyResult = new ConsistencyEvaluationResult
        {
            OverallAssessment = "has_major_issues",
            Issues = new List<ConsistencyIssue>
            {
                new ConsistencyIssue
                {
                    Id = "issue1",
                    Severity = "high",
                    Category = "entity_consistency",
                    SceneIds = new List<string> { "S1", "S2" },
                    Summary = "Entity state inconsistency",
                    Details = "Character X has conflicting states in different paths"
                }
            }
        };

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(consistencyResult);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("has_major_issues", result.OverallAssessment);
        Assert.Single(result.Issues);
        Assert.Equal("entity_consistency", result.Issues[0].Category);
    }

    private static Scenario CreateTestScenario()
    {
        var s0 = new Scene
        {
            Id = "S0",
            Title = "Start",
            Type = SceneType.Choice,
            Description = "The story begins.",
            Branches = new List<Branch>
            {
                new Branch { Choice = "Go left", NextSceneId = "S1" },
                new Branch { Choice = "Go right", NextSceneId = "S2" }
            }
        };

        var s1 = new Scene
        {
            Id = "S1",
            Title = "Left Path",
            Type = SceneType.Narrative,
            Description = "You went left."
        };

        var s2 = new Scene
        {
            Id = "S2",
            Title = "Right Path",
            Type = SceneType.Narrative,
            Description = "You went right."
        };

        return new Scenario
        {
            Id = "TestScenario",
            Title = "Test",
            Scenes = new List<Scene> { s0, s1, s2 }
        };
    }
}
