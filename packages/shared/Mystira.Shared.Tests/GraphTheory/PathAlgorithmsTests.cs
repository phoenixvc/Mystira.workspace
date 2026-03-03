using Mystira.Shared.GraphTheory;
using Mystira.Shared.GraphTheory.Algorithms;
using Xunit;

namespace Mystira.Shared.Tests.GraphTheory;

public class PathAlgorithmsTests
{
    private DirectedGraph<string, string> CreateDiamondGraph()
    {
        // Create diamond: A -> B -> D
        //                  \-> C ->/
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("A", "C", "e2"),
            new Edge<string, string>("B", "D", "e3"),
            new Edge<string, string>("C", "D", "e4")
        };
        return DirectedGraph<string, string>.FromEdges(edges);
    }

    [Fact]
    public void EnumeratePaths_FindsAllPathsToTerminal()
    {
        // Arrange
        var graph = CreateDiamondGraph();

        // Act - enumerate paths from A to terminal node D
        var paths = graph.EnumeratePaths("A", node => node == "D").ToList();

        // Assert
        Assert.Equal(2, paths.Count);
        Assert.Contains(paths, p => p.SequenceEqual(new[] { "A", "B", "D" }));
        Assert.Contains(paths, p => p.SequenceEqual(new[] { "A", "C", "D" }));
    }

    [Fact]
    public void EnumeratePaths_ReturnsEmptyWhenStartNotInGraph()
    {
        // Arrange
        var graph = CreateDiamondGraph();

        // Act - start from nonexistent node
        var paths = graph.EnumeratePaths("Z", node => node == "D").ToList();

        // Assert
        Assert.Empty(paths);
    }

    [Fact]
    public void EnumeratePaths_FindsPathWhenStartIsTerminal()
    {
        // Arrange
        var graph = CreateDiamondGraph();

        // Act - A is both start and terminal
        var paths = graph.EnumeratePaths("A", node => node == "A").ToList();

        // Assert
        Assert.Single(paths);
        Assert.Equal(new[] { "A" }, paths[0]);
    }

    [Fact]
    public void EnumeratePaths_RespectsMaxDepth()
    {
        // Arrange
        var graph = CreateDiamondGraph();

        // Act - limit depth to 1 (only A -> B or A -> C)
        var paths = graph.EnumeratePaths("A", maxDepth: 1).ToList();

        // Assert - all paths should be length 2 (A plus one neighbor)
        Assert.All(paths, p => Assert.Equal(2, p.Count));
    }

    [Fact]
    public void EnumeratePaths_HandlesLinearPath()
    {
        // Arrange
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("B", "C", "e2"),
            new Edge<string, string>("C", "D", "e3")
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Act - D has no outgoing edges so it's naturally terminal
        var paths = graph.EnumeratePaths("A").ToList();

        // Assert
        Assert.Single(paths);
        Assert.Equal(new[] { "A", "B", "C", "D" }, paths[0]);
    }

    [Fact]
    public void EnumeratePaths_HandlesComplexGraph()
    {
        // Arrange - multiple branching paths
        var edges = new[]
        {
            new Edge<string, string>("start", "a", "e1"),
            new Edge<string, string>("start", "b", "e2"),
            new Edge<string, string>("a", "c", "e3"),
            new Edge<string, string>("a", "d", "e4"),
            new Edge<string, string>("b", "c", "e5"),
            new Edge<string, string>("c", "end", "e6"),
            new Edge<string, string>("d", "end", "e7")
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Act - enumerate paths to terminal node "end"
        var paths = graph.EnumeratePaths("start", node => node == "end").ToList();

        // Assert
        Assert.Equal(3, paths.Count);
        // start -> a -> c -> end
        // start -> a -> d -> end
        // start -> b -> c -> end
    }
}
