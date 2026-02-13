using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases;

public class ProgressSceneUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IScenarioRepository> _scenarioRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<ProgressSceneUseCase>> _logger;
    private readonly ProgressSceneUseCase _useCase;

    public ProgressSceneUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _scenarioRepository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<ProgressSceneUseCase>>();
        _useCase = new ProgressSceneUseCase(
            _repository.Object, _scenarioRepository.Object,
            _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ProgressesToScene()
    {
        var session = CreateTestSession(SessionStatus.InProgress);
        var scenario = CreateTestScenario();
        SetupRepositories(session, scenario);

        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "scene-2" };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result!.CurrentSceneId.Should().Be("scene-2");
        _repository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithPausedSession_ResumesAndProgresses()
    {
        var session = CreateTestSession(SessionStatus.Paused);
        session.IsPaused = true;
        session.PausedAt = DateTime.UtcNow.AddMinutes(-5);
        var scenario = CreateTestScenario();
        SetupRepositories(session, scenario);

        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "scene-2" };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result!.Status.Should().Be(SessionStatus.InProgress);
        result.IsPaused.Should().BeFalse();
        result.PausedAt.Should().BeNull();
        result.CurrentSceneId.Should().Be("scene-2");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentSession_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var request = new ProgressSceneRequest { SessionId = "missing", SceneId = "scene-1" };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithCompletedSession_ThrowsInvalidOperationException()
    {
        var session = CreateTestSession(SessionStatus.Completed);
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "scene-1" };

        var act = () => _useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingScenario_ThrowsInvalidOperationException()
    {
        var session = CreateTestSession(SessionStatus.InProgress);
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "scene-1" };

        var act = () => _useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidSceneId_ThrowsArgumentException()
    {
        var session = CreateTestSession(SessionStatus.InProgress);
        var scenario = CreateTestScenario();
        SetupRepositories(session, scenario);

        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "nonexistent-scene" };

        var act = () => _useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesElapsedTime()
    {
        var session = CreateTestSession(SessionStatus.InProgress);
        session.StartTime = DateTime.UtcNow.AddMinutes(-15);
        var scenario = CreateTestScenario();
        SetupRepositories(session, scenario);

        var request = new ProgressSceneRequest { SessionId = "session-1", SceneId = "scene-2" };

        var result = await _useCase.ExecuteAsync(request);

        result!.ElapsedTime.Should().NotBeNull();
        result.ElapsedTime!.Value.TotalMinutes.Should().BeApproximately(15, 1);
    }

    private void SetupRepositories(GameSession session, Scenario scenario)
    {
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
    }

    private static GameSession CreateTestSession(SessionStatus status)
    {
        return new GameSession
        {
            Id = "session-1",
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            Status = status,
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            CurrentSceneId = "scene-1",
            PlayerNames = new List<string> { "Player1" },
            ChoiceHistory = new List<SessionChoice>(),
            EchoHistory = new List<EchoLog>(),
            Achievements = new List<SessionAchievement>(),
            CompassValues = new Dictionary<string, CompassTracking>()
        };
    }

    private static Scenario CreateTestScenario()
    {
        return new Scenario
        {
            Id = "scenario-1",
            Title = "Test Scenario",
            Scenes = new List<Scene>
            {
                new() { Id = "scene-1", Title = "Scene 1", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() },
                new() { Id = "scene-2", Title = "Scene 2", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() }
            },
            CoreAxes = new List<CoreAxis>()
        };
    }
}
