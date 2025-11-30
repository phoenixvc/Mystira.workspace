using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Tests;

public class ScenarioFactoryTests
{
    private readonly ScenarioFactory _factory = new();

    [Fact]
    public async Task CreateFromContentAsync_Json_Success()
    {
        var json = "{" +
                   "\"title\":\"Test Story\"," +
                   "\"description\":\"Desc\"," +
                   "\"tags\":[\"mystery\"]," +
                   "\"difficulty\":\"Easy\"," +
                   "\"session_length\":\"Short\"," +
                   "\"age_group\":\"6-9\"," +
                   "\"minimum_age\":6," +
                   "\"core_axes\":[\"Axis\"]," +
                   "\"archetypes\":[\"the_innovator\"]," +
                   "\"characters\":[]," +
                   "\"scenes\":[{" +
                   "\"id\":\"scene_1_start\",\"title\":\"Start\",\"type\":\"narrative\",\"description\":\"begin\",\"next_scene\":\"scene_2\"},{" +
                   "\"id\":\"scene_2\",\"title\":\"End\",\"type\":\"narrative\",\"description\":\"end\"}" +
                   "]}";

        var scenario = await _factory.CreateFromContentAsync(json, ScenarioContentFormat.Json);
        Assert.NotNull(scenario);
        Assert.Equal("Test Story", scenario.Title);
        Assert.Equal("Desc", scenario.Description);
        Assert.Equal(2, scenario.Scenes.Count);
        Assert.Equal("scene_1_start", scenario.Scenes[0].Id);
        Assert.Equal("scene_2", scenario.Scenes[0].NextSceneId);
    }

    [Fact]
    public async Task CreateFromContentAsync_Empty_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _factory.CreateFromContentAsync(string.Empty, ScenarioContentFormat.Json));
    }

    [Fact]
    public async Task CreateFromContentAsync_MalformedJson_ThrowsInvalidOperationException()
    {
        var badJson = "{"; // malformed JSON
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _factory.CreateFromContentAsync(badJson, ScenarioContentFormat.Json));
        Assert.Contains("Failed to create Scenario from Json content", ex.Message);
    }

    [Fact]
    public async Task CreateFromContentAsync_Yaml_Success()
    {
        var yaml =
            "title: Test Story\n" +
            "description: Desc\n" +
            "tags:\n" +
            "  - mystery\n" +
            "difficulty: Easy\n" +
            "session_length: Short\n" +
            "age_group: 6-9\n" +
            "minimum_age: 6\n" +
            "core_axes:\n" +
            "  - Axis\n" +
            "archetypes:\n" +
            "  - the_innovator\n" +
            "characters: []\n" +
            "scenes:\n" +
            "  - id: scene_1_start\n" +
            "    title: Start\n" +
            "    type: narrative\n" +
            "    description: begin\n" +
            "    next_scene: scene_2\n" +
            "  - id: scene_2\n" +
            "    title: End\n" +
            "    type: narrative\n" +
            "    description: end\n";

        var scenario = await _factory.CreateFromContentAsync(yaml, ScenarioContentFormat.Yaml);
        Assert.NotNull(scenario);
        Assert.Equal("Test Story", scenario.Title);
        Assert.Equal("Desc", scenario.Description);
        Assert.Equal(2, scenario.Scenes.Count);
        Assert.Equal("scene_1_start", scenario.Scenes[0].Id);
        Assert.Equal("scene_2", scenario.Scenes[0].NextSceneId);
    }
}
