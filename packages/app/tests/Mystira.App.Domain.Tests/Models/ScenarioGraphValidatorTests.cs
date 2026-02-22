using Mystira.App.Domain.Models;
using FluentAssertions;

namespace Mystira.App.Domain.Tests.Models;

public class ScenarioGraphValidatorTests
{
    private readonly ScenarioGraphValidator _validator = new();

    #region Valid Scenarios

    [Fact]
    public void ValidateGraph_WithEmptyScenes_ReturnsTrue()
    {
        // Arrange
        var scenario = new Scenario
        {
            Id = "test",
            Title = "Test Scenario",
            Scenes = new List<Scene>()
        };

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGraph_WithNullScenes_ReturnsTrue()
    {
        // Arrange
        var scenario = new Scenario
        {
            Id = "test",
            Title = "Test Scenario",
            Scenes = null!
        };

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGraph_WithSingleScene_ReturnsTrue()
    {
        // Arrange
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGraph_WithLinearPath_ReturnsTrue()
    {
        // Arrange - Scene 1 -> Scene 2 -> Scene 3 (end)
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene { Id = "scene2", Title = "Scene 2", NextSceneId = "scene3" },
            new Scene { Id = "scene3", Title = "Scene 3" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGraph_WithBranchingPaths_ReturnsTrue()
    {
        // Arrange - Scene 1 branches to Scene 2A or Scene 2B, both lead to Scene 3
        var scenario = CreateScenarioWithScenes(
            new Scene
            {
                Id = "scene1",
                Title = "Scene 1",
                Branches = new List<Branch>
                {
                    new Branch { Choice = "Option A", NextSceneId = "scene2a" },
                    new Branch { Choice = "Option B", NextSceneId = "scene2b" }
                }
            },
            new Scene { Id = "scene2a", Title = "Scene 2A", NextSceneId = "scene3" },
            new Scene { Id = "scene2b", Title = "Scene 2B", NextSceneId = "scene3" },
            new Scene { Id = "scene3", Title = "Scene 3" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGraph_WithMultipleEndings_ReturnsTrue()
    {
        // Arrange - Scene 1 branches to Scene 2A (good ending) or Scene 2B (bad ending)
        var scenario = CreateScenarioWithScenes(
            new Scene
            {
                Id = "scene1",
                Title = "Scene 1",
                Branches = new List<Branch>
                {
                    new Branch { Choice = "Be kind", NextSceneId = "goodEnd" },
                    new Branch { Choice = "Be cruel", NextSceneId = "badEnd" }
                }
            },
            new Scene { Id = "goodEnd", Title = "Good Ending" },
            new Scene { Id = "badEnd", Title = "Bad Ending" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    #endregion

    #region Cycle Detection

    [Fact]
    public void ValidateGraph_WithSimpleCycle_DetectsInfiniteLoop()
    {
        // Arrange - Scene 1 -> Scene 2 -> Scene 1 (cycle!)
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene { Id = "scene2", Title = "Scene 2", NextSceneId = "scene1" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Infinite loop"));
    }

    [Fact]
    public void ValidateGraph_WithSelfLoop_DetectsInfiniteLoop()
    {
        // Arrange - Scene 1 -> Scene 1 (self-loop)
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene1" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Infinite loop"));
    }

    [Fact]
    public void ValidateGraph_WithBranchCycle_DetectsInfiniteLoop()
    {
        // Arrange - Scene 1 -> Scene 2, but Scene 2 has a branch back to Scene 1
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene
            {
                Id = "scene2",
                Title = "Scene 2",
                Branches = new List<Branch>
                {
                    new Branch { Choice = "Continue", NextSceneId = "scene3" },
                    new Branch { Choice = "Go back", NextSceneId = "scene1" }
                }
            },
            new Scene { Id = "scene3", Title = "Scene 3" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Infinite loop"));
    }

    [Fact]
    public void ValidateGraph_WithLongCycle_DetectsInfiniteLoop()
    {
        // Arrange - Scene 1 -> 2 -> 3 -> 4 -> 1 (long cycle)
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene { Id = "scene2", Title = "Scene 2", NextSceneId = "scene3" },
            new Scene { Id = "scene3", Title = "Scene 3", NextSceneId = "scene4" },
            new Scene { Id = "scene4", Title = "Scene 4", NextSceneId = "scene1" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("Infinite loop"));
    }

    #endregion

    #region Unreachable Scene Detection

    [Fact]
    public void ValidateGraph_WithUnreachableScene_ReportsError()
    {
        // Arrange - Scene 1 -> Scene 2, but Scene 3 has no path to it
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene { Id = "scene2", Title = "Scene 2" },
            new Scene { Id = "scene3", Title = "Orphan Scene" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().ContainSingle(e => e.Contains("unreachable") && e.Contains("Orphan Scene"));
    }

    [Fact]
    public void ValidateGraph_WithMultipleUnreachableScenes_ReportsAllErrors()
    {
        // Arrange - Only Scene 1 is reachable
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1" },
            new Scene { Id = "orphan1", Title = "Orphan 1" },
            new Scene { Id = "orphan2", Title = "Orphan 2" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().HaveCount(2);
        errors.Should().Contain(e => e.Contains("Orphan 1"));
        errors.Should().Contain(e => e.Contains("Orphan 2"));
    }

    [Fact]
    public void ValidateGraph_WithDisconnectedSubgraph_ReportsUnreachableScenes()
    {
        // Arrange - Scene 1 -> 2, Scene 3 -> 4 (disconnected)
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene { Id = "scene2", Title = "Scene 2" },
            new Scene { Id = "scene3", Title = "Scene 3", NextSceneId = "scene4" },
            new Scene { Id = "scene4", Title = "Scene 4" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Scene 3") && e.Contains("unreachable"));
        errors.Should().Contain(e => e.Contains("Scene 4") && e.Contains("unreachable"));
    }

    #endregion

    #region Combined Issues

    [Fact]
    public void ValidateGraph_WithCycleAndUnreachable_ReportsBothErrors()
    {
        // Arrange - Scene 1 <-> Scene 2 (cycle), Scene 3 is unreachable
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene { Id = "scene2", Title = "Scene 2", NextSceneId = "scene1" },
            new Scene { Id = "scene3", Title = "Orphan Scene" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Infinite loop"));
        errors.Should().Contain(e => e.Contains("unreachable"));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ValidateGraph_WithEmptyNextSceneId_DoesNotCrash()
    {
        // Arrange
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "" },
            new Scene { Id = "scene2", Title = "Scene 2" }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        // Scene 2 is unreachable, but should not crash
        result.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("unreachable"));
    }

    [Fact]
    public void ValidateGraph_WithNullBranches_DoesNotCrash()
    {
        // Arrange
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", Branches = null! }
        );

        // Act
        var act = () => _validator.ValidateGraph(scenario, out _);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateGraph_WithEmptyBranchNextSceneId_HandlesGracefully()
    {
        // Arrange - Branch with empty NextSceneId (story ending)
        var scenario = CreateScenarioWithScenes(
            new Scene
            {
                Id = "scene1",
                Title = "Scene 1",
                Branches = new List<Branch>
                {
                    new Branch { Choice = "The End", NextSceneId = "" }
                }
            }
        );

        // Act
        var result = _validator.ValidateGraph(scenario, out var errors);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Scenario Integration Tests

    [Fact]
    public void Scenario_ValidateGraphIntegrity_UsesValidator()
    {
        // Arrange - Valid scenario
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene { Id = "scene2", Title = "Scene 2" }
        );

        // Act
        var result = scenario.ValidateGraphIntegrity(out var errors);

        // Assert
        result.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Scenario_ValidateGraphIntegrity_WithCycle_ReturnsFalse()
    {
        // Arrange - Scenario with cycle
        var scenario = CreateScenarioWithScenes(
            new Scene { Id = "scene1", Title = "Scene 1", NextSceneId = "scene2" },
            new Scene { Id = "scene2", Title = "Scene 2", NextSceneId = "scene1" }
        );

        // Act
        var result = scenario.ValidateGraphIntegrity(out var errors);

        // Assert
        result.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    #endregion

    #region Helpers

    private static Scenario CreateScenarioWithScenes(params Scene[] scenes)
    {
        return new Scenario
        {
            Id = "test-scenario",
            Title = "Test Scenario",
            Scenes = scenes.ToList()
        };
    }

    #endregion
}
