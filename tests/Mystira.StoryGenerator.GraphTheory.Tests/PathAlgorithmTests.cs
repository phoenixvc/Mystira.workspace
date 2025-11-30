using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using Mystira.StoryGenerator.GraphTheory.Algorithms;
using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.GraphTheory.Tests;

public class PathAlgorithmTests
{
    [Fact]
    public void CompressBySharedSuffixes_ReturnsCorrectlyCompressedPaths()
    {
        var simpleGraphPaths = new List<IReadOnlyList<string>>
        {
            new[] { "v1", "v2", "v4", "v5" }, // p1
            new[] { "v1", "v3", "v4", "v5" }  // p2
        };

        var compressed = PathAlgorithms.CompressBySharedSuffixes(simpleGraphPaths);
        Assert.Equal(2, compressed.Count);
        Assert.Equal(["v1", "v2", "v4", "v5"], compressed[0]);
        Assert.Equal(["v1", "v3", "v4"], compressed[1]);

        var complexGraphPaths = new List<IReadOnlyList<string>>
        {
            new[] { "v1", "v2", "v5" }, // p1
            new[] { "v1", "v3", "v5" },  // p2
            new[] { "v1", "v3", "v6", "v7" }, //p3
            new[] { "v1", "v3", "v6", "v8" }, //p4
            new[] { "v1", "v4", "v6", "v7" }, //p5
            new[] { "v1", "v4", "v6", "v8" }, //p6
        };

        compressed = PathAlgorithms.CompressBySharedSuffixes(complexGraphPaths);
        Assert.Equal(5, compressed.Count);
        Assert.Equal(["v1", "v2", "v5"], compressed[0]);
        Assert.Equal(["v1", "v3", "v5"], compressed[1]);
        Assert.Equal(["v1", "v3", "v6", "v7"], compressed[2]);
        Assert.Equal(["v1", "v3", "v6", "v8"], compressed[3]);
        Assert.Equal(["v1", "v4", "v6"], compressed[4]);
    }

    [Fact]
    public void CompressBySharedSuffixes_WithEmptyPaths_ReturnsEmptyList()
    {
        var compressed = PathAlgorithms.CompressBySharedSuffixes(new List<IReadOnlyList<string>>());
        Assert.Empty(compressed);
    }

    [Fact]
    public void CompressGraphPathsToEdgePaths_ReturnsCorrectlyCompressedPaths()
    {
        // graph paths:
        // v1 -> v2 -> v4 -> v5
        // v1 -> v3 -> v4 -> v5
        var edges = new List<Edge<string, string>>
        {
            new("v1", "v2", ""),
            new("v1", "v3", ""),
            new("v2", "v4", ""),
            new("v3", "v4", ""),
            new("v4", "v5", ""),
        };

        var graph = DirectedGraph<string, string>.FromEdges(edges);
        var compressed = graph.CompressGraphPathsToEdgePaths(edges[0].From, scene => scene == edges[4].To);

        // Compare as node sequences, ignoring any empty edge paths (which can
        // occur if compression truncates a path to a single node)
        var nodeSequences = compressed
            .Select(EdgeListToNodeSequence)
            .Where(seq => seq.Count > 0)
            .ToList();

        // should return
        // path 1: v1 -> v2 -> v4 -> v5
        // path 2: v1 -> v3 -> v4
        var expected1 = new List<string> { "v1", "v2", "v4", "v5" };
        var expected2 = new List<string> { "v1", "v3", "v4" };
        bool SeqEq(IReadOnlyList<string> a, IReadOnlyList<string> b)
            => a.Count == b.Count && a.Zip(b).All(p => p.First == p.Second);

        Assert.Contains(nodeSequences, s => SeqEq(s, expected1));
        Assert.Contains(nodeSequences, s => SeqEq(s, expected2));
    }

    private static List<string> EdgeListToNodeSequence(IReadOnlyList<Edge<string, string>> edgePath)
    {
        if (edgePath.Count == 0)
        {
            return new List<string>();
        }

        var nodes = new List<string> { edgePath[0].From };
        foreach (var e in edgePath)
        {
            nodes.Add(e.To);
        }
        return nodes;
    }

    public class PathAlgorithmsTests
    {
        [Theory]
        [InlineData("Test-Minimal.yaml", 5)]
        [InlineData("Test-Story-6-9.yaml", 26)]
        public async Task CompressGraphPathsToEdgePaths_ReturnsCorrectlyFromScenario(
            string fileName,
            int expectedPathCount)
        {
            // 1) Load scenario from YAML
            var yamlPath = Path.Combine(AppContext.BaseDirectory, "test_data", fileName);
            Assert.True(File.Exists(yamlPath), $"YAML test data not found at {yamlPath}");

            var yaml = await File.ReadAllTextAsync(yamlPath);

            // 2) Convert to graph
            var scenario = await new ScenarioFactory()
                .CreateFromContentAsync(yaml, ScenarioContentFormat.Yaml);

            var graph = ScenarioGraph.FromScenario(scenario);
            Assert.NotNull(graph);

            var roots = graph.Roots().ToArray();
            Assert.Single(roots);
            var startingScene = roots[0];

            // 3) Compress
            var compressed = graph.CompressGraphPathsToEdgePaths(startingScene,
                scene => scene.IsFinalScene());

            // Compression should not increase the number of paths
            Assert.Equal(expectedPathCount, compressed.Count);

            // Should produce at least one compressed path
            Assert.NotEmpty(compressed);
        }
    }
}
