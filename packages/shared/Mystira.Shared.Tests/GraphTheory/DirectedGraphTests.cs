using Mystira.Shared.GraphTheory;
using Xunit;

namespace Mystira.Shared.Tests.GraphTheory;

public class DirectedGraphTests
{
    [Fact]
    public void FromEdges_CreatesGraphWithNodesAndEdges()
    {
        // Arrange
        var edges = new[] { new Edge<string, string>("A", "B", "edge1") };

        // Act
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Assert
        Assert.Contains("A", graph.Nodes);
        Assert.Contains("B", graph.Nodes);
        var outEdges = graph.GetOutgoingEdges("A").ToList();
        Assert.Single(outEdges);
        Assert.Equal("B", outEdges[0].To);
    }

    [Fact]
    public void FromEdges_WithExplicitNodes_IncludesIsolatedNodes()
    {
        // Arrange
        var edges = Array.Empty<Edge<string, string>>();
        var nodes = new[] { "A" };

        // Act
        var graph = DirectedGraph<string, string>.FromEdges(edges, nodes);

        // Assert
        Assert.Contains("A", graph.Nodes);
        Assert.Empty(graph.GetOutgoingEdges("A"));
    }

    [Fact]
    public void GetOutgoingEdges_ReturnsEmptyForUnknownNode()
    {
        // Arrange
        var graph = DirectedGraph<string, string>.FromEdges(Array.Empty<Edge<string, string>>());

        // Act
        var edges = graph.GetOutgoingEdges("nonexistent");

        // Assert
        Assert.Empty(edges);
    }

    [Fact]
    public void GetIncomingEdges_ReturnsCorrectEdges()
    {
        // Arrange
        var edges = new[]
        {
            new Edge<string, string>("A", "C", "e1"),
            new Edge<string, string>("B", "C", "e2")
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges);

        // Act
        var incoming = graph.GetIncomingEdges("C").ToList();

        // Assert
        Assert.Equal(2, incoming.Count);
        Assert.Contains(incoming, e => e.From == "A");
        Assert.Contains(incoming, e => e.From == "B");
    }

    [Fact]
    public void Nodes_ReturnsAllNodes()
    {
        // Arrange
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("B", "C", "e2")
        };
        var graph = DirectedGraph<string, string>.FromEdges(edges, new[] { "D" });

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
    public void Nodes_Contains_ReturnsTrueForExistingNode()
    {
        // Arrange
        var graph = DirectedGraph<string, string>.FromEdges(
            Array.Empty<Edge<string, string>>(),
            new[] { "A" });

        // Act & Assert
        Assert.Contains("A", graph.Nodes);
        Assert.DoesNotContain("B", graph.Nodes);
    }
}
