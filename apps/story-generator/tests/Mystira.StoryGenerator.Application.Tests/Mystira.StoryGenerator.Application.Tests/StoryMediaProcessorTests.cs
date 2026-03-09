using System.Text.Json;
using Mystira.StoryGenerator.Application.Services;
using Xunit;

namespace Mystira.StoryGenerator.Application.Tests;

public class StoryMediaProcessorTests
{
    private readonly StoryMediaProcessor _processor;

    public StoryMediaProcessorTests()
    {
        _processor = new StoryMediaProcessor();
    }

    [Fact]
    public void ProcessMediaIds_ShouldApplyCorrectFormatting()
    {
        // Arrange
        var inputJson = @"
{
  ""scenes"": [
    {
      ""id"": ""start_scene"",
      ""next_scene"": ""choice_scene"",
      ""media"": {
        ""image"": ""old_image.png"",
        ""audio"": ""old_audio.wav""
      }
    },
    {
      ""id"": ""choice_scene"",
      ""branches"": [
        { ""next_scene"": ""outcome_a"" },
        { ""next_scene"": ""outcome_b"" }
      ],
      ""media"": {
        ""image"": ""choice.png""
      }
    },
    {
      ""id"": ""outcome_a"",
      ""media"": {
        ""image"": ""a.png"",
        ""video"": ""a_vid.mp4""
      }
    },
    {
      ""id"": ""outcome_b"",
      ""media"": {
        ""audio"": ""b_audio.mp3""
      }
    }
  ]
}";

        // Act
        var resultJson = _processor.ProcessMediaIds(inputJson);
        var doc = JsonDocument.Parse(resultJson);
        var scenes = doc.RootElement.GetProperty("scenes");

        // Assert
        // Scene 1: depth 1, choice a
        var s1Media = scenes[0].GetProperty("media");
        Assert.Equal("1a_start_scene.webp", s1Media.GetProperty("image").GetString());
        Assert.Equal("1a_start_scene.mp3", s1Media.GetProperty("audio").GetString());

        // Scene 2: depth 2, choice a
        var s2Media = scenes[1].GetProperty("media");
        Assert.Equal("2a_choice_scene.webp", s2Media.GetProperty("image").GetString());

        // Scene 3: depth 3, choice a (outcome_a)
        var s3Media = scenes[2].GetProperty("media");
        Assert.Equal("3a_outcome_a.webp", s3Media.GetProperty("image").GetString());
        Assert.Equal("3a_outcome_a.mp3", s3Media.GetProperty("video").GetString());

        // Scene 4: depth 3, choice b (outcome_b)
        var s4Media = scenes[3].GetProperty("media");
        Assert.Equal("3b_outcome_b.mp3", s4Media.GetProperty("audio").GetString());
    }

    [Fact]
    public void ProcessMediaIds_ShouldHandleNullMedia()
    {
        var inputJson = @"
{
  ""scenes"": [
    {
      ""id"": ""start"",
      ""media"": null
    }
  ]
}";
        var resultJson = _processor.ProcessMediaIds(inputJson);
        Assert.Contains("\"media\": null", resultJson);
    }

    [Fact]
    public void ProcessMediaIds_ShouldHandleEmptyMedia()
    {
        var inputJson = @"
{
  ""scenes"": [
    {
      ""id"": ""start"",
      ""media"": {}
    }
  ]
}";
        var resultJson = _processor.ProcessMediaIds(inputJson);
        Assert.Contains("\"media\": {}", resultJson);
    }
}
