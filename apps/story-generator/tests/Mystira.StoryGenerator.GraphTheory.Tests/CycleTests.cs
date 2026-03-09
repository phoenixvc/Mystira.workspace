using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Domain.Stories;
using Mystira.StoryGenerator.GraphTheory.Algorithms;
using Mystira.StoryGenerator.GraphTheory.Graph;
using Xunit;

namespace Mystira.StoryGenerator.GraphTheory.Tests;

public class CycleTests
{
    [Fact]
    public void EnumeratePaths_WithCycle_ShouldNotInfiniteLoop()
    {
        // v1 -> v2 -> v3 -> v2 (cycle)
        // v3 -> v4 (terminal)
        var edges = new List<Edge<string, string>>
        {
            new("v1", "v2", "next"),
            new("v2", "v3", "next"),
            new("v3", "v2", "loop"),
            new("v3", "v4", "end"),
        };

        var graph = DirectedGraph<string, string>.FromEdges(edges);

        var paths = graph.EnumeratePaths("v1", node => node == "v4").ToList();

        Assert.NotEmpty(paths);
        // If it returns, we check if it found the path v1, v2, v3, v4
        Assert.Contains(paths, p => p.SequenceEqual(new[] { "v1", "v2", "v3", "v4" }));
    }

    [Fact]
    public void GetDominatorPaths_WithCycle_ShouldNotStackOverflow()
    {
        // v1 -> v2 -> v3 -> v2 (cycle)
        // v1 -> v4 (terminal)
        var scenes = new List<Scene>
        {
            new() { Id = "v1", Description = "Start" },
            new() { Id = "v2", Description = "Cycle 1" },
            new() { Id = "v3", Description = "Cycle 2" },
            new() { Id = "v4", Description = "End" }
        };

        scenes[0].Branches = new List<Branch>
        {
            new() { NextSceneId = "v2", Choice = "to-cycle" },
            new() { NextSceneId = "v4", Choice = "to-end" }
        };
        scenes[1].NextSceneId = "v3";
        scenes[2].NextSceneId = "v2";

        var scenario = new Scenario { Scenes = scenes };
        var graph = ScenarioGraph.FromScenario(scenario);

        // This should not stack overflow
        var paths = graph.GetDominatorPaths(compress: true).ToList();

        Assert.NotEmpty(paths);
        Assert.Contains(paths, p => p.SceneIds.Contains("v4"));
    }

    [Fact]
    public void GetDominatorPaths_WithSelfLoop_ShouldNotStackOverflow()
    {
        // v1 -> v1 (self loop)
        var scenes = new List<Scene>
        {
            new() { Id = "v1", Description = "Start", NextSceneId = "v1" }
        };

        var scenario = new Scenario { Scenes = scenes };
        var graph = ScenarioGraph.FromScenario(scenario);

        // This should not stack overflow and should return at least one path
        var paths = graph.GetDominatorPaths(compress: true).ToList();

        Assert.NotEmpty(paths);
        Assert.Contains(paths, p => p.SceneIds.Contains("v1"));
    }

    [Fact]
    public void GetDominatorPaths_PureCycle_ShouldCoverAllNodes()
    {
        // v1 -> v2 -> v1 (pure cycle)
        var scenes = new List<Scene>
        {
            new() { Id = "v1", Description = "Scene 1", NextSceneId = "v2" },
            new() { Id = "v2", Description = "Scene 2", NextSceneId = "v1" }
        };

        var scenario = new Scenario { Scenes = scenes };
        var graph = ScenarioGraph.FromScenario(scenario);

        var paths = graph.GetDominatorPaths(compress: true).ToList();

        Assert.NotEmpty(paths);
        // Should contain the cycle excluding the terminal
        Assert.Equal(paths[0].SceneIds.ToArray(), new [] { "v1", "v2" });
        // Should contain all nodes in the cycle
        var allSceneIds = paths.SelectMany(p => p.SceneIds).ToHashSet();
        Assert.Contains("v1", allSceneIds);
        Assert.Contains("v2", allSceneIds);
    }
}
