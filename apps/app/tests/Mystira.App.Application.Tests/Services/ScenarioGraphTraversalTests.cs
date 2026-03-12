using FluentAssertions;
using Mystira.Core.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.Services;

public class ScenarioGraphTraversalTests
{
    [Fact]
    public void TraverseScenario_WithNullScenes_ReturnsEmptyList()
    {
        var scenario = new Scenario { Id = "s1", Scenes = null! };

        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        result.Should().BeEmpty();
    }

    [Fact]
    public void TraverseScenario_WithEmptyScenes_ReturnsEmptyList()
    {
        var scenario = new Scenario { Id = "s1", Scenes = new List<Scene>() };

        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        result.Should().BeEmpty();
    }

    [Fact]
    public void TraverseScenario_WithLinearPath_ReturnsSinglePath()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new() { Id = "scene-1", NextSceneId = "scene-2", Branches = new List<Branch>() },
                new()
                {
                    Id = "scene-2",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "End",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 1 }
                        }
                    }
                }
            }
        };

        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        result.Should().HaveCount(1);
        result[0].Should().ContainKey("courage");
        result[0]["courage"].Should().Be(1.0);
    }

    [Fact]
    public void TraverseScenario_WithBranchingPath_ReturnsMultiplePaths()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Brave path",
                            NextSceneId = "scene-2a",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 2 }
                        },
                        new()
                        {
                            Choice = "Cautious path",
                            NextSceneId = "scene-2b",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = -1 }
                        }
                    }
                },
                new() { Id = "scene-2a", Branches = new List<Branch>() },
                new() { Id = "scene-2b", Branches = new List<Branch>() }
            }
        };

        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void TraverseScenario_WithCycle_DoesNotInfiniteLoop()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Loop back",
                            NextSceneId = "scene-1", // Cycle!
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 1 }
                        }
                    }
                }
            }
        };

        // Should not throw or hang
        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        // BUG-01 fix: Cycle detected but accumulated scores are preserved
        result.Should().HaveCount(1);
        result[0]["courage"].Should().Be(1.0);
    }

    [Fact]
    public void TraverseScenario_WithCycleAfterMultipleScenes_PreservesAccumulatedScores()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Go forward",
                            NextSceneId = "scene-2",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 2 }
                        }
                    }
                },
                new()
                {
                    Id = "scene-2",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Loop back to start",
                            NextSceneId = "scene-1", // Cycle back to scene-1
                            CompassChange = new CompassChange { AxisId = "honesty", Delta = 3 }
                        }
                    }
                }
            }
        };

        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        // Both accumulated scores should be preserved even though cycle is hit
        result.Should().HaveCount(1);
        result[0]["courage"].Should().Be(2.0);
        result[0]["honesty"].Should().Be(3.0);
    }

    [Fact]
    public void TraverseScenario_AccumulatesScoresAcrossMultipleScenes()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Go",
                            NextSceneId = "scene-2",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 1 }
                        }
                    }
                },
                new()
                {
                    Id = "scene-2",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Continue",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 2 }
                        }
                    }
                }
            }
        };

        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        result.Should().HaveCount(1);
        result[0]["courage"].Should().Be(3.0); // 1.0 + 2.0
    }

    [Fact]
    public void TraverseScenario_TracksMultipleAxes()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Go",
                            NextSceneId = "scene-2",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 1 }
                        }
                    }
                },
                new()
                {
                    Id = "scene-2",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "End",
                            CompassChange = new CompassChange { AxisId = "honesty", Delta = 2 }
                        }
                    }
                }
            }
        };

        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        result.Should().HaveCount(1);
        result[0].Should().ContainKey("courage");
        result[0].Should().ContainKey("honesty");
        result[0]["courage"].Should().Be(1.0);
        result[0]["honesty"].Should().Be(2.0);
    }

    [Fact]
    public void TraverseScenario_BranchWithNoCompassChange_StillTraverses()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Go",
                            NextSceneId = "scene-2",
                            CompassChange = null
                        }
                    }
                },
                new()
                {
                    Id = "scene-2",
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "End",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 1 }
                        }
                    }
                }
            }
        };

        var result = ScenarioGraphTraversal.TraverseScenario(scenario);

        result.Should().HaveCount(1);
        result[0]["courage"].Should().Be(1.0);
    }
}
