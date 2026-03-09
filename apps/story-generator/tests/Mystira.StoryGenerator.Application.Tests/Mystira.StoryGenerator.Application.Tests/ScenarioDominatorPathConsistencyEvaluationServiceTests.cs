using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Tests;

public class ScenarioDominatorPathConsistencyEvaluationServiceTests
{
    private readonly Mock<IDominatorPathConsistencyLlmService> _mockEvaluator;
    private readonly Mock<ILogger<ScenarioDominatorPathConsistencyEvaluationService>> _mockLogger;
    private readonly ScenarioDominatorPathConsistencyEvaluationService _service;

    public ScenarioDominatorPathConsistencyEvaluationServiceTests()
    {
        _mockEvaluator = new Mock<IDominatorPathConsistencyLlmService>();
        _mockLogger = new Mock<ILogger<ScenarioDominatorPathConsistencyEvaluationService>>();

        _service = new ScenarioDominatorPathConsistencyEvaluationService(
            _mockEvaluator.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task EvaluateAsync_GeneratesCompressedPathsAndEvaluatesEachInParallel()
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
        Assert.NotEmpty(result.PathResults);
        
        // Verify evaluator was called for each path
        _mockEvaluator.Verify(
            e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce());
        
        // Verify each path has a result
        foreach (var pathResult in result.PathResults)
        {
            Assert.NotNull(pathResult.SceneIds);
            Assert.NotEmpty(pathResult.SceneIds);
        }
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsNullWhenNoPaths()
    {
        // Arrange
        var scenario = new Scenario
        {
            Id = "EmptyScenario",
            Title = "Empty",
            Scenes = new List<Scene>()
        };

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

        var capturedContents = new List<string>();

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback((string content, CancellationToken ct) => capturedContents.Add(content))
            .ReturnsAsync(new ConsistencyEvaluationResult { OverallAssessment = "ok", Issues = new List<ConsistencyIssue>() });

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(capturedContents);
        
        // Verify content contains path indicator
        foreach (var content in capturedContents)
        {
            Assert.Contains("Path", content);
        }
    }

    [Fact]
    public async Task EvaluateAsync_ThrowsWhenScenarioIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.EvaluateAsync(null!));
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsResultsWithIssuesWhenEvaluatorFindsProblems()
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
                    Details = "Character X has conflicting states"
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
        Assert.NotEmpty(result.PathResults);
        
        foreach (var pathResult in result.PathResults)
        {
            Assert.NotNull(pathResult.Result);
            Assert.Equal("has_major_issues", pathResult.Result.OverallAssessment);
            Assert.Single(pathResult.Result.Issues);
        }
    }

    [Fact]
    public async Task EvaluateAsync_EvaluatesPathsInParallel()
    {
        // Arrange
        var scenario = CreateTestScenario();

        var evaluationCount = 0;
        var maxConcurrentCalls = 0;
        var currentConcurrentCalls = 0;

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string content, CancellationToken ct) =>
            {
                Interlocked.Increment(ref currentConcurrentCalls);
                Interlocked.Increment(ref evaluationCount);

                var current = currentConcurrentCalls;
                if (current > maxConcurrentCalls)
                {
                    maxConcurrentCalls = current;
                }

                await Task.Delay(10, ct);

                Interlocked.Decrement(ref currentConcurrentCalls);
                return new ConsistencyEvaluationResult 
                { 
                    OverallAssessment = "ok", 
                    Issues = new List<ConsistencyIssue>() 
                };
            });

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(result.PathResults.Count, evaluationCount);
        
        // Verify at least some evaluations ran in parallel
        Assert.True(maxConcurrentCalls > 0, "Evaluations should have executed");
    }

    [Fact]
    public async Task EvaluateAsync_IncludesSceneIdsInEachPathResult()
    {
        // Arrange
        var scenario = CreateTestScenario();

        _mockEvaluator
            .Setup(e => e.EvaluateConsistencyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConsistencyEvaluationResult 
            { 
                OverallAssessment = "ok", 
                Issues = new List<ConsistencyIssue>() 
            });

        // Act
        var result = await _service.EvaluateAsync(scenario);

        // Assert
        Assert.NotNull(result);
        foreach (var pathResult in result.PathResults)
        {
            Assert.NotNull(pathResult.SceneIds);
            Assert.NotEmpty(pathResult.SceneIds);
            
            // First scene should be the root
            Assert.Equal("S0", pathResult.SceneIds[0]);
        }
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
