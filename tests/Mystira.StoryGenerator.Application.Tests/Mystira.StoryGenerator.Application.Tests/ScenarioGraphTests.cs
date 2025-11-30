using Mystira.StoryGenerator.Application.Graph;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Tests;

public class ScenarioGraphTests
{
    [Fact]
    public async Task GetCompressedPaths_ReturnsCorrectlyCompressedPaths()
    {
        // 1) Load scenario from YAML
        var yamlPath = Path.Combine(AppContext.BaseDirectory, "test_data", "Test-Story-6-9.yaml");
        Assert.True(File.Exists(yamlPath), $"YAML test data not found at {yamlPath}");

        var yaml = await File.ReadAllTextAsync(yamlPath);

        // 2) Convert to graph
        var scenario = await new ScenarioFactory()
            .CreateFromContentAsync(yaml, ScenarioContentFormat.Yaml);
        var graph = ScenarioGraph.FromScenario(scenario);

        // 3) Get compressed paths
        var paths = graph.GetCompressedPaths();
        Assert.NotEmpty(paths);
    }
}
