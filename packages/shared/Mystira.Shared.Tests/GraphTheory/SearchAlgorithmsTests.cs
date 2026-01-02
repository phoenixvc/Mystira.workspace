using Mystira.Shared.GraphTheory;
using Mystira.Shared.GraphTheory.Algorithms;
using Xunit;

namespace Mystira.Shared.Tests.GraphTheory;

public class SearchAlgorithmsTests
{
    private DirectedGraph<string, string> CreateSimpleGraph()
    {
        // Create: A -> B -> C -> D
        //              |
        //              v
        //              E
        var edges = new[]
        {
            new Edge<string, string>("A", "B", "e1"),
            new Edge<string, string>("B", "C", "e2"),
            new Edge<string, string>("C", "D", "e3"),
            new Edge<string, string>("B", "E", "e4")
        };
        return DirectedGraph<string, string>.FromEdges(edges);
    }

    [Fact]
    public void BreadthFirstSearch_VisitsNodesInOrder()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var visited = new List<string>();

        // Act
        SearchAlgorithms.BreadthFirstSearch(graph, "A", node =>
        {
            visited.Add(node);
            return true;
        });

        // Assert
        Assert.Equal("A", visited[0]); // Start node first
        Assert.Contains("B", visited);
        Assert.Contains("C", visited);
        Assert.Contains("D", visited);
        Assert.Contains("E", visited);
        // B should be visited before its children C and E
        Assert.True(visited.IndexOf("B") < visited.IndexOf("C"));
        Assert.True(visited.IndexOf("B") < visited.IndexOf("E"));
    }

    [Fact]
    public void DepthFirstSearch_VisitsAllNodes()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var visited = new List<string>();

        // Act
        SearchAlgorithms.DepthFirstSearch(graph, "A", node =>
        {
            visited.Add(node);
            return true;
        });

        // Assert
        Assert.Equal(5, visited.Count);
        Assert.Equal("A", visited[0]); // Start node first
        Assert.Contains("B", visited);
        Assert.Contains("C", visited);
        Assert.Contains("D", visited);
        Assert.Contains("E", visited);
    }

    [Fact]
    public void BreadthFirstSearch_StopsWhenVisitorReturnsFalse()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var visited = new List<string>();

        // Act - stop after visiting B
        SearchAlgorithms.BreadthFirstSearch(graph, "A", node =>
        {
            visited.Add(node);
            return node != "B";
        });

        // Assert
        Assert.Equal(2, visited.Count); // Only A and B
    }

    [Fact]
    public void DepthFirstSearch_StopsWhenVisitorReturnsFalse()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var visited = new List<string>();

        // Act - stop after visiting B
        SearchAlgorithms.DepthFirstSearch(graph, "A", node =>
        {
            visited.Add(node);
            return node != "B";
        });

        // Assert
        Assert.Equal(2, visited.Count); // Only A and B
    }

    [Fact]
    public void BreadthFirstSearch_WithInvalidStart_DoesNothing()
    {
        // Arrange
        var graph = CreateSimpleGraph();
        var visited = new List<string>();

        // Act
        SearchAlgorithms.BreadthFirstSearch(graph, "nonexistent", node =>
        {
            visited.Add(node);
            return true;
        });

        // Assert
        Assert.Empty(visited);
    }
}
