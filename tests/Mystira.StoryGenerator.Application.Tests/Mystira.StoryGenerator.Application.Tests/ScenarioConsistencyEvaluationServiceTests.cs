using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Tests;

public class ScenarioConsistencyEvaluationServiceTests
{
    private readonly Mock<IScenarioEntityConsistencyEvaluationService> _mockEntityService;
    private readonly Mock<IScenarioDominatorPathConsistencyEvaluationService> _mockPathService;
    private readonly Mock<ILogger<ScenarioConsistencyEvaluationService>> _mockLogger;
    private readonly ScenarioConsistencyEvaluationService _service;

    public ScenarioConsistencyEvaluationServiceTests()
    {
        _mockEntityService = new Mock<IScenarioEntityConsistencyEvaluationService>();
        _mockPathService = new Mock<IScenarioDominatorPathConsistencyEvaluationService>();
        _mockLogger = new Mock<ILogger<ScenarioConsistencyEvaluationService>>();

        _service = new ScenarioConsistencyEvaluationService(
            _mockEntityService.Object,
            _mockPathService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task EvaluateAsync_ExecutesBothEvaluationsInParallel()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var entityResult = new EntityIntroductionEvaluationResult(
            new List<ScenarioEntityIntroductionValidator.SceneReferenceViolation>(),
            new Dictionary<string, SceneEntityClassificationData>());

        var pathResults = new ConsistencyEvaluationResults(
            new List<PathConsistencyEvaluationResult>
            {
                new PathConsistencyEvaluationResult(
                    new List<string> { "S0", "S1" },
                    new ConsistencyEvaluationResult { OverallAssessment = "ok", Issues = new List<ConsistencyIssue>() })
            });

        _mockEntityService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entityResult);

        _mockPathService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pathResults);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.PathConsistencyResults);
        Assert.NotNull(result.EntityIntroductionResult);

        // Verify both services were called
        _mockEntityService.Verify(
            s => s.EvaluateAsync(scenario, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockPathService.Verify(
            s => s.EvaluateAsync(scenario, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsSuccessfulResultWhenBothEvaluationsComplete()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var entityResult = new EntityIntroductionEvaluationResult(
            new List<ScenarioEntityIntroductionValidator.SceneReferenceViolation>(),
            new Dictionary<string, SceneEntityClassificationData>());

        var pathResults = new ConsistencyEvaluationResults(
            new List<PathConsistencyEvaluationResult>
            {
                new PathConsistencyEvaluationResult(
                    new List<string> { "S0", "S1" },
                    new ConsistencyEvaluationResult { OverallAssessment = "ok", Issues = new List<ConsistencyIssue>() })
            });

        _mockEntityService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entityResult);

        _mockPathService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pathResults);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.PathConsistencyResults);
        Assert.Single(result.PathConsistencyResults.PathResults);
        Assert.Equal("ok", result.PathConsistencyResults.PathResults[0].Result?.OverallAssessment);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsSuccessfulResultWhenOneEvaluationReturnsNull()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var entityResult = new EntityIntroductionEvaluationResult(
            new List<ScenarioEntityIntroductionValidator.SceneReferenceViolation>(),
            new Dictionary<string, SceneEntityClassificationData>());

        _mockEntityService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entityResult);

        _mockPathService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsistencyEvaluationResults?)null);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Null(result.PathConsistencyResults);
        Assert.NotNull(result.EntityIntroductionResult);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsUnsuccessfulResultWhenBothEvaluationsReturnNull()
    {
        // Arrange
        var scenario = CreateTestScenario();

        _mockEntityService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityIntroductionEvaluationResult?)null);

        _mockPathService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsistencyEvaluationResults?)null);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Null(result.PathConsistencyResults);
        Assert.Null(result.EntityIntroductionResult);
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsWhenScenarioIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.EvaluateAsync(null!));
    }

    [Fact]
    public async Task EvaluateAsync_BothServicesReceiveScenarioParameter()
    {
        // Arrange
        var scenario = CreateTestScenario();

        _mockEntityService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityIntroductionEvaluationResult(
                new List<ScenarioEntityIntroductionValidator.SceneReferenceViolation>(),
                new Dictionary<string, SceneEntityClassificationData>()));

        _mockPathService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsistencyEvaluationResults(new List<PathConsistencyEvaluationResult>()));

        // Act
        await _service.EvaluateAsync(scenario);

        // Assert
        _mockEntityService.Verify(
            s => s.EvaluateAsync(
                It.Is<Scenario>(sc => sc.Id == scenario.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockPathService.Verify(
            s => s.EvaluateAsync(
                It.Is<Scenario>(sc => sc.Id == scenario.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnedPathResultsContainSceneIds()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var pathResults = new ConsistencyEvaluationResults(
            new List<PathConsistencyEvaluationResult>
            {
                new PathConsistencyEvaluationResult(
                    new List<string> { "S0", "S1" },
                    new ConsistencyEvaluationResult { OverallAssessment = "ok", Issues = new() }),
                new PathConsistencyEvaluationResult(
                    new List<string> { "S0", "S2" },
                    new ConsistencyEvaluationResult { OverallAssessment = "ok", Issues = new() })
            });

        var entityResult = new EntityIntroductionEvaluationResult(
            new List<ScenarioEntityIntroductionValidator.SceneReferenceViolation>(),
            new Dictionary<string, SceneEntityClassificationData>());

        _mockEntityService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entityResult);

        _mockPathService
            .Setup(s => s.EvaluateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pathResults);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result.PathConsistencyResults);
        Assert.Equal(2, result.PathConsistencyResults.PathResults.Count);
        Assert.Equal(new[] { "S0", "S1" }, result.PathConsistencyResults.PathResults[0].SceneIds);
        Assert.Equal(new[] { "S0", "S2" }, result.PathConsistencyResults.PathResults[1].SceneIds);
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
