using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Tests;

public class ScenarioEntityConsistencyEvaluationServiceTests
{
    private readonly Mock<IEntityLlmClassificationService> _mockClassifier;
    private readonly Mock<ILogger<ScenarioEntityConsistencyEvaluationService>> _mockLogger;
    private readonly ScenarioEntityConsistencyEvaluationService _service;

    public ScenarioEntityConsistencyEvaluationServiceTests()
    {
        _mockClassifier = new Mock<IEntityLlmClassificationService>();
        _mockLogger = new Mock<ILogger<ScenarioEntityConsistencyEvaluationService>>();

        _service = new ScenarioEntityConsistencyEvaluationService(
            _mockClassifier.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task EvaluateAsync_ClassifiesAllScenesAndReturnsResult()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var entityClassification = new EntityClassification
        {
            TimeDelta = "none",
            IntroducedEntities = new[] { new SceneEntity { Type = SceneEntityType.Character, Name = "TestChar" } },
            RemovedEntities = Array.Empty<SceneEntity>()
        };

        _mockClassifier
            .Setup(c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entityClassification);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Violations);
        Assert.NotNull(result.SceneClassifications);

        // Classifier should be called for each scene
        _mockClassifier.Verify(
            c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeast(scenario.Scenes.Count));
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsNullWhenClassifierReturnsNull()
    {
        // Arrange
        var scenario = CreateTestScenario();

        _mockClassifier
            .Setup(c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityClassification?)null);

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Violations);
        Assert.Empty(result.SceneClassifications);
    }

    [Fact]
    public async Task EvaluateAsync_FindsEntityIntroductionViolations()
    {
        // Arrange
        var scenario = CreateTestScenarioWithViolation();

        var mira = new SceneEntity { Type = SceneEntityType.Character, Name = "Mira" };

        _mockClassifier
            .Setup(c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string content, CancellationToken ct) =>
            {
                if (content.Contains("S0"))
                {
                    return Task.FromResult<EntityClassification?>(new EntityClassification
                    {
                        TimeDelta = "none",
                        IntroducedEntities = new[] { mira },
                        RemovedEntities = Array.Empty<SceneEntity>()
                    });
                }

                return Task.FromResult<EntityClassification?>(new EntityClassification
                {
                    TimeDelta = "none",
                    IntroducedEntities = Array.Empty<SceneEntity>(),
                    RemovedEntities = Array.Empty<SceneEntity>()
                });
            });

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Violations);
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsWhenScenarioIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.EvaluateAsync(null!));
    }

    [Fact]
    public async Task EvaluateAsync_ClassifiesAllScenesInParallel()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var classificationCount = 0;
        var maxConcurrentCalls = 0;
        var currentConcurrentCalls = 0;

        var entityClassification = new EntityClassification
        {
            TimeDelta = "none",
            IntroducedEntities = Array.Empty<SceneEntity>(),
            RemovedEntities = Array.Empty<SceneEntity>()
        };

        _mockClassifier
            .Setup(c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string content, CancellationToken ct) =>
            {
                Interlocked.Increment(ref currentConcurrentCalls);
                Interlocked.Increment(ref classificationCount);

                var current = currentConcurrentCalls;
                if (current > maxConcurrentCalls)
                {
                    maxConcurrentCalls = current;
                }

                await Task.Delay(10, ct);

                Interlocked.Decrement(ref currentConcurrentCalls);
                return entityClassification;
            });

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(scenario.Scenes.Count, classificationCount);
        Assert.True(maxConcurrentCalls > 0, "Classifications should have executed in parallel");
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
            Description = "Mira appears. The story begins.",
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
            Description = "Mira walks with you."
        };

        var s2 = new Scene
        {
            Id = "S2",
            Title = "Right Path",
            Type = SceneType.Narrative,
            Description = "You walk alone."
        };

        return new Scenario
        {
            Id = "TestScenario",
            Title = "Test",
            Scenes = new List<Scene> { s0, s1, s2 }
        };
    }
}
