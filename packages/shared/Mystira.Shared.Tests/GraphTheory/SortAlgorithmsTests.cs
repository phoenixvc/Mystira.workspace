using Mystira.Shared.GraphTheory;
using Mystira.Shared.GraphTheory.Algorithms;
using Xunit;

namespace Mystira.Shared.Tests.GraphTheory;

public class SortAlgorithmsTests
{
    [Fact]
    public void TopologicalSort_ReturnsSortedNodes()
    {
        // Arrange - create a DAG
        // A -> B -> D
        //  \-> C ->/
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("A", "C", "e2"),
            new Edge<string, string>("B", "D", "e3"),
            new Edge<string, string>("C", "D", "e4")
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Act
        var sorted = graph.TopologicalSort().ToList();

        // Assert
        Assert.Equal(4, sorted.Count);

        // A must come before B and C
        Assert.True(sorted.IndexOf("A") < sorted.IndexOf("B"));
        Assert.True(sorted.IndexOf("A") < sorted.IndexOf("C"));

        // B and C must come before D
        Assert.True(sorted.IndexOf("B") < sorted.IndexOf("D"));
        Assert.True(sorted.IndexOf("C") < sorted.IndexOf("D"));
    }

    [Fact]
    public void TopologicalSort_DetectsCycle()
    {
        // Arrange - create a cycle: A -> B -> C -> A
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("B", "C", "e2"),
            new Edge<string, string>("C", "A", "e3") // Creates cycle
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Act & Assert - TopologicalSort throws when graph contains a cycle
        Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort());
    }

    [Fact]
    public void TopologicalSort_HandlesEmptyGraph()
    {
        // Arrange
        var graph = DirectedGraph<string, string>.FromEdges(Array.Empty<Edge<string, string>>());

        // Act
        var sorted = graph.TopologicalSort();

        // Assert
        Assert.Empty(sorted);
    }

    [Fact]
    public void TopologicalSort_HandlesSingleNode()
    {
        // Arrange
        var graph = DirectedGraph<string, string>.FromEdges(
            Array.Empty<Edge<string, string>>(),
            new[] { "A" });

        // Act
        var sorted = graph.TopologicalSort();

        // Assert
        Assert.Single(sorted);
        Assert.Equal("A", sorted.First());
    }

    [Fact]
    public void TopologicalSort_HandlesDisconnectedComponents()
    {
        // Arrange - two separate chains
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("C", "D", "e2")
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Act
        var sorted = graph.TopologicalSort().ToList();

        // Assert
        Assert.Equal(4, sorted.Count);

        // A before B, C before D
        Assert.True(sorted.IndexOf("A") < sorted.IndexOf("B"));
        Assert.True(sorted.IndexOf("C") < sorted.IndexOf("D"));
    }

    [Fact]
    public void HasCycle_ReturnsTrueForCyclicGraph()
    {
        // Arrange
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("B", "C", "e2"),
            new Edge<string, string>("C", "A", "e3") // Creates cycle
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Act & Assert
        Assert.True(graph.HasCycle());
    }

    [Fact]
    public void HasCycle_ReturnsFalseForAcyclicGraph()
    {
        // Arrange
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("B", "C", "e2")
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Act & Assert
        Assert.False(graph.HasCycle());
    }
}
