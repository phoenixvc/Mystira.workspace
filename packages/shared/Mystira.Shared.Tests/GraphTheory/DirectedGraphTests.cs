using Mystira.Shared.GraphTheory;
using Xunit;

namespace Mystira.Shared.Tests.GraphTheory;

public class DirectedGraphTests
{
    [Fact]
    public void AddEdge_AddsNodeAndEdge()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();
        var edge = new Edge<string, string>("A", "B", "edge1");

        // Act
        graph.AddEdge(edge);

        // Assert
        Assert.Contains("A", graph.Nodes);
        Assert.Contains("B", graph.Nodes);
        var outEdges = graph.GetOutgoingEdges("A").ToList();
        Assert.Single(outEdges);
        Assert.Equal("B", outEdges[0].Target);
    }

    [Fact]
    public void AddNode_AddsNodeWithoutEdges()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();

        // Act
        graph.AddNode("A");

        // Assert
        Assert.Contains("A", graph.Nodes);
        Assert.Empty(graph.GetOutgoingEdges("A"));
    }

    [Fact]
    public void GetOutgoingEdges_ReturnsEmptyForUnknownNode()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();

        // Act
        var edges = graph.GetOutgoingEdges("nonexistent");

        // Assert
        Assert.Empty(edges);
    }

    [Fact]
    public void GetIncomingEdges_ReturnsCorrectEdges()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "C", "e1"));
        graph.AddEdge(new Edge<string, string>("B", "C", "e2"));

        // Act
        var incoming = graph.GetIncomingEdges("C").ToList();

        // Assert
        Assert.Equal(2, incoming.Count);
        Assert.Contains(incoming, e => e.Source == "A");
        Assert.Contains(incoming, e => e.Source == "B");
    }

    [Fact]
    public void Nodes_ReturnsAllNodes()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();
        graph.AddEdge(new Edge<string, string>("A", "B", "e1"));
        graph.AddEdge(new Edge<string, string>("B", "C", "e2"));
        graph.AddNode("D");

        // Act
        var nodes = graph.Nodes.ToList();

        // Assert
        Assert.Equal(4, nodes.Count);
        Assert.Contains("A", nodes);
        Assert.Contains("B", nodes);
        Assert.Contains("C", nodes);
        Assert.Contains("D", nodes);
    }

    [Fact]
    public void ContainsNode_ReturnsTrueForExistingNode()
    {
        // Arrange
        var graph = new DirectedGraph<string, string>();
        graph.AddNode("A");

        // Act & Assert
        Assert.True(graph.ContainsNode("A"));
        Assert.False(graph.ContainsNode("B"));
    }
}
