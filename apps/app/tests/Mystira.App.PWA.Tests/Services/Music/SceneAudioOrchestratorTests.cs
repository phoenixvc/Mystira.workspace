using FluentAssertions;
using Moq;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.App.PWA.Services.Music;
using Xunit;
using Scene = Mystira.App.PWA.Models.Scene;
using Scenario = Mystira.App.PWA.Models.Scenario;

namespace Mystira.App.PWA.Tests.Services.Music;

public class SceneAudioOrchestratorTests
{
    private readonly Mock<IMusicResolver> _resolverMock;
    private readonly Mock<IAudioBus> _audioBusMock;
    private readonly IAudioStateStore _stateStore;
    private readonly SceneAudioOrchestrator _sut;

    public SceneAudioOrchestratorTests()
    {
        _resolverMock = new Mock<IMusicResolver>();
        _audioBusMock = new Mock<IAudioBus>();
        _stateStore = new AudioStateStore();
        _sut = new SceneAudioOrchestrator(_resolverMock.Object, _audioBusMock.Object, _stateStore);
    }

    [Fact]
    public async Task EnterSceneAsync_ShouldPlayMusic_WhenResolverReturnsTrack()
    {
        // Arrange
        var scene = new Scene();
        var scenario = new Scenario();
        var resolution = new MusicResolutionResult
        {
            TrackId = "new_track",
            Profile = MusicProfile.Cozy,
            Transition = MusicTransitionHint.CrossfadeNormal
        };

        _resolverMock.Setup(x => x.ResolveMusic(It.IsAny<Scene>(), It.IsAny<Scenario>(), It.IsAny<MusicContext>()))
            .Returns(resolution);

        _resolverMock.Setup(x => x.GetEffectiveIntent(It.IsAny<Scene>()))
            .Returns(new SceneMusicSettings { Energy = 1.0 });

        // Act
        await _sut.EnterSceneAsync(scene, scenario);

        // Assert
        _audioBusMock.Verify(x => x.PlayMusicAsync("new_track", MusicTransitionHint.CrossfadeNormal, 1.0f), Times.Once);
    }

    [Fact]
    public async Task EnterSceneAsync_ShouldStopMusic_WhenResolverReturnsSilence()
    {
        // Arrange
        var scene = new Scene();
        var scenario = new Scenario();
        var resolutionSilence = new MusicResolutionResult
        {
            IsSilence = true,
            Transition = MusicTransitionHint.CrossfadeLong
        };

        var trackResolution = new MusicResolutionResult { TrackId = "track1", Profile = MusicProfile.Neutral };

        _resolverMock.SetupSequence(x => x.ResolveMusic(It.IsAny<Scene>(), It.IsAny<Scenario>(), It.IsAny<MusicContext>()))
            .Returns(trackResolution)
            .Returns(resolutionSilence);

        // Required for energy update
        _resolverMock.Setup(x => x.GetEffectiveIntent(It.IsAny<Scene>()))
            .Returns(new SceneMusicSettings());

        // Act 1 (Set state)
        await _sut.EnterSceneAsync(scene, scenario);

        // Act 2 (Silence)
        await _sut.EnterSceneAsync(scene, scenario);

        // Assert
        _audioBusMock.Verify(x => x.StopMusicAsync(MusicTransitionHint.CrossfadeLong), Times.Once);
    }

    [Fact]
    public async Task EnterSceneAsync_ShouldUpdateContextEnergy_WhenTrackChanges()
    {
        // Arrange
        var scene = new Scene();
        var scenario = new Scenario();
        var resolution = new MusicResolutionResult { TrackId = "track1", Profile = MusicProfile.Neutral };
        var intent = new SceneMusicSettings { Energy = 0.8 };

        _resolverMock.Setup(x => x.ResolveMusic(It.IsAny<Scene>(), It.IsAny<Scenario>(), It.IsAny<MusicContext>()))
            .Returns(resolution);
        _resolverMock.Setup(x => x.GetEffectiveIntent(scene)).Returns(intent);

        // Act
        await _sut.EnterSceneAsync(scene, scenario);

        // Assert - We can't easily check private context, but we can check if GetEffectiveIntent was called
        _resolverMock.Verify(x => x.GetEffectiveIntent(scene), Times.AtLeastOnce);
    }

    [Fact]
    public async Task EnterSceneAsync_ShouldUpdateContextEnergy_EvenIfTrackDoesNotChange()
    {
        // Arrange
        var scene = new Scene();
        var scenario = new Scenario();

        // First call to set initial track
        var resolution1 = new MusicResolutionResult { TrackId = "track1", Profile = MusicProfile.Neutral };
        var intent1 = new SceneMusicSettings { Energy = 0.5 };

        // Second call with same track but different energy
        var resolution2 = new MusicResolutionResult { TrackId = "track1", Profile = MusicProfile.Neutral, Transition = MusicTransitionHint.Keep };
        var intent2 = new SceneMusicSettings { Energy = 0.7 };

        _resolverMock.SetupSequence(x => x.ResolveMusic(It.IsAny<Scene>(), It.IsAny<Scenario>(), It.IsAny<MusicContext>()))
            .Returns(resolution1)
            .Returns(resolution2);

        _resolverMock.SetupSequence(x => x.GetEffectiveIntent(scene))
            .Returns(intent1)
            .Returns(intent2);

        // Act
        await _sut.EnterSceneAsync(scene, scenario); // Initial
        await _sut.EnterSceneAsync(scene, scenario); // Update energy

        // Assert
        _resolverMock.Verify(x => x.GetEffectiveIntent(scene), Times.Exactly(2));
        _audioBusMock.Verify(x => x.PlayMusicAsync("track1", It.IsAny<MusicTransitionHint>(), It.IsAny<float>()), Times.Once);
    }

    [Fact]
    public async Task EnterSceneAsync_ShouldForcePlay_WhenCurrentTrackIdIsNull()
    {
        // Arrange
        var scene = new Scene();
        var scenario = new Scenario();
        var resolution = new MusicResolutionResult
        {
            TrackId = "track1",
            Profile = MusicProfile.Neutral,
            Transition = MusicTransitionHint.CrossfadeNormal
        };

        _resolverMock.Setup(x => x.ResolveMusic(It.IsAny<Scene>(), It.IsAny<Scenario>(), It.IsAny<MusicContext>()))
            .Returns(resolution);

        _resolverMock.Setup(x => x.GetEffectiveIntent(It.IsAny<Scene>()))
            .Returns(new SceneMusicSettings { Energy = 0.5 });

        // Act
        // _context.CurrentTrackId is null by default in a new orchestrator
        await _sut.EnterSceneAsync(scene, scenario);

        // Assert
        _audioBusMock.Verify(x => x.PlayMusicAsync("track1", MusicTransitionHint.CrossfadeNormal, 0.5f), Times.Once);
    }

    #region Narration Tests

    [Fact]
    public async Task PlayNarrationAsync_ShouldDuckMusicAndPlaySfx()
    {
        // Act
        await _sut.PlayNarrationAsync("narration-clip.mp3");

        // Assert
        _audioBusMock.Verify(x => x.DuckMusicAsync(true, AudioDefaults.DuckVolume), Times.Once);
        _audioBusMock.Verify(x => x.PlaySoundEffectAsync("narration-clip.mp3", false, 1.0f), Times.Once);
    }

    [Fact]
    public async Task PlayNarrationAsync_WithDuckMusicFalse_ShouldNotDuck()
    {
        // Act
        await _sut.PlayNarrationAsync("clip.mp3", duckMusic: false);

        // Assert
        _audioBusMock.Verify(x => x.DuckMusicAsync(It.IsAny<bool>(), It.IsAny<float>()), Times.Never);
        _audioBusMock.Verify(x => x.PlaySoundEffectAsync("clip.mp3", false, 1.0f), Times.Once);
    }

    [Fact]
    public async Task StopNarrationAsync_ShouldStopSfxAndUnduck()
    {
        // Act
        await _sut.StopNarrationAsync("narration-clip.mp3");

        // Assert
        _audioBusMock.Verify(x => x.StopSoundEffectAsync("narration-clip.mp3"), Times.Once);
        _audioBusMock.Verify(x => x.DuckMusicAsync(false, AudioDefaults.DuckVolume), Times.Once);
    }

    [Fact]
    public async Task StopNarrationAsync_WithUnduckFalse_ShouldNotUnduck()
    {
        // Act
        await _sut.StopNarrationAsync("clip.mp3", unduckMusic: false);

        // Assert
        _audioBusMock.Verify(x => x.StopSoundEffectAsync("clip.mp3"), Times.Once);
        _audioBusMock.Verify(x => x.DuckMusicAsync(It.IsAny<bool>(), It.IsAny<float>()), Times.Never);
    }

    #endregion

    #region Music Pause/Resume Tests

    [Fact]
    public async Task ToggleMusicPauseAsync_WhenPlaying_ShouldPauseAndReturnTrue()
    {
        // Arrange
        _audioBusMock.Setup(x => x.IsMusicPausedAsync()).ReturnsAsync(false);

        // Act
        var result = await _sut.ToggleMusicPauseAsync();

        // Assert
        result.Should().BeTrue();
        _audioBusMock.Verify(x => x.PauseMusicAsync(), Times.Once);
        _audioBusMock.Verify(x => x.ResumeMusicAsync(), Times.Never);
    }

    [Fact]
    public async Task ToggleMusicPauseAsync_WhenPaused_ShouldResumeAndReturnFalse()
    {
        // Arrange
        _audioBusMock.Setup(x => x.IsMusicPausedAsync()).ReturnsAsync(true);

        // Act
        var result = await _sut.ToggleMusicPauseAsync();

        // Assert
        result.Should().BeFalse();
        _audioBusMock.Verify(x => x.ResumeMusicAsync(), Times.Once);
        _audioBusMock.Verify(x => x.PauseMusicAsync(), Times.Never);
    }

    [Fact]
    public async Task IsMusicPausedAsync_ShouldDelegateToAudioBus()
    {
        // Arrange
        _audioBusMock.Setup(x => x.IsMusicPausedAsync()).ReturnsAsync(true);

        // Act
        var result = await _sut.IsMusicPausedAsync();

        // Assert
        result.Should().BeTrue();
        _audioBusMock.Verify(x => x.IsMusicPausedAsync(), Times.Once);
    }

    #endregion

    #region Scene Action Tests

    [Fact]
    public async Task OnSceneActionAsync_ShouldPauseAll_WhenActionIsActive()
    {
        // Act
        await _sut.OnSceneActionAsync(true);

        // Assert
        _audioBusMock.Verify(x => x.PauseAllAsync(), Times.Once);
    }

    [Fact]
    public async Task OnSceneActionAsync_ShouldResumeAll_WhenActionIsInactive()
    {
        // Act
        await _sut.OnSceneActionAsync(false);

        // Assert
        _audioBusMock.Verify(x => x.ResumeAllAsync(), Times.Once);
    }

    #endregion
}
