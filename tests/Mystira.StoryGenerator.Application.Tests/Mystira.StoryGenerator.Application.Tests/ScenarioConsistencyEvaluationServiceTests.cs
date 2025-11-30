using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Tests;

public class ScenarioConsistencyEvaluationServiceTests
{
    private readonly Mock<ILlmConsistencyEvaluator> _mockEvaluator;
    private readonly Mock<IEntityLlmClassificationService> _mockClassifier;
    private readonly Mock<ILogger<ScenarioConsistencyEvaluationService>> _mockLogger;
    private readonly ScenarioConsistencyEvaluationService _service;

    public ScenarioConsistencyEvaluationServiceTests()
    {
        _mockEvaluator = new Mock<ILlmConsistencyEvaluator>();
        _mockClassifier = new Mock<IEntityLlmClassificationService>();
        _mockLogger = new Mock<ILogger<ScenarioConsistencyEvaluationService>>();

        _service = new ScenarioConsistencyEvaluationService(
            _mockEvaluator.Object,
            _mockClassifier.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task EvaluateAsync_ExecutesBothValidationsInParallel()
    {
        // Arrange
        var scenario = CreateTestScenario();
        var graph = ScenarioGraph.FromScenario(scenario);
        var scenarioPathContent = "Test path content";

        var consistencyResult = new ConsistencyEvaluationResult
        {
            OverallAssessment = "ok",
            Issues = new List<ConsistencyIssue>()
        };

        var entityClassification = new EntityClassification
        {
            TimeDelta = "none",
            IntroducedEntities = new[] { new SceneEntity { Type = SceneEntityType.Character, Name = "TestChar" } },
            RemovedEntities = Array.Empty<SceneEntity>(),
            Entities = Array.Empty<SceneEntity>()
        };

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(consistencyResult);

        _mockClassifier
            .Setup(c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entityClassification);

        Func<Scene, IEnumerable<SceneEntity>> getIntroduced = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getRemoved = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getUsed = _ => Array.Empty<SceneEntity>();

        // Act
        var result = await _service.EvaluateAsync(
            graph,
            scenarioPathContent,
            getIntroduced,
            getRemoved,
            getUsed);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.PathConsistencyResult);
        Assert.NotNull(result.EntityIntroductionResult);
        Assert.Equal("ok", result.PathConsistencyResult.OverallAssessment);

        // Verify both evaluators were called
        _mockEvaluator.Verify(
            e => e.EvaluateConsistencyAsync(scenarioPathContent, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockClassifier.Verify(
            c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsSuccessfulResultWhenBothEvaluationsReturnNull()
    {
        // Arrange
        var scenario = CreateTestScenario();
        var graph = ScenarioGraph.FromScenario(scenario);
        var scenarioPathContent = "Test path content";

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsistencyEvaluationResult?)null);

        _mockClassifier
            .Setup(c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityClassification?)null);

        Func<Scene, IEnumerable<SceneEntity>> getIntroduced = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getRemoved = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getUsed = _ => Array.Empty<SceneEntity>();

        // Act
        var result = await _service.EvaluateAsync(
            graph,
            scenarioPathContent,
            getIntroduced,
            getRemoved,
            getUsed);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccessful);
        Assert.Null(result.PathConsistencyResult);
        Assert.Null(result.EntityIntroductionResult);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsPartialResultWhenOneEvaluationFails()
    {
        // Arrange
        var scenario = CreateTestScenario();
        var graph = ScenarioGraph.FromScenario(scenario);
        var scenarioPathContent = "Test path content";

        var consistencyResult = new ConsistencyEvaluationResult
        {
            OverallAssessment = "has_minor_issues",
            Issues = new List<ConsistencyIssue>()
        };

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(consistencyResult);

        _mockClassifier
            .Setup(c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityClassification?)null);

        Func<Scene, IEnumerable<SceneEntity>> getIntroduced = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getRemoved = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getUsed = _ => Array.Empty<SceneEntity>();

        // Act
        var result = await _service.EvaluateAsync(
            graph,
            scenarioPathContent,
            getIntroduced,
            getRemoved,
            getUsed);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.PathConsistencyResult);
        Assert.Null(result.EntityIntroductionResult);
        Assert.Equal("has_minor_issues", result.PathConsistencyResult.OverallAssessment);
    }

    [Fact]
    public async Task EvaluateAsync_FindsEntityIntroductionViolations()
    {
        // Arrange
        var scenario = CreateTestScenarioWithViolation();
        var graph = ScenarioGraph.FromScenario(scenario);
        var scenarioPathContent = "Test path content";

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsistencyEvaluationResult?)null);

        _mockClassifier
            .Setup(c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityClassification?)null);

        var mira = new SceneEntity { Type = SceneEntityType.Character, Name = "Mira" };
        var oldRurik = new SceneEntity { Type = SceneEntityType.Character, Name = "Old Rurik" };

        Func<Scene, IEnumerable<SceneEntity>> getIntroduced = scene =>
            scene.Id == "S0" ? new[] { mira } : Array.Empty<SceneEntity>();

        Func<Scene, IEnumerable<SceneEntity>> getRemoved = _ => Array.Empty<SceneEntity>();

        Func<Scene, IEnumerable<SceneEntity>> getUsed = scene =>
            scene.Id switch
            {
                "S1" => new[] { mira },
                "S2" => new[] { oldRurik },
                _ => Array.Empty<SceneEntity>()
            };

        // Act
        var result = await _service.EvaluateAsync(
            graph,
            scenarioPathContent,
            getIntroduced,
            getRemoved,
            getUsed);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EntityIntroductionResult);
        Assert.Single(result.EntityIntroductionResult.Violations);
        Assert.Equal("S2", result.EntityIntroductionResult.Violations[0].SceneId);
        Assert.Equal("Old Rurik", result.EntityIntroductionResult.Violations[0].Entity.Name);
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsWhenGraphIsNull()
    {
        // Arrange
        var scenarioPathContent = "Test path content";

        Func<Scene, IEnumerable<SceneEntity>> getIntroduced = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getRemoved = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getUsed = _ => Array.Empty<SceneEntity>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.EvaluateAsync(
                null!,
                scenarioPathContent,
                getIntroduced,
                getRemoved,
                getUsed));
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsWhenScenarioPathContentIsNull()
    {
        // Arrange
        var scenario = CreateTestScenario();
        var graph = ScenarioGraph.FromScenario(scenario);

        Func<Scene, IEnumerable<SceneEntity>> getIntroduced = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getRemoved = _ => Array.Empty<SceneEntity>();
        Func<Scene, IEnumerable<SceneEntity>> getUsed = _ => Array.Empty<SceneEntity>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.EvaluateAsync(
                graph,
                null!,
                getIntroduced,
                getRemoved,
                getUsed));
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

    private static Scenario CreateTestScenarioWithViolation()
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
            Description = "Mentions Mira who was introduced."
        };

        var s2 = new Scene
        {
            Id = "S2",
            Title = "Right Path",
            Type = SceneType.Narrative,
            Description = "Mentions Old Rurik who was not introduced."
        };

        return new Scenario
        {
            Id = "TestScenario",
            Title = "Test",
            Scenes = new List<Scene> { s0, s1, s2 }
        };
    }
}
