using Mystira.StoryGenerator.GraphTheory.Algorithms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    public void CompressBySharedSuffixes_ReturnsCorrectlyFromScenario()
    {
        // 1) Load scenario from YAML
        var yamlPath = Path.Combine(AppContext.BaseDirectory, "test_data", "Test-Story-6-9.yaml");
        Assert.True(File.Exists(yamlPath), $"YAML test data not found at {yamlPath}");

        var deserializer = new DeserializerBuilder()
            // YAML uses snake_case (e.g., next_scene), so use UnderscoredNamingConvention
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var yaml = File.ReadAllText(yamlPath);
        var scenario = deserializer.Deserialize<YamlScenario>(yaml);
        Assert.NotNull(scenario);
        Assert.NotNull(scenario!.Scenes);
        Assert.NotEmpty(scenario.Scenes);

        // 2) Build map of scenes and enumerate all paths by following next/branches
        var byId = scenario.Scenes.ToDictionary(s => s.Id);
        Assert.True(byId.ContainsKey("scene_1_start"));

        var paths = new List<IReadOnlyList<string>>();
        EnumeratePathsFrom("scene_1_start", new List<string>(), byId, paths);
        Assert.NotEmpty(paths);

        // 3) Compress
        var compressed = PathAlgorithms.CompressBySharedSuffixes(paths);

        // Sanity: ensure all paths start with the initial scene
        Assert.All(compressed, p => Assert.Equal("scene_1_start", p[0]));

        // Compression should not increase the number of paths
        Assert.True(compressed.Count <= paths.Count);

        // Should produce at least one compressed path
        Assert.NotEmpty(compressed);
    }

    private static void EnumeratePathsFrom(
        string current,
        List<string> prefix,
        Dictionary<string, YamlScene> scenes,
        List<IReadOnlyList<string>> results)
    {
        if (!scenes.TryGetValue(current, out var scene))
            return;

        prefix.Add(scene.Id);

        // Determine successors based on type
        var successors = new List<string>();
        if (!string.IsNullOrWhiteSpace(scene.NextScene))
            successors.Add(scene.NextScene!);

        if (scene.Branches is { Count: > 0 })
        {
            foreach (var b in scene.Branches)
            {
                if (!string.IsNullOrWhiteSpace(b.NextScene))
                    successors.Add(b.NextScene!);
            }
        }

        if (successors.Count == 0)
        {
            // terminal
            results.Add(prefix.ToArray());
            prefix.RemoveAt(prefix.Count - 1);
            return;
        }

        foreach (var succ in successors)
        {
            EnumeratePathsFrom(succ, prefix, scenes, results);
        }

        prefix.RemoveAt(prefix.Count - 1);
    }

    // Minimal DTOs matching YAML structure we need for traversal
    private sealed class YamlScenario
    {
        public List<YamlScene> Scenes { get; set; } = new();
    }

    private sealed class YamlScene
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // narrative | choice | roll
        public string? NextScene { get; set; }
        public List<YamlBranch>? Branches { get; set; }
    }

    private sealed class YamlBranch
    {
        public string? Choice { get; set; }
        public string? NextScene { get; set; }
    }
}
