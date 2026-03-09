using FluentAssertions;
using Moq;
using Mystira.App.PWA.Models;
using Mystira.App.PWA.Services;
using Xunit;
using Scene = Mystira.App.PWA.Models.Scene;

namespace Mystira.App.PWA.Tests.Services;

public class SceneMediaResolverTests
{
    private readonly Mock<IApiClient> _apiClientMock = new();

    [Fact]
    public async Task ResolveMediaUrlsAsync_WhenMediaIsNull_DoesNotCallApiClient()
    {
        var scene = new Scene { Media = null };

        await scene.ResolveMediaUrlsAsync(_apiClientMock.Object);

        _apiClientMock.Verify(
            c => c.GetMediaUrlFromId(It.IsAny<string>()),
            Times.Never);
        scene.AudioUrl.Should().BeNull();
        scene.ImageUrl.Should().BeNull();
        scene.VideoUrl.Should().BeNull();
    }

    [Fact]
    public async Task ResolveMediaUrlsAsync_ResolvesAllThreeMediaTypes()
    {
        var scene = new Scene
        {
            Media = new SceneMedia
            {
                Audio = "audio-id-1",
                Image = "image-id-2",
                Video = "video-id-3"
            }
        };

        _apiClientMock.Setup(c => c.GetMediaUrlFromId("audio-id-1"))
            .ReturnsAsync("https://cdn.example.com/audio.mp3");
        _apiClientMock.Setup(c => c.GetMediaUrlFromId("image-id-2"))
            .ReturnsAsync("https://cdn.example.com/image.jpg");
        _apiClientMock.Setup(c => c.GetMediaUrlFromId("video-id-3"))
            .ReturnsAsync("https://cdn.example.com/video.mp4");

        await scene.ResolveMediaUrlsAsync(_apiClientMock.Object);

        scene.AudioUrl.Should().Be("https://cdn.example.com/audio.mp3");
        scene.ImageUrl.Should().Be("https://cdn.example.com/image.jpg");
        scene.VideoUrl.Should().Be("https://cdn.example.com/video.mp4");
    }

    [Fact]
    public async Task ResolveMediaUrlsAsync_SkipsEmptyMediaIds()
    {
        var scene = new Scene
        {
            Media = new SceneMedia
            {
                Audio = "",
                Image = null,
                Video = "video-id"
            }
        };

        _apiClientMock.Setup(c => c.GetMediaUrlFromId("video-id"))
            .ReturnsAsync("https://cdn.example.com/video.mp4");

        await scene.ResolveMediaUrlsAsync(_apiClientMock.Object);

        scene.AudioUrl.Should().BeNull();
        scene.ImageUrl.Should().BeNull();
        scene.VideoUrl.Should().Be("https://cdn.example.com/video.mp4");
        _apiClientMock.Verify(c => c.GetMediaUrlFromId(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ResolveMediaUrlsAsync_WhenAllMediaIdsEmpty_DoesNotCallApi()
    {
        var scene = new Scene
        {
            Media = new SceneMedia
            {
                Audio = "",
                Image = "",
                Video = ""
            }
        };

        await scene.ResolveMediaUrlsAsync(_apiClientMock.Object);

        _apiClientMock.Verify(
            c => c.GetMediaUrlFromId(It.IsAny<string>()),
            Times.Never);
        scene.AudioUrl.Should().BeNull();
        scene.ImageUrl.Should().BeNull();
        scene.VideoUrl.Should().BeNull();
    }

    [Fact]
    public async Task ResolveMediaUrlsAsync_OnlyAudio_ResolvesAudioOnly()
    {
        var scene = new Scene
        {
            Media = new SceneMedia { Audio = "audio-only" }
        };

        _apiClientMock.Setup(c => c.GetMediaUrlFromId("audio-only"))
            .ReturnsAsync("https://cdn.example.com/narration.mp3");

        await scene.ResolveMediaUrlsAsync(_apiClientMock.Object);

        scene.AudioUrl.Should().Be("https://cdn.example.com/narration.mp3");
        scene.ImageUrl.Should().BeNull();
        scene.VideoUrl.Should().BeNull();
    }
}
