using Mystira.Shared.GraphTheory;

namespace Mystira.Shared.Tests.GraphTheory;

public class PathAlgorithmsTests
{
    private DirectedGraph<string, string> CreateDiamondGraph()
    {
        // Create diamond: A -> B -> D
        //                  \-> C ->/
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("A", "C", "e2"));
        graph.AddEdge(new Edge<string, string>("B", "D", "e3"));
        graph.AddEdge(new Edge<string, string>("C", "D", "e4"));
        return graph;
    }

    [Fact]
    public void EnumerateAllPaths_FindsAllPathsInDiamond()
    {
        // Arrange
        var graph = CreateDiamondGraph();

        // Act
        var paths = PathAlgorithms.EnumerateAllPaths(graph, "A", "D").ToList();

        // Assert
        Assert.Equal(2, paths.Count);
        Assert.Contains(paths, p => p.SequenceEqual(new[] { "A", "B", "D" }));
        Assert.Contains(paths, p => p.SequenceEqual(new[] { "A", "C", "D" }));
    }

    [Fact]
    public void EnumerateAllPaths_ReturnsEmptyWhenNoPath()
    {
        // Arrange
        var graph = CreateDiamondGraph();

        // Act
        var paths = PathAlgorithms.EnumerateAllPaths(graph, "D", "A").ToList();

        // Assert
        Assert.Empty(paths);
    }

    [Fact]
    public void EnumerateAllPaths_ReturnsPathWithSameStartAndEnd()
    {
        // Arrange
        var graph = CreateDiamondGraph();

        // Act
        var paths = PathAlgorithms.EnumerateAllPaths(graph, "A", "A").ToList();

        // Assert
        Assert.Single(paths);
        Assert.Equal(new[] { "A" }, paths[0]);
    }

    [Fact]
    public void EnumerateAllPaths_RespectsMaxPaths()
    {
        // Arrange
        var graph = CreateDiamondGraph();

        // Act
        var paths = PathAlgorithms.EnumerateAllPaths(graph, "A", "D", maxPaths: 1).ToList();

        // Assert
        Assert.Single(paths);
    }

    [Fact]
    public void EnumerateAllPaths_HandlesLinearPath()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("B", "C", "e2"));
        graph.AddEdge(new Edge<string, string>("C", "D", "e3"));

        // Act
        var paths = PathAlgorithms.EnumerateAllPaths(graph, "A", "D").ToList();

        // Assert
        Assert.Single(paths);
        Assert.Equal(new[] { "A", "B", "C", "D" }, paths[0]);
    }

    [Fact]
    public void EnumerateAllPaths_HandlesComplexGraph()
    {
        // Arrange - multiple branching paths
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("start", "a", "e1"));
        graph.AddEdge(new Edge<string, string>("start", "b", "e2"));
        graph.AddEdge(new Edge<string, string>("a", "c", "e3"));
        graph.AddEdge(new Edge<string, string>("a", "d", "e4"));
        graph.AddEdge(new Edge<string, string>("b", "c", "e5"));
        graph.AddEdge(new Edge<string, string>("c", "end", "e6"));
        graph.AddEdge(new Edge<string, string>("d", "end", "e7"));

        // Act
        var paths = PathAlgorithms.EnumerateAllPaths(graph, "start", "end").ToList();

        // Assert
        Assert.Equal(3, paths.Count);
        // start -> a -> c -> end
        // start -> a -> d -> end
        // start -> b -> c -> end
    }
}
