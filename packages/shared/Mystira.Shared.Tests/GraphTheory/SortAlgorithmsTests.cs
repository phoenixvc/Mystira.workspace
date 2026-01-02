using Mystira.Shared.GraphTheory;

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
        var result = SortAlgorithms.TopologicalSort(graph);

        // Assert
        Assert.True(result.IsAcyclic);
        var sorted = result.SortedNodes.ToList();
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

        // Act
        var result = SortAlgorithms.TopologicalSort(graph);

        // Assert
        Assert.False(result.IsAcyclic);
    }

    [Fact]
    public void TopologicalSort_HandlesEmptyGraph()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();

        // Act
        var result = SortAlgorithms.TopologicalSort(graph);

        // Assert
        Assert.True(result.IsAcyclic);
        Assert.Empty(result.SortedNodes);
    }

    [Fact]
    public void TopologicalSort_HandlesSingleNode()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();
        graph.AddNode("A");

        // Act
        var result = SortAlgorithms.TopologicalSort(graph);

        // Assert
        Assert.True(result.IsAcyclic);
        Assert.Single(result.SortedNodes);
        Assert.Equal("A", result.SortedNodes.First());
    }

    [Fact]
    public void TopologicalSort_HandlesDisconnectedComponents()
    {
        // Arrange - two separate chains
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("C", "D", "e2"));

        // Act
        var result = SortAlgorithms.TopologicalSort(graph);

        // Assert
        Assert.True(result.IsAcyclic);
        var sorted = result.SortedNodes.ToList();
        Assert.Equal(4, sorted.Count);

        // A before B, C before D
        Assert.True(sorted.IndexOf("A") < sorted.IndexOf("B"));
        Assert.True(sorted.IndexOf("C") < sorted.IndexOf("D"));
    }
}
