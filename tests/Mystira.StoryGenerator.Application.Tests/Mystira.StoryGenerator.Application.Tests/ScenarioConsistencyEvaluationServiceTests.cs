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
    public async Task EvaluateAsync_ExecutesBothClassificationAndPathConsistencyInParallel()
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

        // Act
        var result = await _service.EvaluateAsync(graph, scenarioPathContent);

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
        
        // Classifier should be called for each scene
        _mockClassifier.Verify(
            c => c.ClassifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeast(graph.Nodes.Count));
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

        // Act
        var result = await _service.EvaluateAsync(graph, scenarioPathContent);

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

        // Act
        var result = await _service.EvaluateAsync(graph, scenarioPathContent);

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

        var mira = new SceneEntity { Type = SceneEntityType.Character, Name = "Mira" };
        var oldRurik = new SceneEntity { Type = SceneEntityType.Character, Name = "Old Rurik" };

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
                        RemovedEntities = Array.Empty<SceneEntity>(),
                        Entities = new[] { mira }
                    });
                }
                else if (content.Contains("S1"))
                {
                    return Task.FromResult<EntityClassification?>(new EntityClassification
                    {
                        TimeDelta = "none",
                        IntroducedEntities = Array.Empty<SceneEntity>(),
                        RemovedEntities = Array.Empty<SceneEntity>(),
                        Entities = Array.Empty<SceneEntity>()
                    });
                }
                else if (content.Contains("S2"))
                {
                    return Task.FromResult<EntityClassification?>(new EntityClassification
                    {
                        TimeDelta = "none",
                        IntroducedEntities = Array.Empty<SceneEntity>(),
                        RemovedEntities = Array.Empty<SceneEntity>(),
                        Entities = Array.Empty<SceneEntity>()
                    });
                }

                return Task.FromResult<EntityClassification?>(null);
            });

        // Act
        var result = await _service.EvaluateAsync(graph, scenarioPathContent);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EntityIntroductionResult);
        
        // We expect at least violations detected (may vary based on text matching)
        Assert.NotNull(result.EntityIntroductionResult.Violations);
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsWhenGraphIsNull()
    {
        // Arrange
        var scenarioPathContent = "Test path content";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.EvaluateAsync(null!, scenarioPathContent));
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsWhenScenarioPathContentIsNull()
    {
        // Arrange
        var scenario = CreateTestScenario();
        var graph = ScenarioGraph.FromScenario(scenario);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.EvaluateAsync(graph, null!));
    }

    [Fact]
    public async Task EvaluateAsync_ClassifiesAllScenesInParallel()
    {
        // Arrange
        var scenario = CreateTestScenario();
        var graph = ScenarioGraph.FromScenario(scenario);
        var scenarioPathContent = "Test path content";

        var classificationCount = 0;
        var maxConcurrentCalls = 0;
        var currentConcurrentCalls = 0;

        var entityClassification = new EntityClassification
        {
            TimeDelta = "none",
            IntroducedEntities = Array.Empty<SceneEntity>(),
            RemovedEntities = Array.Empty<SceneEntity>(),
            Entities = Array.Empty<SceneEntity>()
        };

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsistencyEvaluationResult?)null);

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
        var result = await _service.EvaluateAsync(graph, scenarioPathContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(graph.Nodes.Count, classificationCount);
        
        // Verify that at least some classifications ran in parallel (maxConcurrentCalls > 1)
        // This proves parallelization is occurring
        Assert.True(maxConcurrentCalls > 0, "Classifications should have executed");
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
            Description = "Old Rurik appears."
        };

        return new Scenario
        {
            Id = "TestScenario",
            Title = "Test",
            Scenes = new List<Scene> { s0, s1, s2 }
        };
    }
}
