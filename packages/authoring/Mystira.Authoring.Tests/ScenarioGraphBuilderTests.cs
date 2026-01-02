using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Authoring.Graph;

namespace Mystira.Authoring.Tests;

public class ScenarioGraphBuilderTests
{
    private Scenario CreateSimpleScenario()
    {
        return new Scenario
        {
            Id = "test-scenario",
            Title = "Test Scenario",
            Scenes = new List<Scene>
            {
                new Scene { Id = "start", Title = "Start", Type = SceneType.Narrative, NextSceneId = "choice1" },
                new Scene
                {
                    Id = "choice1",
                    Title = "First Choice",
                    Type = SceneType.Choice,
                    Branches = new List<Branch>
                    {
                        new Branch { Choice = "Option A", NextSceneId = "path_a" },
                        new Branch { Choice = "Option B", NextSceneId = "path_b" }
                    }
                },
                new Scene { Id = "path_a", Title = "Path A", Type = SceneType.Narrative, NextSceneId = "ending" },
                new Scene { Id = "path_b", Title = "Path B", Type = SceneType.Narrative, NextSceneId = "ending" },
                new Scene { Id = "ending", Title = "The End", Type = SceneType.Special }
            }
        };
    }

    [Fact]
    public void Build_CreatesGraphWithAllScenes()
    {
        // Arrange
        var scenario = CreateSimpleScenario();
        var builder = new ScenarioGraphBuilder();

        // Act
        var graph = builder.Build(scenario);

        // Assert
        Assert.Contains("start", graph.Nodes);
        Assert.Contains("choice1", graph.Nodes);
        Assert.Contains("path_a", graph.Nodes);
        Assert.Contains("path_b", graph.Nodes);
        Assert.Contains("ending", graph.Nodes);
    }

    [Fact]
    public void Build_CreatesCorrectEdges()
    {
        // Arrange
        var scenario = CreateSimpleScenario();
        var builder = new ScenarioGraphBuilder();

        // Act
        var graph = builder.Build(scenario);

        // Assert
        var startEdges = graph.GetOutgoingEdges("start").ToList();
        Assert.Single(startEdges);
        Assert.Equal("choice1", startEdges[0].Target);

        var choiceEdges = graph.GetOutgoingEdges("choice1").ToList();
        Assert.Equal(2, choiceEdges.Count);
        Assert.Contains(choiceEdges, e => e.Target == "path_a");
        Assert.Contains(choiceEdges, e => e.Target == "path_b");
    }

    [Fact]
    public void FindStartScene_ReturnsCorrectScene()
    {
        // Arrange
        var scenario = CreateSimpleScenario();
        var builder = new ScenarioGraphBuilder();

        // Act
        var start = builder.FindStartScene(scenario);

        // Assert
        Assert.Equal("start", start);
    }

    [Fact]
    public void FindEndingScenes_ReturnsAllEndings()
    {
        // Arrange
        var scenario = CreateSimpleScenario();
        var builder = new ScenarioGraphBuilder();

        // Act
        var endings = builder.FindEndingScenes(scenario);

        // Assert
        Assert.Single(endings);
        Assert.Contains("ending", endings);
    }

    [Fact]
    public void FindEndingScenes_HandlesMultipleEndings()
    {
        // Arrange
        var scenario = new Scenario
        {
            Id = "multi-ending",
            Scenes = new List<Scene>
            {
                new Scene { Id = "start", Type = SceneType.Choice, Branches = new List<Branch>
                {
                    new Branch { NextSceneId = "good_end" },
                    new Branch { NextSceneId = "bad_end" }
                }},
                new Scene { Id = "good_end", Type = SceneType.Special },
                new Scene { Id = "bad_end", Type = SceneType.Special }
            }
        };
        var builder = new ScenarioGraphBuilder();

        // Act
        var endings = builder.FindEndingScenes(scenario);

        // Assert
        Assert.Equal(2, endings.Count);
        Assert.Contains("good_end", endings);
        Assert.Contains("bad_end", endings);
    }

    [Fact]
    public void EnumerateAllPaths_FindsAllPathsThroughScenario()
    {
        // Arrange
        var scenario = CreateSimpleScenario();
        var builder = new ScenarioGraphBuilder();

        // Act
        var paths = builder.EnumerateAllPaths(scenario);

        // Assert
        Assert.Equal(2, paths.Count);
        Assert.Contains(paths, p => p.Contains("path_a"));
        Assert.Contains(paths, p => p.Contains("path_b"));
    }

    [Fact]
    public void EnumerateAllPaths_ReturnsEmptyForEmptyScenario()
    {
        // Arrange
        var scenario = new Scenario { Id = "empty", Scenes = new List<Scene>() };
        var builder = new ScenarioGraphBuilder();

        // Act
        var paths = builder.EnumerateAllPaths(scenario);

        // Assert
        Assert.Empty(paths);
    }
}
