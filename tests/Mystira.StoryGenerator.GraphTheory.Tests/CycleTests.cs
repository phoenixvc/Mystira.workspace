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

        // This is expected to hang or stack overflow currently
        var paths = graph.EnumeratePaths("v1", node => node == "v4").ToList();

        Assert.NotEmpty(paths);
        // If it returns, we check if it found the path v1, v2, v3, v4
        Assert.Contains(paths, p => p.SequenceEqual(new[] { "v1", "v2", "v3", "v4" }));
    }
}
