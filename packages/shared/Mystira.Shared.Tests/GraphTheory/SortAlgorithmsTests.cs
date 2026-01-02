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
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("A", "C", "e2"));
        graph.AddEdge(new Edge<string, string>("B", "D", "e3"));
        graph.AddEdge(new Edge<string, string>("C", "D", "e4"));

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
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("B", "C", "e2"));
        graph.AddEdge(new Edge<string, string>("C", "A", "e3")); // Creates cycle

        // Act & Assert - TopologicalSort throws when graph contains a cycle
        Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort());
    }

    [Fact]
    public void TopologicalSort_HandlesEmptyGraph()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();

        // Act
        var sorted = graph.TopologicalSort();

        // Assert
        Assert.Empty(sorted);
    }

    [Fact]
    public void TopologicalSort_HandlesSingleNode()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();
        graph.AddNode("A");

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
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("C", "D", "e2"));

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
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("B", "C", "e2"));
        graph.AddEdge(new Edge<string, string>("C", "A", "e3")); // Creates cycle

        // Act & Assert
        Assert.True(graph.HasCycle());
    }

    [Fact]
    public void HasCycle_ReturnsFalseForAcyclicGraph()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("B", "C", "e2"));

        // Act & Assert
        Assert.False(graph.HasCycle());
    }
}
