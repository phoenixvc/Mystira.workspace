using Mystira.StoryGenerator.GraphTheory.Algorithms;
using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.GraphTheory.Tests;

public class CycleHandlingTests
{
    [Fact]
    public void EnumeratePaths_WithCycle_TerminatesAndReturnsPartialPath()
    {
        // v1 -> v2 -> v3 -> v2
        // v1 -> v4
        var edges = new List<Edge<string, string>>
        {
            new("v1", "v2", "next"),
            new("v2", "v3", "next"),
            new("v3", "v2", "cycle"),
            new("v1", "v4", "next"),
        };

        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // v4 is terminal, v2 and v3 are not.
        // With cycle detection, v3 -> v2 will be seen as a cycle and v3 will be treated as terminal for that branch.

        var paths = graph.EnumeratePaths("v1").ToList();

        // Expected paths:
        // 1. [v1, v2, v3] (due to cycle detection at v3 -> v2)
        // 2. [v1, v4] (terminal)

        Assert.Equal(2, paths.Count);
        Assert.Contains(paths, p => p.SequenceEqual(new[] { "v1", "v2", "v3" }));
        Assert.Contains(paths, p => p.SequenceEqual(new[] { "v1", "v4" }));
    }

    [Fact]
    public void GetImmediateDominators_WithCycle_ReturnsCorrectDominators()
    {
        // Example from Dragon Book:
        // 1 -> 2, 3
        // 2 -> 3
        // 3 -> 4
        // 4 -> 3, 5
        // 5 -> 1
        var edges = new List<Edge<int, string>>
        {
            new(1, 2, ""),
            new(1, 3, ""),
            new(2, 3, ""),
            new(3, 4, ""),
            new(4, 3, "back"),
            new(4, 5, ""),
            new(5, 1, "back")
        };

        var graph = DirectedGraph<int, string>.FromEdges(edges);
        var idoms = graph.GetImmediateDominators(1);

        // Dominators:
        // 1 is root, idom[1] = 1
        // 2: only from 1, so idom[2] = 1
        // 3: from 1, 2, 4. 1 dominates 2 and 4 (via 3). Closest common dominator is 1.
        // 4: only from 3, so idom[4] = 3
        // 5: only from 4, so idom[5] = 4

        Assert.Equal(1, idoms[1]);
        Assert.Equal(1, idoms[2]);
        Assert.Equal(1, idoms[3]);
        Assert.Equal(3, idoms[4]);
        Assert.Equal(4, idoms[5]);
    }
}
