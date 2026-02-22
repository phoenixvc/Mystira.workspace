using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using Mystira.StoryGenerator.GraphTheory.Algorithms;
using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.GraphTheory.Tests;

public class DominatorPathAlgorithmTests
{
    [Fact]
    public void GetDominatorPaths_ReturnsCorrectPaths()
    {
        // Simple diamond graph:
        // v1 -> v2 -> v4
        // v1 -> v3 -> v4
        // v4 -> v5

        // Dominators:
        // idom(v2) = v1
        // idom(v3) = v1
        // idom(v4) = v1 (common dominator of v2 and v3)
        // idom(v5) = v4

        // Terminal node is v5.
        // Path from root to v5 via idoms:
        // v5 <- v4 <- v1
        // FindShortestPath(v1, v4) = [v1->v2->v4] or [v1->v3->v4]
        // FindShortestPath(v4, v5) = [v4->v5]
        // Resulting path: [v1, v2, v4, v5] (depending on BFS order)

        var scenes = new List<Scene>
        {
            new() { Id = "v1", Description = "Start" },
            new() { Id = "v2", Description = "Left" },
            new() { Id = "v3", Description = "Right" },
            new() { Id = "v4", Description = "Merge" },
            new() { Id = "v5", Description = "End", Type = SceneType.Narrative }
        };

        var scenario = new Scenario { Scenes = scenes };
        // We need to set up NextSceneId or Branches for ToGraph() to work
        scenes[0].Branches = new List<Branch>
        {
            new() { NextSceneId = "v2", Choice = "left" },
            new() { NextSceneId = "v3", Choice = "right" }
        };
        scenes[1].NextSceneId = "v4";
        scenes[2].NextSceneId = "v4";
        scenes[3].NextSceneId = "v5";

        var graph = ScenarioGraph.FromScenario(scenario);
        var paths = graph.GetDominatorPaths(compress: true).ToList();

        // For this simple diamond, we expect 2 dominator paths when compressed:
        // v1 -> v2 -> v4 -> v5
        // v1 -> v3 -> v4
        Assert.Equal(2, paths.Count);

        var path1 = paths.FirstOrDefault(p => p.SceneIds.SequenceEqual(new[] { "v1", "v2", "v4", "v5" }));
        var path2 = paths.FirstOrDefault(p => p.SceneIds.SequenceEqual(new[] { "v1", "v3", "v4" }));

        Assert.NotNull(path1);
        Assert.NotNull(path2);
    }

    [Fact]
    public void GetDominatorPaths_WithCycle_ReturnsCorrectly()
    {
        // Cycle: v1 -> v2 -> v3 -> v2
        //        v1 -> v4
        // Terminal: v4

        var scenes = new List<Scene>
        {
            new() { Id = "v1", Description = "Start" },
            new() { Id = "v2", Description = "Cycle-Start" },
            new() { Id = "v3", Description = "Cycle-End" },
            new() { Id = "v4", Description = "Terminal", Type = SceneType.Narrative }
        };

        scenes[0].Branches = new List<Branch>
        {
            new() { NextSceneId = "v2", Choice = "to-cycle" },
            new() { NextSceneId = "v4", Choice = "to-terminal" }
        };
        scenes[1].NextSceneId = "v3";
        scenes[2].NextSceneId = "v2";

        var scenario = new Scenario { Scenes = scenes };
        var graph = ScenarioGraph.FromScenario(scenario);

        // GetDominatorPaths should handle the cycle.
        // It uses GetImmediateDominators(v1).
        // idom(v2) = v1
        // idom(v3) = v2
        // idom(v4) = v1

        // Terminals are [v4].
        // If no terminals, it uses all nodes. But here v4 is terminal.

        var paths = graph.GetDominatorPaths().ToList();

        // Expected path to v4: v1 -> v4
        Assert.Contains(paths, p => p.SceneIds.SequenceEqual(new[] { "v1", "v4" }));

        // If we want to ensure cycle nodes are covered if they were "terminals" of a sort,
        // ScenarioGraph.GetDominatorPaths has a fallback:
        // if (terminalNodes.Count == 0) terminalNodes = Nodes.ToList();
        // Since we have a terminal (v4), it might only return v1->v4.
    }

    [Theory]
    [InlineData("Test-Minimal.yaml", 3)] // Increased from 1
    [InlineData("Test-Story-6-9.yaml", 26)] // Increased from 6, matches compressed path count if all branches are covered
    public async Task GetDominatorPaths_ReturnsCorrectlyFromScenario(
        string fileName,
        int expectedMinimumPathCount)
    {
        // 1) Load scenario from YAML
        var yamlPath = Path.Combine(AppContext.BaseDirectory, "test_data", fileName);
        Assert.True(File.Exists(yamlPath), $"YAML test data not found at {yamlPath}");

        var yaml = await File.ReadAllTextAsync(yamlPath);

        // 2) Convert to graph
        var scenario = await new ScenarioFactory()
            .CreateFromContentAsync(yaml, ScenarioContentFormat.Yaml);

        var graph = ScenarioGraph.FromScenario(scenario);
        Assert.NotNull(graph);

        // 3) Get Dominator Paths
        var dominatorPaths = graph.GetDominatorPaths(compress: true).ToList();

        // Should produce at least some paths
        Assert.NotEmpty(dominatorPaths);
        Assert.True(dominatorPaths.Count >= expectedMinimumPathCount, $"Expected at least {expectedMinimumPathCount} paths, but got {dominatorPaths.Count}");

        foreach (var path in dominatorPaths)
        {
            Assert.NotEmpty(path.SceneIds);
            Assert.NotEmpty(path.Story);
        }
    }
}
